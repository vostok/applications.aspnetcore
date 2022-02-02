using System;
using System.Reflection;
using Vostok.Commons.Environment;
using Vostok.Commons.Threading;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Abstractions.Diagnostics;

namespace Vostok.Applications.AspNetCore.Configuration
{
    internal static class PingApiSettingsSetup
    {
        public static Action<PingApiSettings> Get(IVostokHostingEnvironment environment, Type applicationType, AtomicBoolean initialized) =>
            settings =>
            {
                settings.CommitHashProvider = () => AssemblyCommitHashExtractor.ExtractFromAssembly(Assembly.GetAssembly(applicationType));
                settings.InitializationCheck = () => initialized;

                if (environment.HostExtensions.TryGet<IVostokApplicationDiagnostics>(out var diagnostics))
                    settings.HealthCheck = () => diagnostics.HealthTracker.CurrentStatus == HealthStatus.Healthy;
            };
    }
}