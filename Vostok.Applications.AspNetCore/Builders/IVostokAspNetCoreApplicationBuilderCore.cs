#if NETCOREAPP
using System;
using JetBrains.Annotations;
using Microsoft.Extensions.Hosting;

namespace Vostok.Applications.AspNetCore.Builders
{
    public partial interface IVostokAspNetCoreApplicationBuilder
    {
        /// <summary>
        /// Disables web host and HTTP server entirely.
        /// </summary>
        IVostokAspNetCoreApplicationBuilder DisableWebHost();

        /// <summary>
        /// Applies an arbitrary customization to generic host.
        /// </summary>
        IVostokAspNetCoreApplicationBuilder SetupGenericHost([NotNull] Action<IHostBuilder> setup);
    }
}
#endif