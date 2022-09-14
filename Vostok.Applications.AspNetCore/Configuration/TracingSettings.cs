using System;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Vostok.Applications.AspNetCore.Middlewares;
using Vostok.Tracing.Extensions.Http;

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

        /// <summary>
        /// If filled, request URL will be written as an absolute <see cref="Uri"/> by combining <see cref="BaseUrl"/> and request path.
        /// </summary>
        [CanBeNull]
        public Uri BaseUrl { get; set; }
        
        /// <summary>
        /// If set to a non-null value, an additional <see cref="HttpContext"/> transformation will be applied to current <see cref="IHttpRequestServerSpanBuilder"/> before request processing.
        /// </summary>
        [CanBeNull]
        public Action<IHttpRequestServerSpanBuilder, HttpContext> SetAdditionalRequestDetails { get; set; }
        
        /// <summary>
        /// If set to a non-null value, an additional <see cref="HttpContext"/> transformation will be applied to current <see cref="IHttpRequestServerSpanBuilder"/> after request processing.
        /// </summary>
        [CanBeNull]
        public Action<IHttpRequestServerSpanBuilder, HttpContext> SetAdditionalResponseDetails { get; set; }
    }
}