using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Vostok.Applications.AspNetCore.Configuration;
using Vostok.Applications.AspNetCore.Models;
using Vostok.Context;
using Vostok.Logging.Context;
using Vostok.Logging.Tracing;
using Vostok.Tracing.Abstractions;
using Vostok.Tracing.Extensions.Http;

namespace Vostok.Applications.AspNetCore.Middlewares
{
    /// <summary>
    /// Populates <see cref="TraceContext"/> and creates spans of kind <see cref="WellKnownSpanKinds.HttpRequest.Server"/>.
    /// </summary>
    [PublicAPI]
    public class TracingMiddleware
    {
        private readonly RequestDelegate next;
        private readonly IOptions<TracingSettings> options;
        private readonly ITracer tracer;

        public TracingMiddleware(
            [NotNull] RequestDelegate next,
            [NotNull] IOptions<TracingSettings> options,
            [NotNull] ITracer tracer)
        {
            this.next = next ?? throw new ArgumentNullException(nameof(next));
            this.options = options ?? throw new ArgumentNullException(nameof(options));
            this.tracer = tracer ?? throw new ArgumentNullException(nameof(tracer));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var requestInfo = FlowingContext.Globals.Get<IRequestInfo>();

            using (var spanBuilder = tracer.BeginHttpServerSpan())
            using (new OperationContextToken(
                TracingLogPropertiesFormatter.FormatPrefix(
                    spanBuilder.CurrentSpan.ParentSpanId ?? spanBuilder.CurrentSpan.SpanId) ?? string.Empty))
            {
                spanBuilder.SetClientDetails(requestInfo.ClientApplicationIdentity, requestInfo.ClientIpAddress);
                spanBuilder.SetRequestDetails(context.Request.Path, context.Request.Method, context.Request.ContentLength);

                if (options.Value.ResponseTraceIdHeader != null)
                    context.Response.Headers[options.Value.ResponseTraceIdHeader] = spanBuilder.CurrentSpan?.TraceId.ToString();

                await next(context);

                spanBuilder.SetResponseDetails(context.Response.StatusCode, context.Response.ContentLength);
            }
        }
    }
}