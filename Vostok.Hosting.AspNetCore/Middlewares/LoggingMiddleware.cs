using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Vostok.Hosting.AspNetCore.Helpers;
using Vostok.Logging.Abstractions;

namespace Vostok.Hosting.AspNetCore.Middlewares
{
    internal class LoggingMiddleware : IMiddleware
    {
        private readonly ILog log;
        private readonly LoggingMiddlewareSettings settings;

        public LoggingMiddleware([CanBeNull] ILog log = null, [CanBeNull] LoggingMiddlewareSettings settings = null)
        {
            this.log = log ?? LogProvider.Get();
            this.settings = settings ?? new LoggingMiddlewareSettings();
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            LogRequest(context.Request);

            await next(context).ConfigureAwait(false);

            //LogResponse(context.Response);
        }

        private void LogRequest(HttpRequest request)
        {
            log.Info("Recieved request '{Request}' from '{RequestFrom}' at '{RequestConnection}'. Timeout = {Timeout}.",
                new
                {
                    Request = request.FormatRequest(settings.LogQueryString, false),
                    RequestFrom = request.GetClientIdentity(),
                    RequestConnection = request.GetClientConnectionInfo(),
                    Timeout = request.GetTimeout(),
                });
        }
    }
}