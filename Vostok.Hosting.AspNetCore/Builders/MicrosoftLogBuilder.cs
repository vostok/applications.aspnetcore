using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal;
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

        private HashSet<Type> GetDisabledScopes() =>
            new HashSet<Type>(
                new List<Type>
                {
                    GetConnectionLogScopeType(),
                    GetHostingLogScopeType(),
                    GetActionLogScopeType()
                }.Where(t => t != null));

        #region GetScopeTypes

        private static Type GetConnectionLogScopeType() =>
            typeof(ConnectionLogScope);

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