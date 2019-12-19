using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Vostok.Clusterclient.Core.Model;
using Vostok.Commons.Time;
using Vostok.Context;
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
            var parameters = new List<object>(5) { FormatPath(request, settings.LogQueryString) };

            if (requestInfo.ClientApplicationIdentity != null)
            {
                template.Append(" '{RequestFrom}' at");
                parameters.Add(requestInfo.ClientApplicationIdentity);
            }

            template.Append(" '{RequestConnection}'");
            parameters.Add(GetClientConnectionInfo(request));

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
                parameters.Add(FormatRequestHeaders(request, settings.LogRequestHeaders));
            }
            
            settings.Log.Info(template.ToString(), parameters.ToArray());
        }

        private void LogResponse(HttpRequest request, HttpResponse response, TimeSpan elapsed)
        {
            var template = new StringBuilder("Response code = {ResponseCode:D} ('{ResponseCode}'). Time = {ElapsedTime}.");
            var parameters = new List<object>(5) { (ResponseCode)response.StatusCode, (ResponseCode)response.StatusCode, elapsed.ToPrettyString() };

            var bodySize = GetBodySize(response);
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
                parameters.Add(FormatResponseHeaders(response, settings.LogResponseHeaders));
            }

            // CR(iloktionov): А не лучше заменить на log.Info(...) с типизированным объектом properties?
            settings.Log.Log(new LogEvent(LogLevel.Info, PreciseDateTime.Now, template.ToString())
                .WithParameters(parameters.ToArray())
                .WithProperty("ElapsedTimeMs", elapsed.TotalMilliseconds));
        }

        private static string GetClientConnectionInfo(HttpRequest request)
        {
            var connection = request.HttpContext.Connection;
            return $"{connection.RemoteIpAddress}:{connection.RemotePort}";
        }

        private static string FormatPath(HttpRequest request, LoggingCollectionSettings logQueryStringSettings)
        {
            var builder = new StringBuilder();

            builder.Append(request.Method);
            builder.Append(" ");

            builder.Append(request.Path);

            if (logQueryStringSettings.IsEnabledForRequest(request))
            {
                if (logQueryStringSettings.IsEnabledForAllKeys())
                {
                    builder.Append(request.QueryString);
                }
                else
                {
                    var filtered = request.Query.Where(kvp => logQueryStringSettings.IsEnabledForKey(kvp.Key)).ToList();

                    for (var i = 0; i < filtered.Count; i++)
                    {
                        if (i == 0)
                            builder.Append("?");
                        builder.Append($"{filtered[i].Key}={filtered[i].Value}");
                    }
                }
            }

            return builder.ToString();
        }

        private static string FormatRequestHeaders(HttpRequest request, LoggingCollectionSettings settings)
        {
            var builder = new StringBuilder();

            foreach (var header in request.Headers)
            {
                if (!settings.IsEnabledForKey(header.Key))
                    continue;

                builder.AppendLine();
                builder.Append("\t");
                builder.Append(header.Key);
                builder.Append(": ");
                builder.Append(header.Value);
            }

            return builder.ToString();
        }

        private static string FormatResponseHeaders(HttpResponse response, LoggingCollectionSettings settings)
        {
            var builder = new StringBuilder();

            foreach (var header in response.Headers)
            {
                if (!settings.IsEnabledForKey(header.Key))
                    continue;

                builder.AppendLine();
                builder.Append("\t");
                builder.Append(header.Key);
                builder.Append(": ");
                builder.Append(header.Value);
            }

            return builder.ToString();
        }

        private static long? GetBodySize(HttpResponse response)
        {
            if (long.TryParse(response.Headers[HeaderNames.ContentLength], out var length))
                return length;
            return null;
        }
    }
}