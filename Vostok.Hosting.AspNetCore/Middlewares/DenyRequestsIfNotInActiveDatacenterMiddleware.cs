using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Vostok.Datacenters;
using Vostok.Logging.Abstractions;

namespace Vostok.Hosting.AspNetCore.Middlewares
{
    internal class DenyRequestsIfNotInActiveDatacenterMiddleware : IMiddleware
    {
        private readonly DenyRequestsIfNotInActiveDatacenterMiddlewareSettings settings;
        private readonly ILog log;

        public DenyRequestsIfNotInActiveDatacenterMiddleware([NotNull] DenyRequestsIfNotInActiveDatacenterMiddlewareSettings settings, [CanBeNull] ILog log)
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