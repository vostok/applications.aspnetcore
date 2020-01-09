using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;

namespace Vostok.Applications.AspNetCore.Configuration
{
    internal static class LoggingCollectionSettingsExtensions
    {
        public static bool IsEnabledForRequest([NotNull] this LoggingCollectionSettings settings, [NotNull] HttpRequest request) =>
            settings.Enabled(request);

        public static bool IsEnabledForAllKeys([NotNull] this LoggingCollectionSettings settings) =>
            settings.Whitelist == null && settings.Blacklist == null;

        public static bool IsEnabledForKey([NotNull] this LoggingCollectionSettings settings, [NotNull] string key)
        {
            if (settings.Whitelist == null)
                return settings.Blacklist == null || !settings.Blacklist.Contains(key);

            return settings.Whitelist.Contains(key);
        }

        public static LoggingCollectionSettings ToCaseInsensitive([CanBeNull] this LoggingCollectionSettings settings)
            => settings == null
                ? null
                : new LoggingCollectionSettings(settings.Enabled)
                {
                    Whitelist = settings.Whitelist == null ? null : new HashSet<string>(settings.Whitelist, StringComparer.OrdinalIgnoreCase),
                    Blacklist = settings.Blacklist == null ? null : new HashSet<string>(settings.Blacklist, StringComparer.OrdinalIgnoreCase)
                };
    }
}
