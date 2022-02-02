#if NETCOREAPP
using System.Threading;
using System.Threading.Tasks;
using Vostok.Hosting.Abstractions.Diagnostics;
using MicrosoftHealthCheck = Microsoft.Extensions.Diagnostics.HealthChecks.IHealthCheck;
using MicrosoftHealthStatus = Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus;
using MicrosoftHealthCheckResult = Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult;
using MicrosoftHealthCheckContext = Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckContext;
using MicrosoftHealthCheckRegistration = Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckRegistration;
using VostokHealthCheck = Vostok.Hosting.Abstractions.Diagnostics.IHealthCheck;
using VostokHealthStatus = Vostok.Hosting.Abstractions.Diagnostics.HealthStatus;
using VostokHealthCheckResult = Vostok.Hosting.Abstractions.Diagnostics.HealthCheckResult;

namespace Vostok.Applications.AspNetCore.Diagnostics
{
    internal static class VostokHealthCheckAdapters
    {
        public static VostokHealthCheck ToVostokCheck(this MicrosoftHealthCheck microsoftCheck, MicrosoftHealthCheckRegistration registration)
            => new MicrosoftToVostokAdapter(microsoftCheck, registration);

        public static MicrosoftHealthCheck ToMicrosoftCheck(this VostokHealthCheck vostokCheck)
            => new VostokToMicrosoftAdapter(vostokCheck);

        public static bool IsAdapter(this VostokHealthCheck vostokCheck)
            => vostokCheck is MicrosoftToVostokAdapter;

        #region MicrosoftToVostokAdapter

        private class MicrosoftToVostokAdapter : VostokHealthCheck
        {
            private readonly MicrosoftHealthCheck check;
            private readonly MicrosoftHealthCheckRegistration registration;

            public MicrosoftToVostokAdapter(MicrosoftHealthCheck check, MicrosoftHealthCheckRegistration registration)
            {
                this.check = check;
                this.registration = registration;
            }

            public async Task<VostokHealthCheckResult> CheckAsync(CancellationToken cancellationToken)
            {
                var microsoftContext = new MicrosoftHealthCheckContext
                {
                    Registration = registration
                };

                var microsoftResult = await check.CheckHealthAsync(microsoftContext, cancellationToken).ConfigureAwait(false);

                return new VostokHealthCheckResult(
                    ConvertStatus(microsoftResult.Status),
                    ConvertReason(microsoftResult));
            }

            private static VostokHealthStatus ConvertStatus(MicrosoftHealthStatus status)
            {
                switch (status)
                {
                    case MicrosoftHealthStatus.Healthy:
                        return VostokHealthStatus.Healthy;

                    case MicrosoftHealthStatus.Degraded:
                        return VostokHealthStatus.Degraded;

                    default:
                        return VostokHealthStatus.Failing;
                }
            }

            private static string ConvertReason(MicrosoftHealthCheckResult result)
            {
                if (!string.IsNullOrEmpty(result.Description))
                    return result.Description;

                if (result.Exception != null)
                    return $"{result.Exception.GetType().Name}: {result.Exception.Message}";

                return null;
            }
        }

        #endregion

        #region VostokToMicrosoftAdapter

        private class VostokToMicrosoftAdapter : MicrosoftHealthCheck
        {
            private readonly VostokHealthCheck check;

            public VostokToMicrosoftAdapter(VostokHealthCheck check)
                => this.check = check;

            public async Task<MicrosoftHealthCheckResult> CheckHealthAsync(MicrosoftHealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
            {
                var result = await check.CheckSafeAsync(cancellationToken).ConfigureAwait(false);

                return new MicrosoftHealthCheckResult(ConvertStatus(result.Status), result.Reason);
            }

            private static MicrosoftHealthStatus ConvertStatus(VostokHealthStatus status)
            {
                switch (status)
                {
                    case VostokHealthStatus.Healthy:
                        return MicrosoftHealthStatus.Healthy;

                    case VostokHealthStatus.Degraded:
                        return MicrosoftHealthStatus.Degraded;

                    default:
                        return MicrosoftHealthStatus.Unhealthy;
                }
            }
        }

        #endregion
    }
}
#endif