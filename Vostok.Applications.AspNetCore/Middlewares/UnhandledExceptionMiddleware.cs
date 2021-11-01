using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Vostok.Applications.AspNetCore.Configuration;
using Vostok.Applications.AspNetCore.Helpers;
using Vostok.Clusterclient.Core.Model;
using Vostok.Logging.Abstractions;

namespace Vostok.Applications.AspNetCore.Middlewares
{
    /// <summary>
    /// Catches and logs unhandled exception on low level. Upon catching one, serves an error response.
    /// </summary>
    [PublicAPI]
    public class UnhandledExceptionMiddleware
    {
        private readonly RequestDelegate next;
        private readonly UnhandledExceptionSettings options;
        private readonly ILog log;

        public UnhandledExceptionMiddleware(
            [NotNull] RequestDelegate next,
            [NotNull] IOptions<UnhandledExceptionSettings> options,
            [NotNull] ILog log)
        {
            this.options = (options ?? throw new ArgumentNullException(nameof(options))).Value;
            this.next = next ?? throw new ArgumentNullException(nameof(next));
            this.log = (log ?? throw new ArgumentNullException(nameof(log))).ForContext<UnhandledExceptionMiddleware>();
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception error)
            {
                if (IsCancellationError(error) && context.RequestAborted.IsCancellationRequested)
                {
                    log.Warn("Request has been canceled. This is likely due to connection close from client side.");
                    context.Response.StatusCode = (int) ResponseCode.Canceled;
                }
                else
                {
                    // (iloktionov): Log the exception here even if we're going to rethrow it later.
                    // (iloktionov): In that case, Kestrel internals will produce a second log event,
                    // (iloktionov): but the event logged here will have valuable tracing info attached.
                    log.Error(error, "An unhandled exception occurred during request processing. Response started = {ResponseHasStarted}.", context.Response.HasStarted);

                    // (iloktionov): It's not safe to swallow errors that happen during response body streaming.
                    // (iloktionov): This could lead to Kestrel not flushing its output buffers until the connection TTL expires.
                    if (context.Response.HasStarted)
                        throw;

                    context.Response.Clear();
                    context.Response.StatusCode = options.ErrorResponseCode;
                }
            }
        }

        private static bool IsCancellationError(Exception error)
            => error is TaskCanceledException || error is OperationCanceledException || error is ConnectionResetException;
    }
}