#if NET6_0
using System;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

namespace Vostok.Applications.AspNetCore.Builders
{
    /// <summary>
    /// <para>Builds the configuration of <see cref="VostokAspNetCoreApplication"/>.</para>
    /// <para>Can be customized in app's <see cref="VostokAspNetCoreApplication.Setup"/> method.</para>
    /// <para>Allows to configure <see cref="WebApplicationBuilder"/>, <see cref="WebApplication"/> and customize built-in Vostok middlewares.</para>
    /// </summary>
    public partial interface IVostokAspNetCoreApplicationBuilder
    {
        /// <summary>
        /// Disables web host and HTTP server entirely.
        /// </summary>
        IVostokAspNetCoreApplicationBuilder DisableWebHost();

        [Obsolete("Use `IVostokAspNetCoreApplicationBuilder.SetupWebApplicationBuilder(webApplicationBuilder => webApplicationBuilder.Host...)` instead.")]
        IVostokAspNetCoreApplicationBuilder SetupGenericHost([NotNull] Action<IHostBuilder> setup);
        
        IVostokAspNetCoreApplicationBuilder SetupWebApplicationBuilder([NotNull] Action<WebApplicationBuilder> setup);

        IVostokAspNetCoreApplicationBuilder SetupWebApplication([NotNull] Action<WebApplication> setup);
    }
}
#endif