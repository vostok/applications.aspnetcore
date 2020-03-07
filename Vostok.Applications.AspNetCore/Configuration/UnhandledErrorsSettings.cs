using JetBrains.Annotations;

namespace Vostok.Applications.AspNetCore.Configuration
{
    [PublicAPI]
    public class UnhandledErrorsSettings
    {
        /// <summary>
        /// Error response code to be used when an unhandled exception is observed.
        /// </summary>
        public int RejectionResponseCode { get; set; } = 500;
    }
}
