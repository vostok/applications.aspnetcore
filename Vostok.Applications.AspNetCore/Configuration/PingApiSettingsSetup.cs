using System;
using System.Reflection;
using JetBrains.Annotations;
using Vostok.Commons.Environment;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Abstractions.Diagnostics;

namespace Vostok.Applications.AspNetCore.Configuration
{
    [PublicAPI]
    public static class PingApiSettingsSetup
    {
        public static Action<PingApiSettings> Get(IVostokHostingEnvironment environment, Type applicationType, Func<bool> initialization) =>
            settings =>
            {
                settings.CommitHashProvider = () => AssemblyCommitHashExtractor.ExtractFromAssembly(Assembly.GetAssembly(applicationType));
                settings.InitializationCheck = initialization;

                if (environment.HostExtensions.TryGet<IVostokApplicationDiagnostics>(out var diagnostics))
                    settings.HealthCheck = () => diagnostics.HealthTracker.CurrentStatus == HealthStatus.Healthy;
            };
    }
}