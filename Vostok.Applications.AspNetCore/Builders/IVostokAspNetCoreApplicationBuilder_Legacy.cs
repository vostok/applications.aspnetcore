#if !NETCOREAPP
using System;
using JetBrains.Annotations;
using Microsoft.Extensions.Hosting;

namespace Vostok.Applications.AspNetCore.Builders
{
    /// <summary>
    /// <para>Builds the configuration of <see cref="VostokAspNetCoreApplication{TStartup}"/>.</para>
    /// <para>Can be customized in app's <see cref="VostokAspNetCoreApplication{TStartup}.Setup"/> method.</para>
    /// </summary>
    public partial interface IVostokAspNetCoreApplicationBuilder
    {
    }
}
#endif