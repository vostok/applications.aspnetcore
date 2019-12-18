using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Vostok.Clusterclient.Core.Model;
using Vostok.Commons.Time;
using Vostok.Context;
using Vostok.Hosting.AspNetCore.Helpers;
using Vostok.Hosting.AspNetCore.Models;
using Vostok.Logging.Abstractions;

namespace Vostok.Hosting.AspNetCore.Middlewares
{
    /// <summary>
    /// Middleware that logs all requests and responses.
    /// </summary>
    internal class LoggingMiddleware : IMiddleware
    {
        private readonly LoggingMiddlewareSettings settings;

        public LoggingMiddleware(LoggingMiddlewareSettings settings)
        {
            this.settings = settings;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            LogRequest(context.Request);

            var sw = Stopwatch.StartNew();

            await next(context).ConfigureAwait(false);

            LogResponse(context.Request, context.Response, sw.Elapsed);
        }

        // CR(iloktionov): Может, заюзаем StringBuilderCache из vostok.commons.formatting, чтобы не сорить?
        private void LogRequest(HttpRequest request)
        {
            var requestInfo = FlowingContext.Globals.Get<IRequestInfo>();

            var template = new StringBuilder("Received request '{Request}' from");
            var parameters = new List<object>(5) { request.FormatPath(settings.LogQueryString) };

            if (requestInfo.ClientApplicationIdentity != null)
            {
                template.Append(" '{RequestFrom}' at");
                parameters.Add(requestInfo.ClientApplicationIdentity);
            }

            template.Append(" '{RequestConnection}'");
            parameters.Add(request.GetClientConnectionInfo());

            if (requestInfo.Timeout != null)
            {
                template.Append(" with timeout {Timeout}");
                parameters.Add(requestInfo.Timeout.Value.ToPrettyString());
            }
            
            template.Append(".");

            if (settings.LogRequestHeaders.IsEnabledForRequest(request))
            {
                // CR(iloktionov): Нужно дать понять, что это хедеры. Например, так: "Request headers: ...".
                template.Append("{RequestHeaders}");
                parameters.Add(request.FormatHeaders(settings.LogRequestHeaders));
            }
            
            settings.Log.Info(template.ToString(), parameters.ToArray());
        }

        private void LogResponse(HttpRequest request, HttpResponse response, TimeSpan elapsed)
        {
            var template = new StringBuilder("Response code = {ResponseCode:D} ('{ResponseCode}'). Time = {ElapsedTime}.");
            var parameters = new List<object>(5) { (ResponseCode)response.StatusCode, (ResponseCode)response.StatusCode, elapsed.ToPrettyString() };

            var bodySize = response.GetBodySize();
            if (bodySize != null)
            {
                template.Append(" Body size = {BodySize}.");
                parameters.Add(bodySize);
            }

            if (settings.LogResponseHeaders.IsEnabledForRequest(request))
            {
                // CR(iloktionov): 1. Нужно дать понять, что это хедеры. Например, так: "Response headers: ...".
                // CR(iloktionov): 2. Это всё-таки response headers, копипаста.

                template.Append("{RequestHeaders}");
                parameters.Add(response.FormatHeaders(settings.LogResponseHeaders));
            }

            // CR(iloktionov): А не лучше заменить на log.Info(...) с типизированным объектом properties?
            settings.Log.Log(new LogEvent(LogLevel.Info, PreciseDateTime.Now, template.ToString())
                .WithParameters(parameters.ToArray())
                .WithProperty("ElapsedTimeMs", elapsed.TotalMilliseconds));
        }
    }
}