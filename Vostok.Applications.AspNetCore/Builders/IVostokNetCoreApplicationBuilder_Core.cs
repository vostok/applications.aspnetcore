#if NETCOREAPP
using System;
using JetBrains.Annotations;
using Microsoft.Extensions.Hosting;
using Vostok.Logging.Microsoft;

namespace Vostok.Applications.AspNetCore.Builders
{
    [PublicAPI]
    public partial interface IVostokNetCoreApplicationBuilder
    {
        IVostokNetCoreApplicationBuilder SetupGenericHost([NotNull] Action<IHostBuilder> setup);

        IVostokNetCoreApplicationBuilder SetupMicrosoftLog([NotNull] Action<VostokLoggerProviderSettings> setup);
    }
}
#endif