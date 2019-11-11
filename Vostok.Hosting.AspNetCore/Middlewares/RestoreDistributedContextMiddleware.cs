using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Vostok.Clusterclient.Core.Model;
using Vostok.Context;

namespace Vostok.Hosting.AspNetCore.Middlewares
{
    internal class RestoreDistributedContextMiddleware : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            FlowingContext.RestoreDistributedProperties(context.Request.Headers[HeaderNames.ContextProperties]);
            FlowingContext.RestoreDistributedGlobals(context.Request.Headers[HeaderNames.ContextGlobals]);
            await next(context).ConfigureAwait(false);
        }
    }
}