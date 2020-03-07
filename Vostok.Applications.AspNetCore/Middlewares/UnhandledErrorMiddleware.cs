using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Vostok.Applications.AspNetCore.Configuration;
using Vostok.Logging.Abstractions;

namespace Vostok.Applications.AspNetCore.Middlewares
{
    /// <summary>
    /// Catches and logs unhandled exception on low level. Upon catching one, serves an error response.
    /// </summary>
    [PublicAPI]
    public class UnhandledErrorMiddleware
    {
        private readonly RequestDelegate next;
        private readonly IOptions<UnhandledErrorsSettings> options;
        private readonly ILog log;

        public UnhandledErrorMiddleware(
            [NotNull] RequestDelegate next,
            [NotNull] IOptions<UnhandledErrorsSettings> options,
            [NotNull] ILog log)
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));
            this.next = next ?? throw new ArgumentNullException(nameof(next));
            this.log = (log ?? throw new ArgumentNullException(nameof(log))).ForContext<UnhandledErrorMiddleware>();
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception error)
            {
                if (IsCancellationError(error))
                {
                    log.Warn("Request has been canceled. This is likely due to connection close from client side.");
                }
                else
                {
                    log.Error(error, "An unhandled exception occurred during request processing.");

                    RespondWithError(context);
                }
            }
        }

        private static bool IsCancellationError(Exception error)
            => error is TaskCanceledException || error is OperationCanceledException || error is ConnectionResetException;

        private void RespondWithError(HttpContext context)
        {
            var response = context.Response;
            if (response.HasStarted)
                return;

            response.StatusCode = options.Value.ErrorResponseCode;
        }
    }
}