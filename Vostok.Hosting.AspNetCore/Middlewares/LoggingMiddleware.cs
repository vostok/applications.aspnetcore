using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Vostok.Hosting.AspNetCore.Helpers;
using Vostok.Logging.Abstractions;

namespace Vostok.Hosting.AspNetCore.Middlewares
{
    [PublicAPI]
    public class LoggingMiddlewareSettings
    {
        public bool LogQueryString { get; set; } = true;

        public bool LogRequestHeaders { get; set; } = false;

        public bool LogResponseHeaders { get; set; } = false;
    }

    internal class LoggingMiddleware : IMiddleware
    {
        private readonly ILog log;
        private readonly LoggingMiddlewareSettings settings;

        public LoggingMiddleware(ILog log, LoggingMiddlewareSettings settings)
        {
            this.log = log;
            this.settings = settings;
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