using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Vostok.Applications.AspNetCore.Configuration;
using Vostok.Logging.Abstractions;

namespace Vostok.Applications.AspNetCore.Middlewares
{
    internal class UnhandledErrorMiddleware
    {
        private readonly RequestDelegate next;
        private readonly IOptions<UnhandledErrorsSettings> options;
        private readonly ILog log;

        public UnhandledErrorMiddleware(RequestDelegate next, IOptions<UnhandledErrorsSettings> options, ILog log)
        {
            this.options = options;
            this.next = next;
            this.log = log;
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

            response.StatusCode = options.Value.RejectionResponseCode;
        }
    }
}