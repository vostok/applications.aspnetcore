using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Vostok.Applications.AspNetCore.Configuration;
using Vostok.Datacenters;
using Vostok.Logging.Abstractions;

namespace Vostok.Applications.AspNetCore.Middlewares
{
    /// <summary>
    /// Rejects incoming requests when local datacenter is not active.
    /// </summary>
    [PublicAPI]
    public class DatacenterAwarenessMiddleware
    {
        private readonly RequestDelegate next;
        private readonly IOptions<DatacenterAwarenessSettings> options;
        private readonly IDatacenters datacenters;
        private readonly ILog log;

        public DatacenterAwarenessMiddleware(
            [NotNull] RequestDelegate next,
            [NotNull] IOptions<DatacenterAwarenessSettings> options,
            [NotNull] IDatacenters datacenters,
            [NotNull] ILog log)
        {
            this.next = next ?? throw new ArgumentNullException(nameof(next));
            this.options = options ?? throw new ArgumentNullException(nameof(options));
            this.datacenters = datacenters ?? throw new ArgumentNullException(nameof(datacenters));
            this.log = (log ?? throw new ArgumentNullException(nameof(log))).ForContext<DatacenterAwarenessMiddleware>();
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (options.Value.RejectRequestsWhenDatacenterIsInactive && !datacenters.LocalDatacenterIsActive())
            {
                context.Response.StatusCode = options.Value.RejectionResponseCode;

                log.Warn("Rejecting request as local datacenter '{Datacenter}' is not active.", datacenters.GetLocalDatacenter());

                return;
            }

            await next(context);
        }
    }
}