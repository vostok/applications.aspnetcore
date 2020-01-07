using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Vostok.Applications.AspNetCore.Configuration;
using Vostok.Applications.AspNetCore.Models;
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
            var info = FlowingContext.Globals.Get<IRequestInfo>();
            var properties = BuildThrottlingProperties(context, info);

            using (var result = await provider.ThrottleAsync(properties, info.RemainingTimeout))
            {
                if (result.Status == ThrottlingStatus.Passed)
                {
                    if (result.WaitTime >= LongThrottlingWaitTime)
                        LogWaitTime(info, result);

                    await next(context);

                    return;
                }

                LogFailure(info, result);

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

        // TODO(iloktionov): Chunked transfer encoding, web sockets?
        private static bool ShouldAbortConnection(HttpContext context, IThrottlingResult result)
            => result.Status == ThrottlingStatus.RejectedDueToDeadline || context.Request.ContentLength > LargeRequestBodySize;

        private IReadOnlyDictionary<string, string> BuildThrottlingProperties(HttpContext context, IRequestInfo info)
        {
            var builder = new ThrottlingPropertiesBuilder();

            if (settings.AddConsumerProperty)
                builder.AddConsumer(info.ClientApplicationIdentity);

            if (settings.AddPriorityProperty)
                builder.AddPriority(info.Priority?.ToString());

            if (settings.AddMethodProperty)
                builder.AddPriority(context.Request.Method);

            if (settings.AddUrlProperty)
                builder.AddUrl(UrlNormalizer.NormalizePath(context.Request.Path));

            return builder.Build();
        }

        private void LogWaitTime(IRequestInfo info, IThrottlingResult result)
            => log.Warn(
                "Request from {ClientIdentity} spent {ThrottlingWaitTime} on throttling.",
                info.ClientApplicationIdentity,
                result.WaitTime);

        private void LogFailure(IRequestInfo info, IThrottlingResult result)
            => log.Error(
                "Dropping request from {ClientIdentity} due to throttling status {ThrottlingStatus}. Rejection reason = '{RejectionReason}'.",
                info.ClientApplicationIdentity,
                result.Status,
                result.RejectionReason);

        private void LogAbortingConnection()
            => log.Info("Aborting client connection..");
    }
}
