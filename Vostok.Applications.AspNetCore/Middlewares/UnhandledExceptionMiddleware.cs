using System;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Vostok.Applications.AspNetCore.Configuration;
using Vostok.Clusterclient.Core.Model;
using Vostok.Logging.Abstractions;

namespace Vostok.Applications.AspNetCore.Middlewares
{
    /// <summary>
    /// Catches and logs unhandled exception on low level. Upon catching one, serves an error response.
    /// </summary>
    /// <remarks>
    /// Beware that DeveloperExceptionPageMiddleware might be enabled by default starting from dotnet 6.0. It behaves differently
    /// in different versions of dotnet, and it might mess up your exception handling.
    ///
    /// Starting from dotnet6 it overwrites response status code with 400 if <see cref="BadHttpRequestException"/> was thrown. 
    /// Starting from dotnet8 it overwrites response status code with 499 if request was aborted.
    /// 
    /// You can see it for yourself by comparing implementations:
    /// <see href="https://github.com/dotnet/aspnetcore/blob/v5.0.17/src/Middleware/Diagnostics/src/DeveloperExceptionPage/DeveloperExceptionPageMiddleware.cs"/>
    /// <see href="https://github.com/dotnet/aspnetcore/blob/v6.0.0/src/Middleware/Diagnostics/src/DeveloperExceptionPage/DeveloperExceptionPageMiddleware.cs"/>
    /// <see href="https://github.com/dotnet/aspnetcore/blob/v8.0.0/src/Middleware/Diagnostics/src/DeveloperExceptionPage/DeveloperExceptionPageMiddlewareImpl.cs"/>
    /// </remarks>
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
                if (ShouldIgnoreError(error) && context.RequestAborted.IsCancellationRequested)
                {
                    log.Warn("Request has been canceled. This is likely due to connection close from client side.");
                    context.Response.StatusCode = (int)ResponseCode.Canceled;
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

        private bool ShouldIgnoreError(Exception error)
        {
            var errorType = error.GetType();
            return options.ExceptionsToIgnore.Any(x => x.IsAssignableFrom(errorType));
        }
    }
}