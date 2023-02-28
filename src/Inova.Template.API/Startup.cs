using AutoMapper;
using HealthChecks.UI.Client;
using HealthChecks.UI.Core;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.IdentityModel.Logging;
using Microsoft.Net.Http.Headers;
using NSwag;
using NSwag.Generation.Processors.Security;
using Polly;
using Polly.CircuitBreaker;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json.Serialization;
using Inova.Template.API.Extensions;
using Inova.Template.API.Filters;
using Inova.Template.API.Middlewares;
using Inova.Template.API.Services;
using Inova.Template.API.Services.Interfaces;
using Inova.Template.API.Settings;
using Inova.Template.Domain.Interfaces.Identity;
using Inova.Template.Domain.Interfaces.Notifications;
using Inova.Template.Domain.Interfaces.Repository;
using Inova.Template.Domain.Interfaces.Services;
using Inova.Template.Domain.Interfaces.UoW;
using Inova.Template.Domain.Notifications;
using Inova.Template.Infra.Context;
using Inova.Template.Infra.Identity;
using Inova.Template.Infra.Repository;
using Inova.Template.Infra.Services;
using Inova.Template.Infra.UoW;
using Microsoft.IdentityModel.Tokens;

namespace Inova.Template.API;

public class Startup
{
    public Startup(IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
    {
        Configuration = configuration;
        WebHostEnvironment = webHostEnvironment;
    }

    public IConfiguration Configuration { get; }
    public IWebHostEnvironment WebHostEnvironment { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        IdentityModelEventSource.ShowPII = true;
        services.Configure<KestrelServerOptions>(options =>
        {
            options.AllowSynchronousIO = true;
        });

        services.Configure<IISServerOptions>(options =>
        {
            options.AllowSynchronousIO = true;
        });

        services.AddControllers();
        services.AddMvc(options =>
        {
            options.Filters.Add<DomainNotificationFilter>();
            options.EnableEndpointRouting = false;
        }).AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        });
        services.AddAuthentication(options =>
        {
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
        {
            options.RequireHttpsMetadata = bool.Parse(Configuration["Authentication:RequireHttpsMetadata"]);
            options.Authority = Configuration["Authentication:Authority"];
            options.IncludeErrorDetails = bool.Parse(Configuration["Authentication:IncludeErrorDetails"]);
            options.TokenValidationParameters = new TokenValidationParameters()
            {
                ValidateAudience = bool.Parse(Configuration["Authentication:ValidateAudience"]),
                ValidAudience = Configuration["Authentication:ValidAudience"],
                ValidateIssuerSigningKey = bool.Parse(Configuration["Authentication:ValidateIssuerSigningKey"]),
                ValidateIssuer = bool.Parse(Configuration["Authentication:ValidateIssuer"]),
                ValidIssuer = Configuration["Authentication:ValidIssuer"],
                ValidateLifetime = bool.Parse(Configuration["Authentication:ValidateLifetime"])
            };
        });
        services.Configure<TelemetryConfiguration>((o) =>
        {
            o.TelemetryInitializers.Add(new OperationCorrelationTelemetryInitializer());
        });
        services.Configure<GzipCompressionProviderOptions>(x => x.Level = CompressionLevel.Optimal);
        services.AddResponseCompression(x =>
        {
            x.Providers.Add<GzipCompressionProvider>();
        });

        this.RegisterHttpClient(services);

        if (PlatformServices.Default.Application.ApplicationName != "testhost")
        {
            var healthCheck = services.AddHealthChecksUI(setupSettings: setup =>
            {
                setup.DisableDatabaseMigrations();
                setup.MaximumHistoryEntriesPerEndpoint(6);
                setup.AddWebhookNotification("Teams", Configuration["Webhook:Teams"],
                    payload: File.ReadAllText(Path.Combine(".", "MessageCard", "ServiceDown.json")),
                    restorePayload: File.ReadAllText(Path.Combine(".", "MessageCard", "ServiceRestore.json")),
                    customMessageFunc: (str, report) =>
                        {
                            var failing = report.Entries.Where(e => e.Value.Status == UIHealthStatus.Unhealthy);
                            return $"{AppDomain.CurrentDomain.FriendlyName}: {failing.Count()} healthchecks are failing";
                        }
                    );
            }).AddInMemoryStorage();

            var builder = healthCheck.Services.AddHealthChecks();

            //500 mb
            builder.AddProcessAllocatedMemoryHealthCheck(500 * 1024 * 1024, "Process Memory", tags: new[] { "self" });
            //500 mb
            builder.AddPrivateMemoryHealthCheck(1500 * 1024 * 1024, "Private memory", tags: new[] { "self" });

            builder.AddSqlServer(Configuration["ConnectionStrings:CustomerDB"], tags: new[] { "services" });

            //dotnet add <Project> package AspNetCore.HealthChecks.OpenIdConnectServer
            builder.AddIdentityServer(new Uri(Configuration["Authentication:Authority"]), "SSO Inova", tags: new[] { "services" });

            builder.AddApplicationInsightsPublisher();
        }

        if (!WebHostEnvironment.IsProduction())
        {
            services.AddOpenApiDocument(document =>
            {
                document.DocumentName = "v1";
                document.Version = "v1";
                document.Title = "Template API";
                document.Description = "API de Template";
                document.OperationProcessors.Add(new OperationSecurityScopeProcessor("JWT"));
                document.AddSecurity("JWT", Enumerable.Empty<string>(), new OpenApiSecurityScheme
                {
                    Type = OpenApiSecuritySchemeType.ApiKey,
                    Name = HeaderNames.Authorization,
                    Description = "Token de autenticação via SSO",
                    In = OpenApiSecurityApiKeyLocation.Header
                });

                document.PostProcess = (configure) =>
                {
                    configure.Info.TermsOfService = "None";
                    configure.Info.Contact = new OpenApiContact()
                    {
                        Name = "Squad",
                        Email = "squad@xyz.com",
                        Url = "exemplo.xyz.com"
                    };
                    configure.Info.License = new OpenApiLicense()
                    {
                        Name = "Exemplo",
                        Url = "exemplo.xyz.com"
                    };
                };


            });
        }

        services.AddAutoMapper(typeof(Startup));
        services.AddHttpContextAccessor();
        services.AddApplicationInsightsTelemetry();

        this.RegisterServices(services);
        this.RegisterDatabaseServices(services);
    }

    public virtual void Configure(IApplicationBuilder app, IWebHostEnvironment env, TelemetryClient telemetryClient)
    {
        if (!env.IsProduction())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseHsts();
        }

        app.UseRouting();
        app.UseHttpsRedirection();
        app.UseResponseCompression();

        if (!env.IsProduction())
        {
            app.UseOpenApi();
            app.UseSwaggerUi3();
        }

        app.UseAuthentication();
        app.UseAuthorization();
        app.UseLogMiddleware();

        app.UseExceptionHandler(new ExceptionHandlerOptions
        {
            ExceptionHandler = new ErrorHandlerMiddleware(telemetryClient, env).Invoke
        });

        app.UseEndpoints(endpoints =>
        {
            if (PlatformServices.Default.Application.ApplicationName != "testhost")
            {
                endpoints.MapHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("self"),
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });
                endpoints.MapHealthChecks("/ready", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("services"),
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });
                endpoints.MapHealthChecksUI(setup =>
                {
                    setup.UIPath = "/health-ui";
                });
            }

            endpoints.MapControllers();
        });
    }

    private void RegisterHttpClient(IServiceCollection services)
    {
        services.AddHttpClient<IViaCEPService, ViaCEPService>((s, c) =>
                    {
                        c.BaseAddress = new Uri(Configuration["API:ViaCEP"]);
                        c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    })
            .AddPolicyHandler(GetRetryPolicy())
            .AddPolicyHandler(GetCircuitBreakerPolicy());
    }

    protected virtual void RegisterServices(IServiceCollection services)
    {
        services.Configure<ApplicationInsightsSettings>(Configuration.GetSection("ApplicationInsights"));

        #region Service
        services.AddScoped<ICustomerService, CustomerService>();

        #endregion

        #region Domain

        services.AddScoped<IDomainNotification, DomainNotification>();

        #endregion

        #region Infra

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IIdentityService, IdentityService>();

        #endregion
    }

    protected virtual void RegisterDatabaseServices(IServiceCollection services)
    {
        // if (PlatformServices.Default.Application.ApplicationName != "testhost")
        // {
        services.AddDbContext<EntityContext>(options =>
            options.UseSqlServer(Configuration.GetConnectionString("CustomerDB")));
        services.AddSingleton<DbConnection>(conn => new SqlConnection(Configuration.GetConnectionString("CustomerDB")));
        services.AddScoped<DapperContext>();
        // }
    }

    const string SleepDurationKey = "Broken";
    static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return Policy<HttpResponseMessage>
                .HandleResult(res => res.StatusCode == HttpStatusCode.GatewayTimeout || res.StatusCode == HttpStatusCode.RequestTimeout)
                .Or<BrokenCircuitException>()
                .WaitAndRetryAsync(4,
                    sleepDurationProvider: (c, ctx) =>
                    {
                        if (ctx.ContainsKey(SleepDurationKey))
                            return (TimeSpan)ctx[SleepDurationKey];
                        return TimeSpan.FromMilliseconds(200);
                    },
                    onRetry: (dr, ts, ctx) =>
                    {
                        Console.WriteLine($"Context: {(ctx.ContainsKey(SleepDurationKey) ? "Open" : "Closed")}");
                        Console.WriteLine($"Waits: {ts.TotalMilliseconds}");
                    });
    }

    static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
    {
        return Policy<HttpResponseMessage>
            .HandleResult(res => res.StatusCode == HttpStatusCode.GatewayTimeout || res.StatusCode == HttpStatusCode.RequestTimeout)
            .CircuitBreakerAsync(3, TimeSpan.FromSeconds(30),
               onBreak: (dr, ts, ctx) => { ctx[SleepDurationKey] = ts; },
               onReset: (ctx) => { ctx[SleepDurationKey] = null; });
    }
}
