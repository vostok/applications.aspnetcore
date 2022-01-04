#if NETCOREAPP
using System;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Vostok.Logging.Microsoft;

namespace Vostok.Applications.AspNetCore.Builders
{
    /// <summary>
    /// <para>Builds the configuration of <see cref="VostokNetCoreApplication"/>.</para>
    /// <para>Can be customized in app's <see cref="VostokNetCoreApplication.Setup"/> method.</para>
    /// </summary>
    [PublicAPI]
    public interface IVostokNetCoreApplicationBuilder
    {
#if NET6_0
        IVostokNetCoreApplicationBuilder SetupWebApplicationBuilder([NotNull] Action<WebApplicationBuilder> setup);
#endif
        
        IVostokNetCoreApplicationBuilder SetupGenericHost([NotNull] Action<IHostBuilder> setup);

        IVostokNetCoreApplicationBuilder SetupMicrosoftLog([NotNull] Action<VostokLoggerProviderSettings> setup);
    }
}
#endif