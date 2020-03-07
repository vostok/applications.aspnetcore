using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Vostok.Applications.AspNetCore.Configuration;
using Vostok.Commons.Environment;

namespace Vostok.Applications.AspNetCore.Middlewares
{
    internal class PingApiMiddleware
    {
        private readonly RequestDelegate next;
        private readonly IOptions<PingApiSettings> options;

        private volatile string defaultCommitHash;

        public PingApiMiddleware(RequestDelegate next, IOptions<PingApiSettings> options)
        {
            this.next = next;
            this.options = options;
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
            => options.Value.InitializationCheck?.Invoke() ?? true ? options.Value.HealthCheck?.Invoke() ?? true ? "Ok" : "Warn" : "Init";

        private string ObtainCommitHash()
            => options.Value.CommitHashProvider?.Invoke() ?? (defaultCommitHash ?? (defaultCommitHash = AssemblyCommitHashExtractor.ExtractFromEntryAssembly()));
    }
}