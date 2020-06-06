using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Vostok.Applications.AspNetCore.Configuration;
using Vostok.Applications.AspNetCore.Models;
using Vostok.Clusterclient.Core.Model;
using Vostok.Commons.Helpers.Url;
using Vostok.Commons.Time;
using Vostok.Context;
using Vostok.Logging.Abstractions;
using Vostok.Throttling;

namespace Vostok.Applications.AspNetCore.Middlewares
{
    /// <summary>
    /// Limits request parallelism. See <see cref="IThrottlingProvider"/> for more info.
    /// </summary>
    [PublicAPI]
    public class ThrottlingMiddleware
    {
        private const long LargeRequestBodySize = 256 * 1024;
        private static readonly TimeSpan LongThrottlingWaitTime = 500.Milliseconds();

        private readonly RequestDelegate next;
        private readonly ThrottlingSettings options;
        private readonly IThrottlingProvider provider;
        private readonly ILog log;

        public ThrottlingMiddleware(
            [NotNull] RequestDelegate next,
            [NotNull] IOptions<ThrottlingSettings> options,
            [NotNull] IThrottlingProvider provider,
            [NotNull] ILog log)
        {
            this.next = next ?? throw new ArgumentNullException(nameof(next));
            this.options = (options ?? throw new ArgumentNullException(nameof(options))).Value;
            this.provider = provider ?? throw new ArgumentNullException(nameof(provider));
            this.log = (log ?? throw new ArgumentNullException(nameof(log))).ForContext<ThrottlingMiddleware>();
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (IsDisabled(context))
            {
                await next(context);
                return;
            }

            var info = FlowingContext.Globals.Get<IRequestInfo>();
            var properties = BuildThrottlingProperties(context, info);

            using (var result = await provider.ThrottleAsync(properties, info?.RemainingTimeout))
            {
                if (result.Status == ThrottlingStatus.Passed)
                {
                    if (result.WaitTime >= LongThrottlingWaitTime)
                        LogWaitTime(context, info, result);

                    await next(context);

                    return;
                }

                LogFailure(context, info, result);

                if (ShouldAbortConnection(context, result))
                {
                    LogAbortingConnection();
                    context.Abort();
                }
                else
                {
                    context.Response.StatusCode = options.RejectionResponseCode;
                    context.Response.Headers.ContentLength = 0L;
                }
            }
        }

        private static bool ShouldAbortConnection(HttpContext context, IThrottlingResult result)
            => result.Status == ThrottlingStatus.RejectedDueToDeadline ||
               context.Request.ContentLength > LargeRequestBodySize ||
               context.Request.Headers[HeaderNames.TransferEncoding] == "chunked";

        private static string GetClientConnectionInfo(HttpContext context)
            => $"{context.Connection.RemoteIpAddress}:{context.Connection.RemotePort}";

        private bool IsDisabled(HttpContext context)
        {
            if (options.DisableForWebSockets && context.WebSockets.IsWebSocketRequest)
                return true;

            if (options.Enabled != null && !options.Enabled(context))
                return true;

            return false;
        }

        private IReadOnlyDictionary<string, string> BuildThrottlingProperties(HttpContext context, IRequestInfo info)
        {
            var builder = new ThrottlingPropertiesBuilder();

            if (options.AddConsumerProperty)
                builder.AddConsumer(info?.ClientApplicationIdentity);

            if (options.AddPriorityProperty)
                builder.AddPriority(info?.Priority.ToString());

            if (options.AddMethodProperty)
                builder.AddPriority(context.Request.Method);

            if (options.AddUrlProperty)
                builder.AddUrl(UrlNormalizer.NormalizePath(context.Request.Path));

            foreach (var additionalProperty in options.AdditionalProperties)
            {
                var (propertyName, propertyValue) = additionalProperty(context);
                builder.AddProperty(propertyName, propertyValue);
            }

            return builder.Build();
        }

        private void LogWaitTime(HttpContext context, IRequestInfo info, IThrottlingResult result)
            => log.Warn(
                "Request from '{ClientIdentity}' at {RequestConnection} spent {ThrottlingWaitTime} on throttling.",
                new
                {
                    ClientIdentity = info?.ClientApplicationIdentity ?? "unknown",
                    RequestConnection = GetClientConnectionInfo(context),
                    ThrottlingWaitTime = result.WaitTime.ToPrettyString(),
                    ThrottlingWaitTimeMs = result.WaitTime.TotalMilliseconds
                });

        private void LogFailure(HttpContext context, IRequestInfo info, IThrottlingResult result)
            => log.Error(
                "Dropping request from '{ClientIdentity}' at {RequestConnection} due to throttling status {ThrottlingStatus}. Rejection reason = '{RejectionReason}'.",
                info?.ClientApplicationIdentity ?? "unknown",
                GetClientConnectionInfo(context),
                result.Status,
                result.RejectionReason);

        private void LogAbortingConnection()
            => log.Info("Aborting client connection..");
    }
}