using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Vostok.Clusterclient.Core.Model;
using Vostok.Context;

namespace Vostok.Hosting.AspNetCore.Middlewares
{
    /// <summary>
    /// Middleware that restores distributed <see cref="FlowingContext.Properties"/> and <see cref="FlowingContext.Globals"/> from <see cref="HttpContext"/>
    /// </summary>
    internal class RestoreDistributedContextMiddleware : IMiddleware
    {
        private readonly RestoreDistributedContextMiddlewareSettings settings;

        public RestoreDistributedContextMiddleware(RestoreDistributedContextMiddlewareSettings settings)
        {
            this.settings = settings;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            FlowingContext.RestoreDistributedProperties(context.Request.Headers[HeaderNames.ContextProperties]);
            FlowingContext.RestoreDistributedGlobals(context.Request.Headers[HeaderNames.ContextGlobals]);
            
            settings.AdditionalRestoreDistributedContextAction?.Invoke(context.Request);

            await next(context).ConfigureAwait(false);
        }
    }
}