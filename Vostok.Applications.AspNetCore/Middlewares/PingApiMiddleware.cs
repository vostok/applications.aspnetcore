using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Vostok.Applications.AspNetCore.Configuration;
using Vostok.Commons.Environment;

namespace Vostok.Applications.AspNetCore.Middlewares
{
    /// <summary>
    /// Handles diagnostic <c>/_status/ping</c> and <c>/_status/version</c> requests.
    /// </summary>
    [PublicAPI]
    public class PingApiMiddleware
    {
        private readonly RequestDelegate next;
        private readonly IOptions<PingApiSettings> options;
        private volatile string commitHash;

        public PingApiMiddleware(
            [NotNull] RequestDelegate next,
            [NotNull] IOptions<PingApiSettings> options)
        {
            this.next = next ?? throw new ArgumentNullException(nameof(next));
            this.options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public Task InvokeAsync(HttpContext context)
        {
            var request = context.Request;

            switch (request.Path.Value)
            {
                case "/_status/ping":
                    if (!HttpMethods.IsGet(request.Method))
                        break;

                    return HandlePingRequest(context);

                case "/_status/version":
                    if (!HttpMethods.IsGet(request.Method))
                        break;

                    return HandleVersionRequest(context);
            }

            return next.Invoke(context);
        }

        private Task HandlePingRequest(HttpContext context)
        {
            context.Response.StatusCode = 200;
            return context.Response.WriteAsync("{" + $"\"Status\":\"{GetHealthStatus()}\"" + "}");
        }

        private Task HandleVersionRequest(HttpContext context)
        {
            context.Response.StatusCode = 200;
            return context.Response.WriteAsync("{" + $"\"CommitHash\":\"{ObtainCommitHash()}\"" + "}");
        }

        private string GetHealthStatus()
        {
            var isInitialized = options.Value.InitializationCheck?.Invoke() ?? true;
            if (isInitialized)
                return options.Value.HealthCheck?.Invoke() ?? true ? "Ok" : "Warn";

            return "Init";
        }

        private string ObtainCommitHash()
            => commitHash ?? (commitHash = options.Value.CommitHashProvider?.Invoke() ?? AssemblyCommitHashExtractor.ExtractFromEntryAssembly());
    }
}