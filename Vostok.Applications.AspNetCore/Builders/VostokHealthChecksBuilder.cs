#if NETCOREAPP
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Vostok.Applications.AspNetCore.Diagnostics;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Abstractions.Diagnostics;

namespace Vostok.Applications.AspNetCore.Builders
{
    internal class VostokHealthChecksBuilder
    {
        private readonly IVostokHostingEnvironment environment;
        private readonly List<IDisposable> disposables;

        public VostokHealthChecksBuilder(IVostokHostingEnvironment environment, List<IDisposable> disposables)
        {
            this.environment = environment;
            this.disposables = disposables;
        }

        public void Register(IServiceCollection services)
        {
            services.AddHealthChecks();

            if (environment.HostExtensions.TryGet<IVostokApplicationDiagnostics>(out var diagnostics))
                ReplaceHealthCheckService(services, diagnostics.HealthTracker);
        }

        private void ReplaceHealthCheckService(IServiceCollection services, IHealthTracker healthTracker)
        {
            var descriptors = services.Where(service => service.ServiceType == typeof(HealthCheckService)).ToArray();
            var defaultRegistration = descriptors.First();

            var vostokRegistration = ServiceDescriptor.Describe(
                typeof(HealthCheckService),
                provider => new VostokHealthCheckService(
                    provider.GetRequiredService<IOptions<HealthCheckServiceOptions>>(),
                    provider,
                    healthTracker,
                    defaultRegistration.ImplementationType,
                    disposables),
                ServiceLifetime.Singleton);

            foreach (var descriptor in descriptors)
                services.Remove(descriptor);

            services.Add(vostokRegistration);
        }
    }
}
#endif
