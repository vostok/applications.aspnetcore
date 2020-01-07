﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Vostok.Context;
using Vostok.Hosting.AspNetCore.Configuration;
using Vostok.Hosting.AspNetCore.Models;
using Vostok.Tracing.Abstractions;
using Vostok.Tracing.Extensions.Http;

namespace Vostok.Hosting.AspNetCore.Middlewares
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
                {
                    var traceId = spanBuilder.CurrentSpan.TraceId;

                    context.Response.OnStarting(state =>
                    {
                        var ctx = (HttpContext)state;
                        ctx.Response.Headers[settings.ResponseTraceIdHeader] = traceId.ToString();
                        return Task.CompletedTask;
                    }, context);
                }

                await next(context).ConfigureAwait(false);

                spanBuilder.SetResponseDetails(context.Response.StatusCode, context.Response.ContentLength);
            }
        }
    }
}