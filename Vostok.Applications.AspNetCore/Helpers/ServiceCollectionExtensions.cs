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

        public static IServiceCollection AddVostokEnvironmentComponents(this IServiceCollection services, Func<IServiceProvider, IVostokHostingEnvironment> environmentProvider = null)
        {
            environmentProvider ??= provider => provider.GetRequiredService<IVostokHostingEnvironment>();
            
            services
                .AddSingleton(serviceProvider => environmentProvider(serviceProvider).ApplicationIdentity)
                .AddSingleton(serviceProvider => environmentProvider(serviceProvider).ApplicationLimits)
                .AddTransient(serviceProvider => environmentProvider(serviceProvider).ApplicationReplicationInfo)
                .AddSingleton(serviceProvider => environmentProvider(serviceProvider).Metrics)
                .AddSingleton(serviceProvider => environmentProvider(serviceProvider).Log)
                .AddSingleton(serviceProvider => environmentProvider(serviceProvider).Tracer)
                .AddSingleton(serviceProvider => environmentProvider(serviceProvider).HerculesSink)
                .AddSingleton(serviceProvider => environmentProvider(serviceProvider).ConfigurationSource)
                .AddSingleton(serviceProvider => environmentProvider(serviceProvider).ConfigurationProvider)
                .AddSingleton(serviceProvider => environmentProvider(serviceProvider).ClusterConfigClient)
                .AddSingleton(serviceProvider => environmentProvider(serviceProvider).ServiceBeacon)
                .AddSingleton(serviceProvider => environmentProvider(serviceProvider).ServiceLocator)
                .AddSingleton(serviceProvider => environmentProvider(serviceProvider).ContextGlobals)
                .AddSingleton(serviceProvider => environmentProvider(serviceProvider).ContextProperties)
                .AddSingleton(serviceProvider => environmentProvider(serviceProvider).ContextConfiguration)
                .AddSingleton(serviceProvider => environmentProvider(serviceProvider).Datacenters)
                .AddSingleton(serviceProvider => environmentProvider(serviceProvider).HostExtensions);

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