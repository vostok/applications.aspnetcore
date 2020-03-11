using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Vostok.Applications.AspNetCore.Configuration;
using Vostok.Applications.AspNetCore.Models;
using Vostok.Clusterclient.Context;
using Vostok.Clusterclient.Core.Model;
using Vostok.Context;

namespace Vostok.Applications.AspNetCore.Middlewares
{
    /// <summary>
    /// Restores distributed context from request headers and ensures request priority is in <see cref="FlowingContext"/>.
    /// </summary>
    [PublicAPI]
    public class DistributedContextMiddleware
    {
        private readonly RequestDelegate next;
        private readonly DistributedContextSettings options;

        static DistributedContextMiddleware()
            => FlowingContext.Configuration.RegisterDistributedGlobal(DistributedContextConstants.RequestPriorityGlobalName, new RequestPrioritySerializer());

        public DistributedContextMiddleware(
            [NotNull] RequestDelegate next,
            [NotNull] IOptions<DistributedContextSettings> options)
        {
            this.next = next ?? throw new ArgumentNullException(nameof(next));
            this.options = (options ?? throw new ArgumentNullException(nameof(options))).Value;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            FlowingContext.RestoreDistributedProperties(context.Request.Headers[HeaderNames.ContextProperties]);
            FlowingContext.RestoreDistributedGlobals(context.Request.Headers[HeaderNames.ContextGlobals]);

            if (FlowingContext.Globals.Get<RequestPriority?>() == null)
            {
                var priority = FlowingContext.Globals.Get<IRequestInfo>()?.Priority ?? RequestPriority.Ordinary;

                FlowingContext.Globals.Set<RequestPriority?>(priority);
                FlowingContext.Globals.Set(priority);
            }

            foreach (var action in options.AdditionalActions)
                action(context.Request);

            await next(context);
        }
    }
}