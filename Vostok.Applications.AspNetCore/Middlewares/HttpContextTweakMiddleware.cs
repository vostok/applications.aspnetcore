using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Vostok.Applications.AspNetCore.Configuration;
using Vostok.Applications.AspNetCore.Helpers;
using Vostok.Logging.Abstractions;

namespace Vostok.Applications.AspNetCore.Middlewares
{
    /// <summary>
    /// Applies performance tweaks to <see cref="HttpContext"/>.
    /// </summary>
    [PublicAPI]
    public class HttpContextTweakMiddleware
    {
        private const string HTTP_1_1 = "HTTP/1.1";
        private const string HTTP_1_0 = "HTTP/1.0";

        private readonly RequestDelegate next;
        private readonly HttpContextTweakSettings options;
        private readonly ILog log;

        public HttpContextTweakMiddleware(
            [NotNull] RequestDelegate next,
            [NotNull] IOptions<HttpContextTweakSettings> options,
            [NotNull] ILog log)
        {
            this.next = next ?? throw new ArgumentNullException(nameof(next));
            this.options = (options ?? throw new ArgumentNullException(nameof(options))).Value;
            this.log = (log ?? throw new ArgumentNullException(nameof(log))).ForContext<HttpContextTweakMiddleware>();
        }

        public Task InvokeAsync(HttpContext context)
        {
            try
            {
                if (options.EnableResponseWriteCallSizeLimit && IsOlderThanHttp2(context))
                    context.Response.Body = new ResponseStreamWrapper(context.Response.Body, options.MaxResponseWriteCallSize);
            }
            catch (Exception error)
            {
                log.Error(error);
            }

            return next(context);
        }

        private static bool IsOlderThanHttp2(HttpContext context)
        {
            var protocol = context.Request.Protocol;

            return protocol.Equals(HTTP_1_1, StringComparison.OrdinalIgnoreCase) || protocol.Equals(HTTP_1_0, StringComparison.OrdinalIgnoreCase);
        }
    }
}