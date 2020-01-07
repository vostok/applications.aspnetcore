using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Vostok.Clusterclient.Core.Model;
using Vostok.Commons.Time;
using Vostok.Context;
using Vostok.Hosting.AspNetCore.Configuration;
using Vostok.Hosting.AspNetCore.Models;

namespace Vostok.Hosting.AspNetCore.Middlewares
{
    internal class FillRequestInfoMiddleware : IMiddleware
    {
        private readonly FillRequestInfoSettings settings;
        
        public FillRequestInfoMiddleware([NotNull] FillRequestInfoSettings settings)
        {
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            IRequestInfo requestInfo = new RequestInfo(
                GetTimeout(context.Request),
                GetPriority(context.Request),
                GetClientApplicationIdentity(context.Request),
                context.Request.HttpContext.Connection.RemoteIpAddress);

            FlowingContext.Globals.Set(requestInfo);

            await next(context).ConfigureAwait(false);
        }

        private TimeSpan? GetTimeout(HttpRequest request)
        {
            if (double.TryParse(request.Headers[HeaderNames.RequestTimeout], NumberStyles.Any, CultureInfo.InvariantCulture, out var seconds))
                return seconds.Seconds();

            return settings.AdditionalTimeoutProviders
                .Select(provider => provider.Invoke(request))
                .FirstOrDefault(result => result != null);
        }

        private RequestPriority? GetPriority(HttpRequest request)
        {
            if (Enum.TryParse(request.Headers[HeaderNames.RequestPriority], true, out RequestPriority priority))
                return priority;

            return settings.AdditionalPriorityProviders
                .Select(provider => provider.Invoke(request))
                .FirstOrDefault(result => result != null);
        }

        private string GetClientApplicationIdentity(HttpRequest request)
        {
            var clientApplicationIdentity = request.Headers[HeaderNames.ApplicationIdentity].ToString();
            if (!string.IsNullOrEmpty(clientApplicationIdentity))
                return clientApplicationIdentity;

            return settings.AdditionalClientApplicationIdentityProviders
                .Select(provider => provider.Invoke(request))
                .FirstOrDefault(result => result != null);
        }
    }
}