using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Vostok.Logging.Abstractions;

namespace Vostok.Hosting.AspNetCore.Middlewares
{
    /// <summary>
    /// Middleware that denies all incoming requests in case of non-healthy application status.
    /// </summary>
    internal class DenyRequestsMiddleware : IMiddleware
    {
        private readonly DenyRequestsMiddlewareSettings settings;
        private readonly ILog log;

        public DenyRequestsMiddleware([NotNull] DenyRequestsMiddlewareSettings settings, [CanBeNull] ILog log)
        {
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
            this.log = log ?? LogProvider.Get();
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (settings.Enabled())
            {
                context.Response.StatusCode = settings.ResponseCode;

                log.Info("Request has been denied.");

                return;
            }

            await next(context).ConfigureAwait(false);
        }
    }
}