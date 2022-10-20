using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Vostok.Applications.AspNetCore.Models;
using Vostok.Configuration.Abstractions;
using Vostok.Context;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Abstractions.Requirements;

namespace Vostok.Applications.AspNetCore.Helpers
{
    [PublicAPI]
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddVostokEnvironment(this IServiceCollection services, IVostokHostingEnvironment environment, IVostokApplication application)
        {
            services.AddSingleton(environment);

            services.AddVostokEnvironmentComponents();

            AddVostokEnvironmentHostExtensions(services, environment);

            AddSettingsProviders(services, RequirementDetector.GetRequiredConfigurations(application).Select(r => r.Type), environment.ConfigurationProvider);
            AddSettingsProviders(services, RequirementDetector.GetRequiredSecretConfigurations(application).Select(r => r.Type), environment.SecretConfigurationProvider);
            AddSettingsProviders(services, RequirementDetector.GetRequiredMergedConfigurations(application).Select(r => r.Type), environment.ConfigurationProvider);

            services.AddScoped(_ => FlowingContext.Globals.Get<IRequestInfo>());

            return services;
        }

        public static void AddVostokEnvironmentHostExtensions(this IServiceCollection services, IVostokHostingEnvironment environment)
        {
            foreach (var (type, obj) in environment.HostExtensions.GetAll())
                services.AddSingleton(type, obj);

            if (environment.HostExtensions.TryGet<IVostokApplicationDiagnostics>(out var diagnostics))
            {
                services
                    .AddSingleton(diagnostics.Info)
                    .AddSingleton(diagnostics.HealthTracker);
            }
        }

        public static IServiceCollection AddVostokEnvironmentComponents(this IServiceCollection services)
        {
            services
                .AddSingleton(provider => provider.GetRequiredService<IVostokHostingEnvironment>().ApplicationIdentity)
                .AddSingleton(provider => provider.GetRequiredService<IVostokHostingEnvironment>().ApplicationIdentity)
                .AddSingleton(provider => provider.GetRequiredService<IVostokHostingEnvironment>().ApplicationLimits)
                .AddTransient(provider => provider.GetRequiredService<IVostokHostingEnvironment>().ApplicationReplicationInfo)
                .AddSingleton(provider => provider.GetRequiredService<IVostokHostingEnvironment>().Metrics)
                .AddSingleton(provider => provider.GetRequiredService<IVostokHostingEnvironment>().Log)
                .AddSingleton(provider => provider.GetRequiredService<IVostokHostingEnvironment>().Tracer)
                .AddSingleton(provider => provider.GetRequiredService<IVostokHostingEnvironment>().HerculesSink)
                .AddSingleton(provider => provider.GetRequiredService<IVostokHostingEnvironment>().ConfigurationSource)
                .AddSingleton(provider => provider.GetRequiredService<IVostokHostingEnvironment>().ConfigurationProvider)
                .AddSingleton(provider => provider.GetRequiredService<IVostokHostingEnvironment>().ClusterConfigClient)
                .AddSingleton(provider => provider.GetRequiredService<IVostokHostingEnvironment>().ServiceBeacon)
                .AddSingleton(provider => provider.GetRequiredService<IVostokHostingEnvironment>().ServiceLocator)
                .AddSingleton(provider => provider.GetRequiredService<IVostokHostingEnvironment>().ContextGlobals)
                .AddSingleton(provider => provider.GetRequiredService<IVostokHostingEnvironment>().ContextProperties)
                .AddSingleton(provider => provider.GetRequiredService<IVostokHostingEnvironment>().ContextConfiguration)
                .AddSingleton(provider => provider.GetRequiredService<IVostokHostingEnvironment>().Datacenters)
                .AddSingleton(provider => provider.GetRequiredService<IVostokHostingEnvironment>().HostExtensions);

            return services;
        }

        private static void AddSettingsProviders(IServiceCollection services, IEnumerable<Type> types, IConfigurationProvider provider)
        {
            foreach (var type in types)
            {
                var methodInfo = typeof(ServiceCollectionExtensions).GetMethod(nameof(AddSettingsProvider), BindingFlags.NonPublic | BindingFlags.Static);
                var genericMethodInfo = methodInfo.MakeGenericMethod(type);
                genericMethodInfo.Invoke(null, new object[] {services, provider});
            }
        }

        private static void AddSettingsProvider<TSettings>(IServiceCollection services, IConfigurationProvider provider) =>
            services.AddSingleton<Func<TSettings>>(provider.Get<TSettings>);
    }
}