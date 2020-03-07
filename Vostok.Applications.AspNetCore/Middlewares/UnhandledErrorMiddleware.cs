using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Http;
using Vostok.Logging.Abstractions;

namespace Vostok.Applications.AspNetCore.Middlewares
{
    internal class UnhandledErrorMiddleware
    {
        private readonly ILog log;

        public UnhandledErrorMiddleware(ILog log)
            => this.log = log;

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
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

        private static void RespondWithError(HttpContext context)
        {
            var response = context.Response;
            if (response.HasStarted)
                return;

            response.StatusCode = 500;
        }
    }
}