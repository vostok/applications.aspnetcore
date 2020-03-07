using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Vostok.Applications.AspNetCore.Configuration;
using Vostok.Commons.Environment;

namespace Vostok.Applications.AspNetCore.Middlewares
{
    internal class PingApiMiddleware : IMiddleware
    {
        private readonly PingApiSettings settings;

        private volatile string defaultCommitHash;

        public PingApiMiddleware([NotNull] PingApiSettings settings)
        {
            this.settings = settings;
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
            => settings.InitializationCheck?.Invoke() ?? true ? settings.HealthCheck?.Invoke() ?? true ? "Ok" : "Warn" : "Init";

        private string ObtainCommitHash()
            => settings.CommitHashProvider?.Invoke() ?? (defaultCommitHash ?? (defaultCommitHash = AssemblyCommitHashExtractor.ExtractFromEntryAssembly()));
    }
}