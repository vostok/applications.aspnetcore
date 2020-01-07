using System;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Hosting;
using Vostok.Applications.AspNetCore.Configuration;
using Vostok.Logging.Microsoft;

namespace Vostok.Applications.AspNetCore.Builders
{
    /// <summary>
    /// <para>Represents the configuration of <see cref="VostokAspNetCoreApplication{TStartup}"/> builder which can be customized during <see cref="VostokAspNetCoreApplication{TStartup}.Setup"/>.</para>
    /// <para>Allows to configure <see cref="IWebHostBuilder"/> and customize built-in Vostok middlewares.</para>
    /// </summary>
    [PublicAPI]
    public interface IVostokAspNetCoreApplicationBuilder
    {
        IVostokAspNetCoreApplicationBuilder SetupWebHost([NotNull] Action<IWebHostBuilder> setup);

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