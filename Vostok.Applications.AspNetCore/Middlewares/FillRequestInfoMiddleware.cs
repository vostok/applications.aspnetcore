using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Vostok.Applications.AspNetCore.Configuration;
using Vostok.Applications.AspNetCore.Models;
using Vostok.Clusterclient.Core.Model;
using Vostok.Commons.Helpers;
using Vostok.Commons.Time;
using Vostok.Context;

namespace Vostok.Applications.AspNetCore.Middlewares
{
    internal class FillRequestInfoMiddleware
    {
        private readonly RequestDelegate next;
        private readonly FillRequestInfoSettings settings;

        public FillRequestInfoMiddleware(RequestDelegate next, FillRequestInfoSettings settings)
        {
            this.next = next;
            this.settings = settings;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            IRequestInfo requestInfo = new RequestInfo(
                GetTimeout(context.Request),
                GetPriority(context.Request),
                GetClientApplicationIdentity(context.Request),
                context.Request.HttpContext.Connection.RemoteIpAddress);

            FlowingContext.Globals.Set(requestInfo);

            await next(context);
        }

        private static TResult ObtainFromProviders<TResult>(HttpRequest request, IEnumerable<Func<HttpRequest, TResult>> providers)
            => providers.Select(provider => provider(request)).FirstOrDefault();

        private TimeSpan GetTimeout(HttpRequest request)
        {
            if (NumericTypeParser<double>.TryParse(request.Headers[HeaderNames.RequestTimeout], out var seconds))
                return seconds.Seconds();

            return ObtainFromProviders(request, settings.AdditionalTimeoutProviders) ?? settings.DefaultTimeoutProvider(request);
        }

        private RequestPriority GetPriority(HttpRequest request)
        {
            if (Enum.TryParse(request.Headers[HeaderNames.RequestPriority], true, out RequestPriority priority))
                return priority;

            return ObtainFromProviders(request, settings.AdditionalPriorityProviders) ?? settings.DefaultPriorityProvider(request);
        }

        private string GetClientApplicationIdentity(HttpRequest request)
        {
            var clientApplicationIdentity = request.Headers[HeaderNames.ApplicationIdentity].ToString();
            if (!string.IsNullOrEmpty(clientApplicationIdentity))
                return clientApplicationIdentity;

            return ObtainFromProviders(request, settings.AdditionalClientIdentityProviders);
        }
    }
}