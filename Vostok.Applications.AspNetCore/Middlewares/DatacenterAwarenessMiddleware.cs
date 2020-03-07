using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Vostok.Applications.AspNetCore.Configuration;
using Vostok.Datacenters;
using Vostok.Logging.Abstractions;

namespace Vostok.Applications.AspNetCore.Middlewares
{
    internal class DatacenterAwarenessMiddleware
    {
        private readonly RequestDelegate next;
        private readonly IOptions<DatacenterAwarenessSettings> options;
        private readonly IDatacenters datacenters;
        private readonly ILog log;

        public DatacenterAwarenessMiddleware(RequestDelegate next, IOptions<DatacenterAwarenessSettings> options, IDatacenters datacenters, ILog log)
        {
            this.next = next;
            this.options = options;
            this.datacenters = datacenters;
            this.log = log;
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