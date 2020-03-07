using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Vostok.Commons.Helpers;
using Vostok.Hosting.Abstractions;
using Vostok.Logging.Microsoft;

namespace Vostok.Applications.AspNetCore.Helpers
{
    internal class WebHostFactory
    {
        private readonly IVostokHostingEnvironment environment;

        private readonly Customization<VostokLoggerProviderSettings> loggerCustomization = new Customization<VostokLoggerProviderSettings>();

        public WebHostFactory(IVostokHostingEnvironment environment)
            => this.environment = environment;

        public IWebHostBuilder CreateHostBuilder()
        {
            var hostBuilder = WebHost.CreateDefaultBuilder();

            hostBuilder.ConfigureLogging(log => log.AddVostokLogging(environment, GetLoggerSettings()));

            hostBuilder.ConfigureAppConfiguration(config => config.AddVostokSources(environment));

            hostBuilder.ConfigureServices(services => services.AddVostokEnvironment(environment));

            return hostBuilder;
        }

        public void SetupLogger(Action<VostokLoggerProviderSettings> setup)
            => loggerCustomization.AddCustomization(setup ?? throw new ArgumentNullException(nameof(setup)));

        private VostokLoggerProviderSettings GetLoggerSettings()
            => loggerCustomization.Customize(new VostokLoggerProviderSettings());
    }
}
