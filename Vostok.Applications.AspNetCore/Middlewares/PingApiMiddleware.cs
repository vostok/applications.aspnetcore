using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Vostok.Applications.AspNetCore.Configuration;
using Vostok.Commons.Environment;
using Vostok.Commons.Threading;

namespace Vostok.Applications.AspNetCore.Middlewares
{
    internal class PingApiMiddleware : IMiddleware
    {
        private readonly PingApiSettings settings;
        private readonly AtomicBoolean initialized;

        private volatile string defaultCommitHash;

        public PingApiMiddleware([NotNull] PingApiSettings settings, AtomicBoolean initialized)
        {
            this.settings = settings;
            this.initialized = initialized;
        }

        public Task InvokeAsync(HttpContext context, RequestDelegate next)
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
            => initialized.Value ? settings.HealthCheck?.Invoke() ?? true ? "Ok" : "Warn" : "Init";

        private string ObtainCommitHash()
            => settings.CommitHashProvider?.Invoke() ?? (defaultCommitHash ?? (defaultCommitHash = AssemblyCommitHashExtractor.ExtractFromEntryAssembly()));
    }
}