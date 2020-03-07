using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Vostok.Applications.AspNetCore.Configuration;
using Vostok.Applications.AspNetCore.Models;
using Vostok.Clusterclient.Core.Model;
using Vostok.Commons.Helpers;
using Vostok.Commons.Time;
using Vostok.Context;

namespace Vostok.Applications.AspNetCore.Middlewares
{
    /// <summary>
    /// Populates an <see cref="IRequestInfo"/> object from request properties and stores as a <see cref="FlowingContext"/> global for the lifetime of the request.
    /// </summary>
    [PublicAPI]
    public class FillRequestInfoMiddleware
    {
        private readonly RequestDelegate next;
        private readonly IOptions<FillRequestInfoSettings> options;

        public FillRequestInfoMiddleware(
            [NotNull] RequestDelegate next,
            [NotNull] IOptions<FillRequestInfoSettings> options)
        {
            this.next = next ?? throw new ArgumentNullException(nameof(next));
            this.options = options ?? throw new ArgumentNullException(nameof(options));
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

            return ObtainFromProviders(request, options.Value.AdditionalTimeoutProviders) ?? options.Value.DefaultTimeoutProvider(request);
        }

        private RequestPriority GetPriority(HttpRequest request)
        {
            if (Enum.TryParse(request.Headers[HeaderNames.RequestPriority], true, out RequestPriority priority))
                return priority;

            return ObtainFromProviders(request, options.Value.AdditionalPriorityProviders) ?? options.Value.DefaultPriorityProvider(request);
        }

        private string GetClientApplicationIdentity(HttpRequest request)
        {
            var clientApplicationIdentity = request.Headers[HeaderNames.ApplicationIdentity].ToString();
            if (!string.IsNullOrEmpty(clientApplicationIdentity))
                return clientApplicationIdentity;

            return ObtainFromProviders(request, options.Value.AdditionalClientIdentityProviders);
        }
    }
}