using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Vostok.Clusterclient.Core.Model;
using Vostok.Commons.Time;
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

            var sw = Stopwatch.StartNew();

            await next(context).ConfigureAwait(false);

            LogResponse(context.Response, sw.Elapsed);
        }

        private void LogRequest(HttpRequest request)
        {
            var template = new StringBuilder("Recieved request '{Request}' from");
            var parameters = new List<object>(5) { request.FormatPath(settings.LogQueryString) };

            var requestFrom = request.GetClientIdentity();
            if (requestFrom != null)
            {
                template.Append(" '{RequestFrom}' at");
                parameters.Add(requestFrom);
            }

            template.Append(" '{RequestConnection}'");
            parameters.Add(request.GetClientConnectionInfo());

            var timeout = request.GetTimeout();
            if (timeout != null)
            {
                template.Append(" with timeout {Timeout}");
                parameters.Add(timeout.Value.ToPrettyString());
            }

            template.Append(".");

            if (settings.LogRequestHeaders)
            {
                template.Append("{RequestHeaders}");
                parameters.Add(request.FormatHeaders());
            }
            
            log.Info(template.ToString(), parameters.ToArray());
        }

        private void LogResponse(HttpResponse response, TimeSpan elapsed)
        {
            var template = new StringBuilder("Response code = {ResponseCode:D} ('{ResponseCode}'). Time = {ElapsedTime}.");
            var parameters = new List<object>(5) { (ResponseCode)response.StatusCode, (ResponseCode)response.StatusCode, elapsed.ToPrettyString() };

            var bodySize = response.GetBodySize();
            if (bodySize != null)
            {
                template.Append(" Body size = {BodySize}.");
                parameters.Add(bodySize);
            }

            if (settings.LogResponseHeaders)
            {
                template.Append("{RequestHeaders}");
                parameters.Add(response.FormatHeaders());
            }

            log.Log(new LogEvent(LogLevel.Info, PreciseDateTime.Now, template.ToString())
                .WithParameters(parameters.ToArray())
                .WithProperty("ElapsedTimeMs", elapsed.TotalMilliseconds));
        }
    }
}