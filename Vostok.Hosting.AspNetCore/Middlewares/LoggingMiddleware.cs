using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Vostok.Clusterclient.Core.Model;
using Vostok.Commons.Formatting;
using Vostok.Commons.Time;
using Vostok.Context;
using Vostok.Hosting.AspNetCore.Configuration;
using Vostok.Hosting.AspNetCore.Models;
using Vostok.Logging.Abstractions;

namespace Vostok.Hosting.AspNetCore.Middlewares
{
    internal class LoggingMiddleware : IMiddleware
    {
        private const int StringBuilderCapacity = 256;

        private readonly LoggingSettings settings;
        private readonly ILog log;

        public LoggingMiddleware(LoggingSettings settings, ILog log)
        {
            this.settings = settings;
            this.log = log;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            LogRequest(context.Request);

            var sw = Stopwatch.StartNew();

            await next(context).ConfigureAwait(false);

            LogResponse(context.Request, context.Response, sw.Elapsed);
        }

        private void LogRequest(HttpRequest request)
        {
            var requestInfo = FlowingContext.Globals.Get<IRequestInfo>();

            var template = StringBuilderCache.Acquire(StringBuilderCapacity);
            template.Append("Received request '{Request}' from");
            var parameters = new List<object>(5) { FormatPath(request, settings.LogQueryString) };

            if (requestInfo.ClientApplicationIdentity != null)
            {
                template.Append(" '{RequestFrom}' at");
                parameters.Add(requestInfo.ClientApplicationIdentity);
            }

            template.Append(" '{RequestConnection}'");
            parameters.Add(GetClientConnectionInfo(request));

            var timeout = requestInfo.RemainingTimeout;
            if (timeout != null)
            {
                template.Append(" with timeout {Timeout}");
                parameters.Add(timeout.Value.ToPrettyString());
            }
            
            template.Append(".");

            if (settings.LogRequestHeaders.IsEnabledForRequest(request))
            {
                template.Append("\nRequest headers:{RequestHeaders}");
                parameters.Add(FormatRequestHeaders(request, settings.LogRequestHeaders));
            }
            
            log.Info(template.ToString(), parameters.ToArray());

            StringBuilderCache.Release(template);
        }

        private void LogResponse(HttpRequest request, HttpResponse response, TimeSpan elapsed)
        {
            var template = StringBuilderCache.Acquire(StringBuilderCapacity);
            template.Append("Response code = {ResponseCode:D} ('{ResponseCode}'). Time = {ElapsedTime}.");
            var parameters = new List<object>(5) { (ResponseCode)response.StatusCode, (ResponseCode)response.StatusCode, elapsed.ToPrettyString() };

            var bodySize = response.ContentLength;
            if (bodySize != null)
            {
                template.Append(" Body size = {BodySize}.");
                parameters.Add(bodySize);
            }

            if (settings.LogResponseHeaders.IsEnabledForRequest(request))
            {
                template.Append("\nResponse headers:{ResponseHeaders}");
                parameters.Add(FormatResponseHeaders(response, settings.LogResponseHeaders));
            }

            log.Log(new LogEvent(LogLevel.Info, PreciseDateTime.Now, template.ToString())
                .WithParameters(parameters.ToArray())
                .WithProperty("ElapsedTimeMs", elapsed.TotalMilliseconds));

            StringBuilderCache.Release(template);
        }

        private static string GetClientConnectionInfo(HttpRequest request)
        {
            var connection = request.HttpContext.Connection;
            return $"{connection.RemoteIpAddress}:{connection.RemotePort}";
        }

        private static string FormatPath(HttpRequest request, LoggingCollectionSettings logQueryStringSettings)
        {
            var builder = StringBuilderCache.Acquire(StringBuilderCapacity);

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

            var result = builder.ToString();
            StringBuilderCache.Release(builder);
            return result;
        }

        private static string FormatRequestHeaders(HttpRequest request, LoggingCollectionSettings settings)
        {
            var builder = StringBuilderCache.Acquire(StringBuilderCapacity);

            foreach (var (key, value) in request.Headers)
            {
                if (!settings.IsEnabledForKey(key))
                    continue;

                builder.AppendLine();
                builder.Append("\t");
                builder.Append(key);
                builder.Append(": ");
                builder.Append(value);
            }

            var result = builder.ToString();
            StringBuilderCache.Release(builder);
            return result;
        }

        private static string FormatResponseHeaders(HttpResponse response, LoggingCollectionSettings settings)
        {
            var builder = StringBuilderCache.Acquire(StringBuilderCapacity);

            foreach (var (key, value) in response.Headers)
            {
                if (!settings.IsEnabledForKey(key))
                    continue;

                builder.AppendLine();
                builder.Append("\t");
                builder.Append(key);
                builder.Append(": ");
                builder.Append(value);
            }

            var result = builder.ToString();
            StringBuilderCache.Release(builder);
            return result;
        }
    }
}