using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Vostok.Applications.AspNetCore.Configuration;
using Vostok.Applications.AspNetCore.Models;
using Vostok.Clusterclient.Core.Model;
using Vostok.Commons.Formatting;
using Vostok.Commons.Time;
using Vostok.Context;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Abstractions.Values;
using Vostok.Tracing.Abstractions;

namespace Vostok.Applications.AspNetCore.Middlewares
{
    /// <summary>
    /// Logs incoming requests and outgoing responses.
    /// </summary>
    [PublicAPI]
    public class LoggingMiddleware
    {
        private const int StringBuilderCapacity = 256;

        private readonly RequestDelegate next;
        private readonly LoggingSettings options;
        private readonly ILog log;

        public LoggingMiddleware(
            [NotNull] RequestDelegate next,
            [NotNull] IOptions<LoggingSettings> options,
            [NotNull] ILog log)
        {
            this.next = next ?? throw new ArgumentNullException(nameof(next));
            this.options = (options ?? throw new ArgumentNullException(nameof(options))).Value;
            this.log = (log ?? throw new ArgumentNullException(nameof(log))).ForContext<LoggingMiddleware>();
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (options.LogRequests)
                LogRequest(context.Request);

            var watch = Stopwatch.StartNew();
           
            if (options.LogResponseCompletion)
            {
                var tracingContext = FlowingContext.Globals.Get<TraceContext>();
                var operationContext = FlowingContext.Globals.Get<OperationContextValue>();

                context.Response.OnCompleted(
                    () =>
                    {
                        using (FlowingContext.Globals.Use(tracingContext))
                        using (FlowingContext.Globals.Use(operationContext))
                            LogResponseCompleted(watch.Elapsed);

                        return Task.CompletedTask;
                    });
            }

            await next(context);

            if (options.LogResponses)
                LogResponse(context.Request, context.Response, watch.Elapsed);
        }

        private void LogRequest(HttpRequest request)
        {
            var requestInfo = FlowingContext.Globals.Get<IRequestInfo>();
            var builder = StringBuilderCache.Acquire(StringBuilderCapacity);

            var addClientIdentity = requestInfo?.ClientApplicationIdentity != null;
            var addBodySize = request.ContentLength > 0L;
            var addHeaders = options.LogRequestHeaders.IsEnabledForRequest(request);

            var parametersCount = 3 + (addClientIdentity ? 1 : 0) + (addBodySize ? 1 : 0) + (addHeaders ? 1 : 0);
            var parameters = new object[parametersCount];
            var parametersIndex = 0;

            AppendSegment(builder, parameters, "Received request '{Request}' from", FormatPath(builder, request, options.LogQueryString), ref parametersIndex);

            if (addClientIdentity)
                AppendSegment(builder, parameters, " '{ClientIdentity}' at", requestInfo.ClientApplicationIdentity, ref parametersIndex);

            AppendSegment(builder, parameters, " '{RequestConnection}'", GetClientConnectionInfo(request), ref parametersIndex);
            AppendSegment(builder, parameters, " with timeout = {Timeout}", requestInfo?.Timeout.ToPrettyString() ?? "unknown", ref parametersIndex);

            builder.Append('.');

            if (addBodySize)
                AppendSegment(builder, parameters, " Body size = {BodySize}.", request.ContentLength, ref parametersIndex);

            if (addHeaders)
                AppendSegment(builder, parameters, " Request headers: {RequestHeaders}", FormatHeaders(builder, request.Headers, options.LogRequestHeaders), ref parametersIndex);

            log.Info(builder.ToString(), parameters);

            StringBuilderCache.Release(builder);
        }

        private void LogResponse(HttpRequest request, HttpResponse response, TimeSpan elapsed)
        {
            var builder = StringBuilderCache.Acquire(StringBuilderCapacity);

            var addBodySize = response.ContentLength > 0;
            var addHeaders = options.LogResponseHeaders.IsEnabledForRequest(request);

            builder.Append("Response code = {ResponseCode:D} ('{ResponseCode}'). Time = {ElapsedTime}.");

            if (addBodySize)
                builder.Append(" Body size = {BodySize}.");

            if (addHeaders)
                builder.Append(" Response headers: {ResponseHeaders}");

            var logEvent = new LogEvent(LogLevel.Info, PreciseDateTime.Now, builder.ToString())
                .WithProperty("Path", request.Path)
                .WithProperty("Method", request.Method)
                .WithProperty("ResponseCode", (ResponseCode)response.StatusCode)
                .WithProperty("ElapsedTime", elapsed.ToPrettyString())
                .WithProperty("ElapsedTimeMs", elapsed.TotalMilliseconds);

            if (addBodySize)
                logEvent = logEvent.WithProperty("BodySize", response.ContentLength);

            if (addHeaders)
                logEvent = logEvent.WithProperty("ResponseHeaders", FormatHeaders(builder, response.Headers, options.LogResponseHeaders));

            log.Log(logEvent);

            StringBuilderCache.Release(builder);
        }

        private void LogResponseCompleted(TimeSpan elapsed)
            => log.Info("Response has completed in {ElapsedTime}.", new
            {
                ElapsedTime = elapsed.ToPrettyString(),
                ElapsedTimeMs = elapsed.TotalMilliseconds,
            });

        private static void AppendSegment(StringBuilder builder, object[] parameters, string templateSegment, object parameter, ref int parameterIndex)
        {
            builder.Append(templateSegment);

            parameters[parameterIndex++] = parameter;
        }

        private static string GetClientConnectionInfo(HttpRequest request)
        {
            var connection = request.HttpContext.Connection;
            return $"{connection.RemoteIpAddress}:{connection.RemotePort}";
        }

        private static string FormatPath(StringBuilder builder, HttpRequest request, LoggingCollectionSettings querySettings)
        {
            return FormatAndRollback(
                builder,
                b =>
                {
                    b.Append(request.Method);
                    b.Append(" ");
                    b.Append(request.Path);

                    if (querySettings.IsEnabledForRequest(request))
                    {
                        if (querySettings.IsEnabledForAllKeys())
                        {
                            b.Append(request.QueryString);
                        }
                        else
                        {
                            var writtenFirst = false;

                            foreach (var pair in request.Query.Where(kvp => querySettings.IsEnabledForKey(kvp.Key)))
                            {
                                if (!writtenFirst)
                                {
                                    b.Append('?');
                                    writtenFirst = true;
                                }

                                b.Append($"{pair.Key}={pair.Value}");
                            }
                        }
                    }
                });
        }

        private static string FormatHeaders(StringBuilder builder, IHeaderDictionary headers, LoggingCollectionSettings settings)
        {
            return FormatAndRollback(
                builder,
                b =>
                {
                    foreach (var pair in headers)
                    {
                        if (!settings.IsEnabledForKey(pair.Key))
                            continue;

                        b.AppendLine();
                        b.Append('\t');
                        b.Append(pair.Key);
                        b.Append(": ");
                        b.Append(pair.Value);
                    }
                });
        }

        private static string FormatAndRollback(StringBuilder builder, Action<StringBuilder> format)
        {
            var positionBefore = builder.Length;

            format(builder);

            var result = builder.ToString(positionBefore, builder.Length - positionBefore);

            builder.Length = positionBefore;

            return result;
        }
    }
}