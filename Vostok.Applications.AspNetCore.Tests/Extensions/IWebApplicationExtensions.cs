#if NETCOREAPP

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Vostok.Applications.AspNetCore.Tests.Controllers;

namespace Vostok.Applications.AspNetCore.Tests.Extensions
{
    internal static class IWebApplicationExtensions
    {
        public static void ConfigureTestsDefaults(this IServiceCollection services)
        {
            services
                .AddControllers()
                .AddNewtonsoftJson()
                .AddApplicationPart(typeof(ContextController).Assembly);
        }

        public static void ConfigureTestsDefaults(this IApplicationBuilder application)
        {
            application.UseRouting();
            application.UseEndpoints(s => s.MapControllers());
            application.UseHealthChecks("/health");
        }
    }
}

#endif