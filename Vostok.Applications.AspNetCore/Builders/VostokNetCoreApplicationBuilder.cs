#if NETCOREAPP3_1
using System;
using Microsoft.Extensions.Hosting;
using Vostok.Applications.AspNetCore.Helpers;
using Vostok.Logging.Microsoft;

namespace Vostok.Applications.AspNetCore.Builders
{
    internal class VostokNetCoreApplicationBuilder : IVostokNetCoreApplicationBuilder
    {
        private readonly GenericHostFactory hostFactory;

        public VostokNetCoreApplicationBuilder(GenericHostFactory hostFactory)
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