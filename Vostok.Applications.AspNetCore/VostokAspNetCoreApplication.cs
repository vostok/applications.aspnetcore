#if !NET6_0

using JetBrains.Annotations;
using Microsoft.AspNetCore.Hosting;
using Vostok.Applications.AspNetCore.Builders;
using Vostok.Applications.AspNetCore.Models;

namespace Vostok.Applications.AspNetCore
{
    /// <summary>
    /// <para><see cref="VostokAspNetCoreApplication"/> is the abstract class developers inherit from in order to create Vostok-compatible AspNetCore services without using <c>Startup</c> class.</para>
    /// <para>Implement <see cref="VostokAspNetCoreApplication{TStartup}.Setup"/> method to configure <see cref="IWebHostBuilder"/> and customize built-in Vostok middlewares (see <see cref="IVostokAspNetCoreApplicationBuilder"/>).</para>
    /// <para>Override <see cref="VostokAspNetCoreApplication{TStartup}.WarmupAsync"/> method to perform any additional initialization after the DI container gets built but before the app gets registered in service discovery.</para>
    /// </summary>
    [PublicAPI]
    public abstract class VostokAspNetCoreApplication : VostokAspNetCoreApplication<EmptyStartup>
    {
    }
}

#endif