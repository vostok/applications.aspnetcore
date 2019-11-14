using System;
using JetBrains.Annotations;
using Vostok.Tracing.Abstractions;

namespace Vostok.Hosting.AspNetCore.Middlewares
{
    [PublicAPI]
    public class TracingMiddlewareSettings
    {
        public TracingMiddlewareSettings([NotNull] ITracer tracer)
        {
            Tracer = tracer ?? throw new ArgumentNullException(nameof(tracer));
        }

        [NotNull]
        public ITracer Tracer { get; }

        [CanBeNull]
        public string ResponseTraceIdHeader { get; set; }
    }
}