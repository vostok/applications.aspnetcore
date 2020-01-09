using JetBrains.Annotations;

namespace Vostok.Applications.AspNetCore.Configuration
{
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