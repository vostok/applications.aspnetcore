using System.Linq;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;

namespace Vostok.Hosting.AspNetCore.Configuration
{
    internal static class LoggingCollectionSettingsExtensions
    {
        public static bool IsEnabledForRequest([NotNull] this LoggingCollectionSettings settings, [NotNull] HttpRequest request) =>
            settings.Enabled(request);

        public static bool IsEnabledForAllKeys([NotNull] this LoggingCollectionSettings settings) =>
            settings.WhitelistKeys == null && settings.BlacklistKeys == null;

        public static bool IsEnabledForKey([NotNull] this LoggingCollectionSettings settings, [NotNull] string key)
        {
            if (settings.WhitelistKeys == null)
                return settings.BlacklistKeys == null || !settings.BlacklistKeys.Contains(key);

            return settings.WhitelistKeys.Contains(key);
        }
    }
}