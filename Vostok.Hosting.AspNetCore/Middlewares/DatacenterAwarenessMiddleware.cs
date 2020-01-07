using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Vostok.Datacenters;
using Vostok.Hosting.AspNetCore.Configuration;
using Vostok.Logging.Abstractions;

namespace Vostok.Hosting.AspNetCore.Middlewares
{
    internal class DatacenterAwarenessMiddleware : IMiddleware
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