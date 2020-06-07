#if NETCOREAPP3_1
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Vostok.Hosting.Abstractions.Diagnostics;
using HealthReport = Microsoft.Extensions.Diagnostics.HealthChecks.HealthReport;

namespace Vostok.Applications.AspNetCore.Diagnostics
{
    internal class VostokHealthCheckService : HealthCheckService
    {
        private readonly HealthCheckServiceOptions options;
        private readonly IServiceProvider serviceProvider;
        private readonly IHealthTracker vostokTracker;
        private readonly Type defaultServiceType;
        private readonly List<IDisposable> disposables;

        public VostokHealthCheckService(
            [NotNull] IOptions<HealthCheckServiceOptions> options,
            [NotNull] IServiceProvider serviceProvider,
            [NotNull] IHealthTracker vostokTracker,
            [NotNull] Type defaultServiceType,
            [NotNull] List<IDisposable> disposables)
        {
            this.defaultServiceType = defaultServiceType ?? throw new ArgumentNullException(nameof(defaultServiceType));
            this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            this.vostokTracker = vostokTracker ?? throw new ArgumentNullException(nameof(vostokTracker));
            this.disposables = disposables ?? throw new ArgumentNullException(nameof(disposables));
            this.options = (options ?? throw new ArgumentNullException(nameof(options))).Value;

            RegisterMicrosoftChecksInVostokTracker();
        }

        public override Task<HealthReport> CheckHealthAsync(Func<HealthCheckRegistration, bool> predicate, CancellationToken cancellationToken = new CancellationToken()) 
            => CreateService(CreateOptions()).CheckHealthAsync(predicate, cancellationToken);

        private void RegisterMicrosoftChecksInVostokTracker()
        {
            foreach (var registration in options.Registrations)
            {
                var vostokCheck = registration.Factory(serviceProvider).ToVostokCheck(registration);

                disposables.Add(vostokTracker.RegisterCheck(registration.Name, vostokCheck));
            }
        }

        private HealthCheckServiceOptions CreateOptions()
        {
            var result = new HealthCheckServiceOptions();

            foreach (var microsoftRegistration in options.Registrations)
                result.Registrations.Add(microsoftRegistration);

            foreach (var (name, check) in vostokTracker)
            {
                if (check.IsAdapter())
                    continue;
                
                var registration = new HealthCheckRegistration(name, check.ToMicrosoftCheck(), null, new[] {"vostok"});

                result.Registrations.Add(registration);
            }

            return result;
        }

        private HealthCheckService CreateService(HealthCheckServiceOptions optionsToUse)
            => (HealthCheckService) ActivatorUtilities.CreateInstance(serviceProvider, defaultServiceType, Options.Create(optionsToUse));
    }
}
#endif