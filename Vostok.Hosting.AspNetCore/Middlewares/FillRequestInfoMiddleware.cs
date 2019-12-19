using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Vostok.Context;
using Vostok.Hosting.AspNetCore.Models;

namespace Vostok.Hosting.AspNetCore.Middlewares
{
    /// <summary>
    /// Middleware that obtains <see cref="IRequestInfo"/> from request and stores it to <see cref="FlowingContext.Globals"/>.
    /// </summary>
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
                settings.ClientApplicationIdentityProvider?.Invoke(context.Request),
                context.Request.HttpContext.Connection.RemoteIpAddress);

            FlowingContext.Globals.Set(requestInfo);

            await next(context).ConfigureAwait(false);
        }
    }
}