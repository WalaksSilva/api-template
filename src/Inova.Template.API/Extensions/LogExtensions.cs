using Microsoft.AspNetCore.Builder;
using Inova.Template.API.Middlewares;

namespace Inova.Template.API.Extensions;

public static class LogExtensions
{
    public static IApplicationBuilder UseLogMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<LogMiddleware>();
    }
}
