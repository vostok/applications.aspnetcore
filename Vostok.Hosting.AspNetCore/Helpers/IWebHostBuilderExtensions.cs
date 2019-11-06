using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.AspNetCore.StartupFilters;

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
            builder.ConfigureServices(
                services =>
                    services
                        .AddTransient(_ => startupFilter));

        public static IWebHostBuilder AddMiddleware<T>(this IWebHostBuilder builder, T middleware)
            where T : class, IMiddleware =>
            builder.ConfigureServices(
                    services =>
                        services
                            .AddSingleton(middleware)
                )
                .AddStartupFilter(new AddMiddlewareStartupFilter<T>(middleware));

        public static IWebHostBuilder RegisterTypes(this IWebHostBuilder builder, IVostokHostingEnvironment environment) =>
            builder.ConfigureServices(
                services =>
                {
                    services
                        .AddSingleton(environment.ApplicationIdentity)
                        .AddSingleton(environment.ApplicationLimits)
                        .AddTransient(_ => environment.ApplicationReplicationInfo)
                        .AddSingleton(environment.Metrics)
                        .AddSingleton(environment.Log)
                        .AddSingleton(environment.Tracer)
                        .AddSingleton(environment.HerculesSink)
                        .AddSingleton(environment.ConfigurationSource)
                        .AddSingleton(environment.ConfigurationProvider)
                        .AddSingleton(environment.ClusterConfigClient)
                        .AddSingleton(environment.ServiceBeacon)
                        .AddSingleton(environment.ServiceLocator)
                        .AddSingleton(environment.ContextGlobals)
                        .AddSingleton(environment.ContextProperties)
                        .AddSingleton(environment.ContextConfiguration)
                        .AddSingleton(environment.ClusterClientSetup)
                        .AddSingleton(environment.Datacenters)
                        .AddSingleton(environment.HostExtensions);

                    foreach (var (type, obj) in environment.HostExtensions.GetAll())
                    {
                        services.AddSingleton(type, obj);
                    }
                });
    }
}