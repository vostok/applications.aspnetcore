using JetBrains.Annotations;

namespace Vostok.Hosting.AspNetCore.Middlewares
{
    [PublicAPI]
    public class ThrottlingMiddlewareSettings
    {
        public int RejectionResponseCode { get; set; } = 429;
    }
}
