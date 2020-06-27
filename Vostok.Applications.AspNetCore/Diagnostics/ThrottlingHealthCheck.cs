using System.Threading;
using System.Threading.Tasks;
using Vostok.Hosting.Abstractions.Diagnostics;
using Vostok.Throttling;

namespace Vostok.Applications.AspNetCore.Diagnostics
{
    internal class ThrottlingHealthCheck : IHealthCheck
    {
        private readonly ThrottlingProvider throttlingProvider;

        public ThrottlingHealthCheck(ThrottlingProvider throttlingProvider)
            => this.throttlingProvider = throttlingProvider;

        public Task<HealthCheckResult> CheckAsync(CancellationToken cancellationToken)
        {
            var info = throttlingProvider.CurrentInfo;

            if (info.Enabled && info.QueueSize > 0)
                return Task.FromResult(HealthCheckResult.Degraded($"There's a throttling queue of size {info.QueueSize}."));

            return Task.FromResult(HealthCheckResult.Healthy());
        }
    }
}
