using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Vostok.Context;
using Vostok.Hosting.AspNetCore.Models;

namespace Vostok.Hosting.AspNetCore.Middlewares
{
    internal class FillRequestInfoMiddleware : IMiddleware
    {
        private readonly FillRequestInfoMiddlewareSettings settings;
        
        public FillRequestInfoMiddleware([NotNull] FillRequestInfoMiddlewareSettings settings)
        {
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            IRequestInfo requestInfo = new RequestInfo(
                settings.TimeoutProvider?.Invoke(context.Request),
                settings.PriorityProvider?.Invoke(context.Request),
                settings.ApplicationIdentityProvider?.Invoke(context.Request));

            FlowingContext.Globals.Set(requestInfo);

            await next(context).ConfigureAwait(false);
        }
    }
}