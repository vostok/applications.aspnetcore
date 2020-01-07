using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Vostok.Clusterclient.Core.Model;
using Vostok.Context;
using Vostok.Hosting.AspNetCore.Configuration;

namespace Vostok.Hosting.AspNetCore.Middlewares
{
    internal class RestoreDistributedContextMiddleware : IMiddleware
    {
        private readonly DistributedContextSettings settings;

        public RestoreDistributedContextMiddleware(DistributedContextSettings settings)
        {
            this.settings = settings;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            FlowingContext.RestoreDistributedProperties(context.Request.Headers[HeaderNames.ContextProperties]);
            FlowingContext.RestoreDistributedGlobals(context.Request.Headers[HeaderNames.ContextGlobals]);

            foreach (var action in settings.AdditionalRestoreDistributedContextActions)
                action(context.Request);

            await next(context).ConfigureAwait(false);
        }
    }
}