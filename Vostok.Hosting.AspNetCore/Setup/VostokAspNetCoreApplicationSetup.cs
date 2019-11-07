using JetBrains.Annotations;

namespace Vostok.Hosting.AspNetCore.Setup
{
    /// <summary>
    /// Delegate which configures <see cref="VostokAspNetCoreApplication"/>.
    /// </summary>
    [PublicAPI]
    public delegate void VostokAspNetCoreApplicationSetup([NotNull] IVostokAspNetCoreApplicationBuilder builder);
}