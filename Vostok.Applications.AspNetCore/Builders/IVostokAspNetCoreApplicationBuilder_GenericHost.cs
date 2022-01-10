#if NETCOREAPP && !NET6_0
using System;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Vostok.Applications.AspNetCore.Builders
{
    /// <summary>
    /// <para>Builds the configuration of <see cref="VostokAspNetCoreApplication{TStartup}"/>.</para>
    /// <para>Can be customized in app's <see cref="VostokAspNetCoreApplication{TStartup}.Setup"/> method.</para>
    /// <para>Allows to configure/disable <see cref="IWebHostBuilder"/> and customize built-in Vostok middlewares.</para>
    /// </summary>
    public partial interface IVostokAspNetCoreApplicationBuilder
    {
        /// <summary>
        /// Disables web host and HTTP server entirely.
        /// </summary>
        IVostokAspNetCoreApplicationBuilder DisableWebHost();

        IVostokAspNetCoreApplicationBuilder SetupGenericHost([NotNull] Action<IHostBuilder> setup);
    }
}
#endif