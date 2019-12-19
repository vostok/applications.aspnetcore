using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Vostok.Commons.Helpers;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.AspNetCore.Helpers;
using Vostok.Hosting.AspNetCore.Setup;
using Vostok.Logging.Microsoft;

// ReSharper disable ParameterHidesMember

namespace Vostok.Hosting.AspNetCore.Builders
{
    internal class MicrosoftLogBuilder : IVostokMicrosoftLogBuilder
    {
        private readonly Customization<VostokLoggerProviderSettings> settingsCustomization;
        private volatile bool connectionLogScopeEnabled;
        private volatile bool hostingLogScopeEnabled;
        private volatile bool actionLogScopeEnabled;

        public MicrosoftLogBuilder()
        {
            settingsCustomization = new Customization<VostokLoggerProviderSettings>();
        }

        public ILoggerProvider Build(IVostokHostingEnvironment environment)
        {
            var settings = new VostokLoggerProviderSettings
            {
                IgnoredScopes = GetIgnoredScopes()
            };

            settingsCustomization.Customize(settings);

            return new VostokLoggerProvider(environment.Log, settings);
        }

        public IVostokMicrosoftLogBuilder CustomizeSettings(Action<VostokLoggerProviderSettings> settingsCustomization)
        {
            this.settingsCustomization.AddCustomization(settingsCustomization ?? throw new ArgumentNullException(nameof(settingsCustomization)));
            return this;
        }

        private HashSet<string> GetIgnoredScopes()
        {
            var ignoredScopes = new List<string>();

            if (!actionLogScopeEnabled)
                ignoredScopes.Add(MicrosoftConstants.ActionLogScope);

            if (!hostingLogScopeEnabled)
                ignoredScopes.Add(MicrosoftConstants.HostingLogScope);

            if (!connectionLogScopeEnabled)
                ignoredScopes.Add(MicrosoftConstants.ConnectionLogScope);

            return new HashSet<string>(ignoredScopes);
        }

        #region SetScopeEnabled

        public IVostokMicrosoftLogBuilder SetConnectionLogScopeIgnored(bool enabled)
        {
            connectionLogScopeEnabled = enabled;
            return this;
        }

        public IVostokMicrosoftLogBuilder SetHostingLogScopeIgnored(bool enabled)
        {
            hostingLogScopeEnabled = enabled;
            return this;
        }

        public IVostokMicrosoftLogBuilder SetActionLogScopeIgnored(bool enabled)
        {
            actionLogScopeEnabled = enabled;
            return this;
        }

        #endregion
    }
}