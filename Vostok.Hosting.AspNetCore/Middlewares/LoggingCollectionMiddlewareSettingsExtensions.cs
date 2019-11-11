using System.Linq;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;

namespace Vostok.Hosting.AspNetCore.Middlewares
{
    internal static class LoggingCollectionMiddlewareSettingsExtensions
    {
        public static bool IsEnabledForRequest([NotNull] this LoggingCollectionMiddlewareSettings settings, [NotNull] HttpRequest request) =>
            settings.Enabled(request);

        public static bool IsEnabledForAllKeys([NotNull] this LoggingCollectionMiddlewareSettings settings) =>
            settings.WhitelistKeys == null;

        public static bool IsEnabledForKey([NotNull] this LoggingCollectionMiddlewareSettings settings, [NotNull] string key) =>
            settings.WhitelistKeys == null || settings.WhitelistKeys.Contains(key);
    }
}