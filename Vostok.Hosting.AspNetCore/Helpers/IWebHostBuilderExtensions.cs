using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Vostok.Hosting.Abstractions;

namespace Vostok.Hosting.AspNetCore.Helpers
{
    internal static class IWebHostBuilderExtensions
    {
        public static IWebHostBuilder ConfigureUrl(this IWebHostBuilder builder, IVostokHostingEnvironment environment)
        {
            var url = environment.ServiceBeacon.ReplicaInfo.GetUrl();

            if (url != null)
                builder = builder.UseUrls($"http://*:{url.Port}/");

            return builder;
        }

        public static IWebHostBuilder AddStartupFilter(this IWebHostBuilder builder, IStartupFilter startupFilter) => 
            builder.ConfigureServices(services => services.AddTransient(_ => startupFilter));
    }
}