using JetBrains.Annotations;

namespace Vostok.Hosting.AspNetCore.Middlewares
{
    [PublicAPI]
    public class LoggingMiddlewareSettings
    {
        public bool LogQueryString { get; set; } = true;

        public bool LogRequestHeaders { get; set; } = false;

        public bool LogResponseHeaders { get; set; } = false;
    }
}