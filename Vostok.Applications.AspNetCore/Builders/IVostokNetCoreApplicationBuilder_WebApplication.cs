#if NET6_0

using System;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;

namespace Vostok.Applications.AspNetCore.Builders
{
    [PublicAPI]
    public partial interface IVostokNetCoreApplicationBuilder
    {
        IVostokNetCoreApplicationBuilder SetupWebApplicationBuilder([NotNull] Action<WebApplicationBuilder> setup);
    }
}

#endif