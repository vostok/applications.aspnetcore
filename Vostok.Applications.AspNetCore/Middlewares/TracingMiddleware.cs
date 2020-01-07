using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Vostok.Applications.AspNetCore.Configuration;
using Vostok.Applications.AspNetCore.Models;
using Vostok.Context;
using Vostok.Tracing.Abstractions;
using Vostok.Tracing.Extensions.Http;

namespace Vostok.Applications.AspNetCore.Middlewares
{
    internal class TracingMiddleware : IMiddleware
    {
        private readonly TracingSettings settings;
        private readonly ITracer tracer;

        public TracingMiddleware(TracingSettings settings, ITracer tracer)
        {
            this.settings = settings;
            this.tracer = tracer;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var requestInfo = FlowingContext.Globals.Get<IRequestInfo>();
            
            using (var spanBuilder = tracer.BeginHttpServerSpan())
            {
                spanBuilder.SetClientDetails(requestInfo.ClientApplicationIdentity, requestInfo.ClientIpAddress);
                spanBuilder.SetRequestDetails(context.Request.Path, context.Request.Method, context.Request.ContentLength);
                
                if (settings.ResponseTraceIdHeader != null)
                    context.Response.Headers[settings.ResponseTraceIdHeader] = spanBuilder.CurrentSpan?.TraceId.ToString();

                await next(context).ConfigureAwait(false);

                spanBuilder.SetResponseDetails(context.Response.StatusCode, context.Response.ContentLength);
            }
        }
    }
}