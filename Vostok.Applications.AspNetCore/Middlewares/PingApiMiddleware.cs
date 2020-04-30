using System;
using System.Text;
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
        private readonly PingApiSettings options;
        private volatile string commitHash;

        public PingApiMiddleware(
            [NotNull] RequestDelegate next,
            [NotNull] IOptions<PingApiSettings> options)
        {
            this.next = next ?? throw new ArgumentNullException(nameof(next));
            this.options = (options ?? throw new ArgumentNullException(nameof(options))).Value;
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

        private Task HandlePingRequest(HttpContext context) =>
            HandleRequest(context, "{" + $"\"Status\":\"{GetHealthStatus()}\"" + "}");

        private Task HandleVersionRequest(HttpContext context) =>
            HandleRequest(context, "{" + $"\"CommitHash\":\"{ObtainCommitHash()}\"" + "}");

        private Task HandleRequest(HttpContext context, string text)
        {
            context.Response.StatusCode = 200;
            context.Response.ContentType = "application/json";

            var body = Encoding.UTF8.GetBytes(text);
            context.Response.ContentLength = body.Length;
            return context.Response.Body.WriteAsync(body, 0, body.Length);
        }

        private string GetHealthStatus()
        {
            var isInitialized = options.InitializationCheck?.Invoke() ?? true;
            if (isInitialized)
                return options.HealthCheck?.Invoke() ?? true ? "Ok" : "Warn";

            return "Init";
        }

        private string ObtainCommitHash()
            => commitHash ?? (commitHash = options.CommitHashProvider?.Invoke() ??
                                           AssemblyCommitHashExtractor.ExtractFromEntryAssembly());
    }
}