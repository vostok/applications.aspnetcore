#if NET6_0_OR_GREATER
using JetBrains.Annotations;
using Vostok.Applications.AspNetCore.Builders;
using Vostok.Hosting.Abstractions;

namespace Vostok.Applications.AspNetCore.Helpers
{
    /// <summary>
    /// Delegate which configures <see cref="IVostokAspNetCoreWebApplicationBuilder"/>.
    /// </summary>
    [PublicAPI]
    public delegate void VostokAspNetCoreWebApplicationSetup(
        [NotNull] IVostokAspNetCoreWebApplicationBuilder builder,
        [NotNull] IVostokHostingEnvironment environment);
}
#endif