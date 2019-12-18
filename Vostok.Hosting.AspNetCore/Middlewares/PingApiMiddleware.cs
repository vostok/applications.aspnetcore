using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;

namespace Vostok.Hosting.AspNetCore.Middlewares
{
    // CR(iloktionov): Почему у этой штуки нет дефолтной реализации?

    /// <summary>
    /// Middleware that handles '_status/ping' and '/_status/version' requests.
    /// </summary>
    internal class PingApiMiddleware : IMiddleware
    {
        private readonly PingApiMiddlewareSettings settings;

        public PingApiMiddleware([NotNull] PingApiMiddlewareSettings settings)
        {
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
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
            return context.Response.WriteAsync("{" + $"\"Status\":\"{settings.StatusProvider()}\"" + "}");
        }

        private Task HandleVersionRequest(HttpContext context)
        {
            context.Response.StatusCode = 200;
            return context.Response.WriteAsync("{" + $"\"CommitHash\":\"{settings.CommitHashProvider()}\"" + "}");
        }
    }
}