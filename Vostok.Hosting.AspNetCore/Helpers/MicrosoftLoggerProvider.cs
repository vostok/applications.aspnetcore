using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal;
using Microsoft.Extensions.Logging;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Microsoft;

namespace Vostok.Hosting.AspNetCore.Helpers
{
    internal static class MicrosoftLoggerProvider
    {
        public static ILoggerProvider Get(ILog log)
        {
            return new VostokLoggerProvider(
                log,
                new VostokLoggerProviderSettings
                {
                    DisabledScopes = GetDisabledScopes()
                });
        }

        private static HashSet<Type> GetDisabledScopes() =>
            new HashSet<Type>(
                new List<Type>
                {
                    typeof(ConnectionLogScope),
                    GetHostingLogScopeType()
                }.Where(t => t != null));

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
    }
}