using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.AspNetCore.StartupFilters;

namespace Vostok.Hosting.AspNetCore.Helpers
{
    internal static class IWebHostBuilderExtensions
    {
        public static IWebHostBuilder UseLog(this IWebHostBuilder builder, ILoggerProvider loggerProvider) =>
            builder
                .ConfigureLogging(loggingBuilder => loggingBuilder.ClearProviders())
                .ConfigureLogging(loggingBuilder => loggingBuilder.AddProvider(loggerProvider));

        public static IWebHostBuilder AddConfigurationSource(this IWebHostBuilder builder, IConfigurationSource configurationSource) =>
            builder
                .ConfigureAppConfiguration(c => c.Add(configurationSource));

        public static IWebHostBuilder UseUrl(this IWebHostBuilder builder, IVostokHostingEnvironment environment)
        {
            var url = environment.ServiceBeacon.ReplicaInfo.GetUrl();

            if (url != null)
                builder = builder.UseUrls($"http://*:{url.Port}/");

            return builder;
        }

        public static IWebHostBuilder UseUrlPath(this IWebHostBuilder builder, IVostokHostingEnvironment environment) =>
            builder.AddStartupFilter(new UrlPathStartupFilter(environment));

        private static IWebHostBuilder AddStartupFilter(this IWebHostBuilder builder, IStartupFilter startupFilter) =>
            builder.ConfigureServices(
                services =>
                    services
                        .AddTransient(_ => startupFilter));

        public static IWebHostBuilder AddMiddleware<T>(this IWebHostBuilder builder, T middleware)
            where T : class, IMiddleware =>
            middleware == null
                ? builder
                : builder.ConfigureServices(
                        services =>
                            services
                                .AddSingleton(middleware)
                    )
                    .AddStartupFilter(new AddMiddlewareStartupFilter<T>());

        public static IWebHostBuilder RegisterTypes(this IWebHostBuilder builder, IVostokHostingEnvironment environment) =>
            builder.ConfigureServices(
                services =>
                {
                    services
                        .AddSingleton(environment)
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