using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Vostok.Clusterclient.Core.Model;
using Vostok.Context;
using Vostok.Hosting.AspNetCore.Configuration;

namespace Vostok.Hosting.AspNetCore.Middlewares
{
    internal class DistributedContextMiddleware : IMiddleware
    {
        private readonly DistributedContextSettings settings;

        public DistributedContextMiddleware(DistributedContextSettings settings)
            => this.settings = settings;

        // TODO(iloktionov): ensure RequestPriority in flowing context globals
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