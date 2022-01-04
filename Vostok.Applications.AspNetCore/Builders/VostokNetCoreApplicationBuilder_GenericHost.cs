#if NETCOREAPP && !NET6_0
using System;
using Microsoft.Extensions.Hosting;
using Vostok.Applications.AspNetCore.HostBuilders;
using Vostok.Logging.Microsoft;

namespace Vostok.Applications.AspNetCore.Builders
{
    internal class VostokNetCoreApplicationBuilder_GenericHost : IVostokNetCoreApplicationBuilder
    {
        private readonly GenericHostFactory hostFactory;

        public VostokNetCoreApplicationBuilder_GenericHost(GenericHostFactory hostFactory)
            => this.hostFactory = hostFactory;

        public IVostokNetCoreApplicationBuilder SetupGenericHost(Action<IHostBuilder> setup)
        {
            hostFactory.SetupHost(setup);
            return this;
        }

        public IVostokNetCoreApplicationBuilder SetupMicrosoftLog(Action<VostokLoggerProviderSettings> setup)
        {
            hostFactory.SetupLogger(setup);
            return this;
        }
    }
}
#endif