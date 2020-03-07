using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
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
    internal class ThrottlingMiddleware : IMiddleware
    {
        private const long LargeRequestBodySize = 4 * 1024;
        private static readonly TimeSpan LongThrottlingWaitTime = 500.Milliseconds();

        private readonly ThrottlingSettings settings;
        private readonly IThrottlingProvider provider;
        private readonly ILog log;

        public ThrottlingMiddleware(ThrottlingSettings settings, IThrottlingProvider provider, ILog log)
        {
            this.settings = settings;
            this.provider = provider;
            this.log = log;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (IsDisabled(context))
            {
                await next(context);
                return;
            }

            var info = FlowingContext.Globals.Get<IRequestInfo>();
            var properties = BuildThrottlingProperties(context, info);

            using (var result = await provider.ThrottleAsync(properties, info.RemainingTimeout))
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
                    context.Response.StatusCode = settings.RejectionResponseCode;
                    context.Response.Headers.ContentLength = 0L;
                }
            }
        }

        private static bool ShouldAbortConnection(HttpContext context, IThrottlingResult result)
            => result.Status == ThrottlingStatus.RejectedDueToDeadline ||
               context.Request.ContentLength > LargeRequestBodySize ||
               context.Request.Headers[HeaderNames.TransferEncoding] == "chunked";

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string GetClientConnectionInfo(HttpContext context)
            => $"{context.Connection.RemoteIpAddress}:{context.Connection.RemotePort}";

        private bool IsDisabled(HttpContext context)
        {
            if (settings.DisableForWebSockets && context.WebSockets.IsWebSocketRequest)
                return true;

            if (settings.Enabled != null && !settings.Enabled(context))
                return true;

            return false;
        }

        private IReadOnlyDictionary<string, string> BuildThrottlingProperties(HttpContext context, IRequestInfo info)
        {
            var builder = new ThrottlingPropertiesBuilder();

            if (settings.AddConsumerProperty)
                builder.AddConsumer(info.ClientApplicationIdentity);

            if (settings.AddPriorityProperty)
                builder.AddPriority(info.Priority.ToString());

            if (settings.AddMethodProperty)
                builder.AddPriority(context.Request.Method);

            if (settings.AddUrlProperty)
                builder.AddUrl(UrlNormalizer.NormalizePath(context.Request.Path));

            foreach (var additionalProperty in settings.AdditionalProperties)
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
                    ClientIdentity = info.ClientApplicationIdentity,
                    RequestConnection = GetClientConnectionInfo(context),
                    ThrottlingWaitTime = result.WaitTime.ToPrettyString(),
                    ThrottlingWaitTimeMs = result.WaitTime.TotalMilliseconds
                });

        private void LogFailure(HttpContext context, IRequestInfo info, IThrottlingResult result)
            => log.Error(
                "Dropping request from '{ClientIdentity}' at {RequestConnection} due to throttling status {ThrottlingStatus}. Rejection reason = '{RejectionReason}'.",
                info.ClientApplicationIdentity,
                GetClientConnectionInfo(context),
                result.Status,
                result.RejectionReason);

        private void LogAbortingConnection()
            => log.Info("Aborting client connection..");
    }
}