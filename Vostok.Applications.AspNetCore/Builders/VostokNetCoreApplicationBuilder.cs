using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Vostok.Applications.AspNetCore.Helpers;
using Vostok.Commons.Helpers;
using Vostok.Hosting.Abstractions;
using Vostok.Logging.Microsoft;

namespace Vostok.Applications.AspNetCore.Builders
{
    internal class VostokNetCoreApplicationBuilder : IVostokNetCoreApplicationBuilder
    {
        private readonly IVostokHostingEnvironment environment;
        private readonly Customization<IHostBuilder> genericHostCustomization;
        private readonly Customization<VostokLoggerProviderSettings> microsoftLogCustomization;

        public VostokNetCoreApplicationBuilder(IVostokHostingEnvironment environment)
        {
            this.environment = environment;

            genericHostCustomization = new Customization<IHostBuilder>();
            microsoftLogCustomization = new Customization<VostokLoggerProviderSettings>();
        }

        public IHost BuildHost() =>
            CreateHostBuilder().Build();

        public IHostBuilder CreateHostBuilder()
        {
            var hostBuilder = Host.CreateDefaultBuilder();

            hostBuilder.ConfigureLogging(log => log.AddVostokLogging(environment, microsoftLogCustomization.Customize(new VostokLoggerProviderSettings())));

            hostBuilder.ConfigureAppConfiguration(config => config.AddVostokSources(environment));

            hostBuilder.ConfigureServices(services => services.AddSingleton<IHostLifetime, EmptyHostLifetime>());

            hostBuilder.ConfigureServices(services => services.AddVostokEnvironment(environment));

            genericHostCustomization.Customize(new HostBuilderWrapper(hostBuilder));

            return hostBuilder;
        }

        public IVostokNetCoreApplicationBuilder SetupGenericHost(Action<IHostBuilder> setup)
        {
            genericHostCustomization.AddCustomization(setup ?? throw new ArgumentNullException(nameof(setup)));
            return this;
        }

        public IVostokNetCoreApplicationBuilder SetupMicrosoftLog(Action<VostokLoggerProviderSettings> setup)
        {
            microsoftLogCustomization.AddCustomization(setup ?? throw new ArgumentNullException(nameof(setup)));
            return this;
        }
    }
}