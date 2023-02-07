using Microsoft.AspNetCore.Builder;
using Inova.Modelo.API.Middlewares;

namespace Inova.Modelo.API.Extensions;

public static class LogExtensions
{
    public static IApplicationBuilder UseLogMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<LogMiddleware>();
    }
}
