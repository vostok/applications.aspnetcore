using JetBrains.Annotations;
using Vostok.Hosting.AspNetCore.Middlewares;

namespace Vostok.Hosting.AspNetCore.Configuration
{
    /// <summary>
    /// Configuration of the <see cref="TracingMiddleware"/>.
    /// </summary>
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