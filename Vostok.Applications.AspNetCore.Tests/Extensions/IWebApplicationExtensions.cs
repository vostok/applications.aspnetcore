using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Vostok.Applications.AspNetCore.Tests.Controllers;

namespace Vostok.Applications.AspNetCore.Tests.Extensions
{
    internal static class IWebApplicationExtensions
    {
#if NETCOREAPP
        public static void ConfigureServiceCollection(this IServiceCollection services)
        {
            services
                .AddControllers()
                .AddNewtonsoftJson()
                .AddApplicationPart(typeof(ContextController).Assembly);
        }
#endif

        public static void ConfigureWebApplication(this IApplicationBuilder application)
        {
            application
                .UseRouting()
                .UseEndpoints(s => s.MapControllers())
                .UseHealthChecks("/health");
        }
    }
}