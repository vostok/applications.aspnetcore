using JetBrains.Annotations;
using Vostok.Applications.AspNetCore.Middlewares;

namespace Vostok.Applications.AspNetCore.Configuration
{
    /// <summary>
    /// Represents configuration of <see cref="TracingMiddleware"/>.
    /// </summary>
    [PublicAPI]
    public class TracingSettings
    {
        /// <summary>
        /// If filled, trace id will be written to response header. A good value to use is '<c>Trace-Id</c>'.
        /// </summary>
        [CanBeNull]
        public string ResponseTraceIdHeader { get; set; }
    }
}