#if NET6_0
using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Vostok.Applications.AspNetCore.HostBuilders;
using Vostok.Logging.Microsoft;

namespace Vostok.Applications.AspNetCore.Builders
{
    internal class VostokNetCoreApplicationBuilder_WebApplication : IVostokNetCoreApplicationBuilder
    {
        private readonly WebApplicationHostFactory hostFactory;

        public VostokNetCoreApplicationBuilder_WebApplication(WebApplicationHostFactory hostFactory)
            => this.hostFactory = hostFactory;

        public IVostokNetCoreApplicationBuilder SetupWebApplicationBuilder(Action<WebApplicationBuilder> setup)
        {
            hostFactory.SetupBuilder(setup);
            return this;
        }

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