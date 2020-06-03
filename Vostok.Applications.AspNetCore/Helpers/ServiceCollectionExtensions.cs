using Microsoft.Extensions.DependencyInjection;
using Vostok.Hosting.Abstractions;

namespace Vostok.Applications.AspNetCore.Helpers
{
    internal static class ServiceCollectionExtensions
    {
        public static void AddVostokEnvironment(this IServiceCollection services, IVostokHostingEnvironment environment)
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
                .AddSingleton(environment.Datacenters)
                .AddSingleton(environment.Diagnostics)
                .AddSingleton(environment.Diagnostics.Info)
                .AddSingleton(environment.Diagnostics.HealthTracker)
                .AddSingleton(environment.HostExtensions);

            foreach (var (type, obj) in environment.HostExtensions.GetAll())
                services.AddSingleton(type, obj);
        }
    }
}
