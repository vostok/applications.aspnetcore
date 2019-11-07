using System;
using JetBrains.Annotations;
using Vostok.Logging.Microsoft;

namespace Vostok.Hosting.AspNetCore.Setup
{
    [PublicAPI]
    public interface IVostokMicrosoftLogBuilder
    {
        IVostokMicrosoftLogBuilder CustomizeSettings([NotNull] Action<VostokLoggerProviderSettings> settingsCustomization);
    }
}