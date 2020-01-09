using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Vostok.Applications.AspNetCore.Configuration;
using Vostok.Applications.AspNetCore.Models;
using Vostok.Clusterclient.Context;
using Vostok.Clusterclient.Core.Model;
using Vostok.Context;

namespace Vostok.Applications.AspNetCore.Middlewares
{
    internal class DistributedContextMiddleware : IMiddleware
    {
        private readonly DistributedContextSettings settings;

        public DistributedContextMiddleware(DistributedContextSettings settings)
            => this.settings = settings;

        static DistributedContextMiddleware()
            => FlowingContext.Configuration.RegisterDistributedGlobal(DistributedContextConstants.RequestPriorityGlobalName, new RequestPrioritySerializer());

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            FlowingContext.RestoreDistributedProperties(context.Request.Headers[HeaderNames.ContextProperties]);
            FlowingContext.RestoreDistributedGlobals(context.Request.Headers[HeaderNames.ContextGlobals]);

            if (FlowingContext.Globals.Get<RequestPriority?>() == null)
                FlowingContext.Globals.Set<RequestPriority?>(FlowingContext.Globals.Get<IRequestInfo>()?.Priority ?? RequestPriority.Ordinary);

            foreach (var action in settings.AdditionalActions)
                action(context.Request);

            await next(context);
        }
    }
}