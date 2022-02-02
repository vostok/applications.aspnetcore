using JetBrains.Annotations;
using Vostok.Applications.AspNetCore.Middlewares;

namespace Vostok.Applications.AspNetCore.Configuration
{
    /// <summary>
    /// Represents configuration of <see cref="UnhandledExceptionMiddleware"/>.
    /// </summary>
    [PublicAPI]
    public class UnhandledExceptionSettings
    {
        /// <summary>
        /// Error response code to be used when an unhandled exception is observed.
        /// </summary>
        public int ErrorResponseCode { get; set; } = 500;
    }
}