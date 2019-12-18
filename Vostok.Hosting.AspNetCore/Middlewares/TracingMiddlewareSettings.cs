using System;
using JetBrains.Annotations;
using Vostok.Tracing.Abstractions;

namespace Vostok.Hosting.AspNetCore.Middlewares
{
    /// <summary>
    /// Configuration of the <see cref="TracingMiddleware"/>.
    /// </summary>
    [PublicAPI]
    public class TracingMiddlewareSettings
    {
        public TracingMiddlewareSettings([NotNull] ITracer tracer)
        {
            Tracer = tracer ?? throw new ArgumentNullException(nameof(tracer));
        }

        // CR(iloktionov): Зачем это здесь торчит в паблик?
        /// <summary>
        /// <see cref="ITracer"/> that will be used for span creation.
        /// </summary>
        [NotNull]
        public ITracer Tracer { get; }

        // CR(iloktionov): Почему нет дефолта?
        /// <summary>
        /// If filled, trace id will be written to response header.
        /// </summary>
        [CanBeNull]
        public string ResponseTraceIdHeader { get; set; }
    }
}