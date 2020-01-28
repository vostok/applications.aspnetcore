using System;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Hosting;
using Vostok.Applications.AspNetCore.Configuration;
using Vostok.Logging.Microsoft;

namespace Vostok.Applications.AspNetCore.Builders
{
    /// <summary>
    /// <para>Builds the configuration of <see cref="VostokAspNetCoreApplication{TStartup}"/>.</para>
    /// <para>Can be customized in app's <see cref="VostokAspNetCoreApplication{TStartup}.Setup"/> method.</para>
    /// <para>Allows to configure/disable <see cref="IWebHostBuilder"/> and customize built-in Vostok middlewares.</para>
    /// </summary>
    [PublicAPI]
    public interface IVostokAspNetCoreApplicationBuilder
    {
        IVostokAspNetCoreApplicationBuilder DisableWebHost();

        IVostokAspNetCoreApplicationBuilder SetupWebHost([NotNull] Action<IWebHostBuilder> setup);

        IVostokAspNetCoreApplicationBuilder SetupKestrel([NotNull] Action<KestrelSettings> setup);

        IVostokAspNetCoreApplicationBuilder SetupLogging([NotNull] Action<LoggingSettings> setup);

        IVostokAspNetCoreApplicationBuilder SetupTracing([NotNull] Action<TracingSettings> setup);

        IVostokAspNetCoreApplicationBuilder SetupThrottling([NotNull] Action<IVostokThrottlingBuilder> setup);

        IVostokAspNetCoreApplicationBuilder SetupDatacenterAwareness([NotNull] Action<DatacenterAwarenessSettings> setup);

        IVostokAspNetCoreApplicationBuilder SetupPingApi([NotNull] Action<PingApiSettings> setup);

        IVostokAspNetCoreApplicationBuilder SetupRequestInfoFilling([NotNull] Action<FillRequestInfoSettings> setup);

        IVostokAspNetCoreApplicationBuilder SetupDistributedContext([NotNull] Action<DistributedContextSettings> setup);
        
        IVostokAspNetCoreApplicationBuilder SetupMicrosoftLog([NotNull] Action<VostokLoggerProviderSettings> setup);
    }
}