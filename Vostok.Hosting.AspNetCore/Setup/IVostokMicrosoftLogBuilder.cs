using System;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Microsoft;

namespace Vostok.Hosting.AspNetCore.Setup
{
    /// <summary>
    /// Represents a Vostok <see cref="ILog"/> builder configuration that will be used as Microsoft <see cref="ILoggerProvider"/>.
    /// </summary>
    [PublicAPI]
    public interface IVostokMicrosoftLogBuilder
    {
        /// <summary>
        /// <para>Sets whether or not to log ConnectionLogScope.</para>
        /// <para>Disabled by default.</para>
        /// </summary>
        IVostokMicrosoftLogBuilder SetConnectionLogScopeEnabled(bool enabled);

        /// <summary>
        /// <para>Sets whether or not to log HostingLogScope.</para>
        /// <para>Disabled by default.</para>
        /// </summary>
        IVostokMicrosoftLogBuilder SetHostingLogScopeEnabled(bool enabled);

        /// <summary>
        /// <para>Sets whether or not to log ActionLogScope.</para>
        /// <para>Disabled by default.</para>
        /// </summary>
        IVostokMicrosoftLogBuilder SetActionLogScopeEnabled(bool enabled);

        /// <summary>
        /// Delegate which configures <see cref="VostokLoggerProviderSettings"/>.
        /// </summary>
        IVostokMicrosoftLogBuilder CustomizeSettings([NotNull] Action<VostokLoggerProviderSettings> settingsCustomization);
    }
}