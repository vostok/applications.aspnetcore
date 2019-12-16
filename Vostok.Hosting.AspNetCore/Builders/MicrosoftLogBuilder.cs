using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Vostok.Commons.Helpers;
using Vostok.Hosting.Abstractions;
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
                DisabledScopes = GetDisabledScopes()
            };

            settingsCustomization.Customize(settings);

            return new VostokLoggerProvider(environment.Log, settings);
        }

        public IVostokMicrosoftLogBuilder CustomizeSettings(Action<VostokLoggerProviderSettings> settingsCustomization)
        {
            this.settingsCustomization.AddCustomization(settingsCustomization ?? throw new ArgumentNullException(nameof(settingsCustomization)));
            return this;
        }

        private HashSet<Type> GetDisabledScopes()
        {
            var disabledScopes = new List<Type>();

            if (!actionLogScopeEnabled)
                disabledScopes.Add(GetActionLogScopeType());

            if (!hostingLogScopeEnabled)
                disabledScopes.Add(GetHostingLogScopeType());

            //if (!connectionLogScopeEnabled)
            //    disabledScopes.Add(GetConnectionLogScopeType());

            return new HashSet<Type>(disabledScopes);
        }

        #region SetScopeEnabled

        public IVostokMicrosoftLogBuilder SetConnectionLogScopeEnabled(bool enabled)
        {
            connectionLogScopeEnabled = enabled;
            return this;
        }

        public IVostokMicrosoftLogBuilder SetHostingLogScopeEnabled(bool enabled)
        {
            hostingLogScopeEnabled = enabled;
            return this;
        }

        public IVostokMicrosoftLogBuilder SetActionLogScopeEnabled(bool enabled)
        {
            actionLogScopeEnabled = enabled;
            return this;
        }

        #endregion

        #region GetScopeTypes

        //private static Type GetConnectionLogScopeType() =>
        //    typeof(ConnectionLogScope);

        private static Type GetHostingLogScopeType()
        {
            try
            {
                var assembly = Assembly.Load("Microsoft.AspNetCore.Hosting");
                var type = assembly.GetType("Microsoft.AspNetCore.Hosting.Internal.HostingLoggerExtensions");
                var nested = type.GetNestedType("HostingLogScope", BindingFlags.NonPublic);
                return nested;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static Type GetActionLogScopeType()
        {
            try
            {
                var assembly = Assembly.Load("Microsoft.AspNetCore.Mvc.Core");
                var type = assembly.GetType("Microsoft.AspNetCore.Mvc.Internal.MvcCoreLoggerExtensions");
                var nested = type.GetNestedType("ActionLogScope", BindingFlags.NonPublic);
                return nested;
            }
            catch (Exception)
            {
                return null;
            }
        }

        #endregion
    }
}