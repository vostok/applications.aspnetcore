using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Vostok.Applications.AspNetCore.Diagnostics;
using Vostok.Applications.AspNetCore.Models;
using Vostok.Configuration.Abstractions;
using Vostok.Context;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Abstractions.Diagnostics;
using Vostok.Hosting.Abstractions.Requirements;
#if NETCOREAPP
using Microsoft.Extensions.Diagnostics.HealthChecks;
#endif

namespace Vostok.Applications.AspNetCore.Helpers
{
    [PublicAPI]
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddVostokEnvironment(this IServiceCollection services, IVostokHostingEnvironment environment, IVostokApplication application)
        {
            services.AddSingleton(environment);

            services.AddVostokEnvironmentComponents(environment);

            services.AddVostokEnvironmentHostExtensions(environment);

            services.AddSettingsProviders(RequirementDetector.GetRequiredConfigurations(application).Select(r => r.Type), environment.ConfigurationProvider);
            services.AddSettingsProviders(RequirementDetector.GetRequiredSecretConfigurations(application).Select(r => r.Type), environment.SecretConfigurationProvider);
            services.AddSettingsProviders(RequirementDetector.GetRequiredMergedConfigurations(application).Select(r => r.Type), environment.ConfigurationProvider);

            services.AddScoped(_ => FlowingContext.Globals.Get<IRequestInfo>());

            return services;
        }

        public static IServiceCollection AddVostokEnvironmentComponents(this IServiceCollection services, IVostokHostingEnvironment environment)
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
                .AddSingleton(environment.Datacenters)
                .AddSingleton(environment.HostExtensions);

            return services;
        }
        
        [Obsolete("Doesn't work due to https://github.com/dotnet/runtime/issues/36049. Use overload which takes environment.")]
        public static IServiceCollection AddVostokEnvironmentComponents(this IServiceCollection services)
        {
            services
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
        
#if NETCOREAPP
        public static void AddVostokHealthChecks(this IServiceCollection services, IVostokHostingEnvironment environment)
        {
            services.AddHealthChecks();

            if (!environment.HostExtensions.TryGet<IVostokApplicationDiagnostics>(out var diagnostics))
                return;
            
            var descriptors = services.Where(service => service.ServiceType == typeof(HealthCheckService)).ToArray();
            var defaultRegistration = descriptors.First();

            var vostokRegistration = ServiceDescriptor.Describe(
                typeof(HealthCheckService),
                provider => new VostokHealthCheckService(
                    provider.GetRequiredService<IOptions<HealthCheckServiceOptions>>(),
                    provider,
                    diagnostics.HealthTracker,
                    defaultRegistration.ImplementationType,
                    provider.GetRequiredService<VostokDisposables>()),
                ServiceLifetime.Singleton);

            foreach (var descriptor in descriptors)
                services.Remove(descriptor);

            services.Add(vostokRegistration);
        }
#endif

        private static void AddSettingsProviders(this IServiceCollection services, IEnumerable<Type> types, IConfigurationProvider provider)
        {
            foreach (var type in types)
            {
                var methodInfo = typeof(ServiceCollectionExtensions).GetMethod(nameof(AddSettingsProvider), BindingFlags.NonPublic | BindingFlags.Static);
                var genericMethodInfo = methodInfo.MakeGenericMethod(type);
                genericMethodInfo.Invoke(null, new object[] {services, provider});
            }
        }

        private static void AddSettingsProvider<TSettings>(this IServiceCollection services, IConfigurationProvider provider) =>
            services.AddSingleton<Func<TSettings>>(provider.Get<TSettings>);
    }
}