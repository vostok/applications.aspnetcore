using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Vostok.Datacenters;
using Vostok.Hosting.AspNetCore.Configuration;
using Vostok.Logging.Abstractions;

namespace Vostok.Hosting.AspNetCore.Middlewares
{
    internal class DenyRequestsIfNotInActiveDatacenterMiddleware : IMiddleware
    {
        private readonly DatacenterAwarenessSettings settings;
        private readonly ILog log;

        public DenyRequestsIfNotInActiveDatacenterMiddleware([NotNull] DatacenterAwarenessSettings settings, [CanBeNull] ILog log)
        {
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
            this.log = log ?? LogProvider.Get();
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (!settings.Datacenters.LocalDatacenterIsActive())
            {
                context.Response.StatusCode = settings.DenyResponseCode;

                log.Info("Request has been denied.");

                return;
            }

            await next(context).ConfigureAwait(false);
        }
    }
}