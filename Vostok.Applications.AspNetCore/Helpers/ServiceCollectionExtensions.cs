using System;
using Microsoft.Extensions.DependencyInjection;
using Vostok.Configuration.Abstractions;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Abstractions.Requirements;
using Vostok.Logging.Abstractions;

namespace Vostok.Applications.AspNetCore.Helpers
{
    internal static class ServiceCollectionExtensions
    {
        public static void AddVostokEnvironment(this IServiceCollection services, IVostokHostingEnvironment environment, IVostokApplication application)
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
                .AddSingleton(environment.HostExtensions);

            foreach (var (type, obj) in environment.HostExtensions.GetAll())
                services.AddSingleton(type, obj);

            if (environment.HostExtensions.TryGet<IVostokApplicationDiagnostics>(out var diagnostics))
            {
                services
                    .AddSingleton(diagnostics.Info)
                    .AddSingleton(diagnostics.HealthTracker);
            }

            foreach (var configuration in RequirementDetector.GetRequiredConfigurations(application))
            {
                LogProvider.Get().Info("Adding configuration 1 " + configuration.Type);
                var methodInfo = typeof(ServiceCollectionExtensions).GetMethod(nameof(AddSettingsProvider));
                var genericMethodInfo = methodInfo.MakeGenericMethod(configuration.Type);
                genericMethodInfo.Invoke(null, new object[] {services, environment.ConfigurationProvider});
            }
        }

        public static void AddSettingsProvider<TSettings>(IServiceCollection services, IConfigurationProvider provider)
        {
            services.AddSingleton<Func<TSettings>>(provider.Get<TSettings>);
            LogProvider.Get().Info("Adding configuration 2 " + typeof(TSettings));
        }
    }
}