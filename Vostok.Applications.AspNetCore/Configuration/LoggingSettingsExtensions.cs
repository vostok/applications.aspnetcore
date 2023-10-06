using JetBrains.Annotations;

namespace Vostok.Applications.AspNetCore.Configuration
{
    [PublicAPI]
    internal static class LoggingSettingsExtensions
    {
        /// <summary>
        /// Determines whether to response codes of outbound responses for log.
        /// </summary>
        public static void SetLogResponseStatusCodes([NotNull] this LoggingSettings settings, params int[] logResponseStatusCodes) =>
            settings.LogResponseStatusCodes = logResponseStatusCodes;
    }
}