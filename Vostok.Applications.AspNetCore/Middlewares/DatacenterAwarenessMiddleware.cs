using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Vostok.Applications.AspNetCore.Configuration;
using Vostok.Datacenters;
using Vostok.Logging.Abstractions;

namespace Vostok.Applications.AspNetCore.Middlewares
{
    internal class DatacenterAwarenessMiddleware
    {
        private readonly DatacenterAwarenessSettings settings;
        private readonly IDatacenters datacenters;
        private readonly ILog log;

        public DatacenterAwarenessMiddleware(DatacenterAwarenessSettings settings, IDatacenters datacenters, ILog log)
        {
            this.settings = settings;
            this.datacenters = datacenters;
            this.log = log;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (settings.RejectRequestsWhenDatacenterIsInactive && !datacenters.LocalDatacenterIsActive())
            {
                context.Response.StatusCode = settings.RejectionResponseCode;

                log.Warn("Rejecting request as local datacenter '{Datacenter}' is not active.", datacenters.GetLocalDatacenter());

                return;
            }

            await next(context);
        }
    }
}