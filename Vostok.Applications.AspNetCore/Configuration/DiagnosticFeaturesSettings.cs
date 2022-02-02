using JetBrains.Annotations;

namespace Vostok.Applications.AspNetCore.Configuration
{
    /// <summary>
    /// Configuration of built-in diagnostic info providers and health checks.
    /// </summary>
    [PublicAPI]
    public class DiagnosticFeaturesSettings
    {
        public bool AddThrottlingInfoProvider { get; set; } = true;

        public bool AddThrottlingHealthCheck { get; set; } = true;

        public bool AddCurrentRequestsInfoProvider { get; set; } = true;
    }
}