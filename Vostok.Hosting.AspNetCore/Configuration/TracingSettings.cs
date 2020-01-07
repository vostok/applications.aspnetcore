using JetBrains.Annotations;

namespace Vostok.Hosting.AspNetCore.Configuration
{
    [PublicAPI]
    public class TracingSettings
    {
        /// <summary>
        /// If filled, trace id will be written to response header.
        /// </summary>
        [CanBeNull]
        public string ResponseTraceIdHeader { get; set; } = "Trace-Id";
    }
}