using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

            AddSettingsProviders(services, RequirementDetector.GetRequiredConfigurations(application).Select(r => r.Type), environment.ConfigurationProvider);
            AddSettingsProviders(services, RequirementDetector.GetRequiredSecretConfigurations(application).Select(r => r.Type), environment.ConfigurationProvider);
        }

        private static void AddSettingsProviders(IServiceCollection services, IEnumerable<Type> types, IConfigurationProvider provider)
        {
            foreach (var type in types)
            {
                var methodInfo = typeof(ServiceCollectionExtensions).GetMethod(nameof(AddSettingsProvider), BindingFlags.NonPublic | BindingFlags.Static);
                var genericMethodInfo = methodInfo.MakeGenericMethod(type);
                genericMethodInfo.Invoke(null, new object[] { services, provider });
            }
        }

        private static void AddSettingsProvider<TSettings>(IServiceCollection services, IConfigurationProvider provider) =>
            services.AddSingleton<Func<TSettings>>(provider.Get<TSettings>);
    }
}