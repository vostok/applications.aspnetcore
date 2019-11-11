using System;
using JetBrains.Annotations;
using Vostok.Logging.Microsoft;

namespace Vostok.Hosting.AspNetCore.Setup
{
    [PublicAPI]
    public interface IVostokMicrosoftLogBuilder
    {
        IVostokMicrosoftLogBuilder SetConnectionLogScopeEnabled(bool enabled);

        IVostokMicrosoftLogBuilder SetHostingLogScopeEnabled(bool enabled);

        IVostokMicrosoftLogBuilder SetActionLogScopeEnabled(bool enabled);

        IVostokMicrosoftLogBuilder CustomizeSettings([NotNull] Action<VostokLoggerProviderSettings> settingsCustomization);
    }
}