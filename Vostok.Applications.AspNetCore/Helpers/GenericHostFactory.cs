#if NETCOREAPP3_1
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Vostok.Commons.Helpers;
using Vostok.Commons.Time;
using Vostok.Hosting.Abstractions;
using Vostok.Logging.Microsoft;

namespace Vostok.Applications.AspNetCore.Helpers
{
    internal class GenericHostFactory
    {
        private readonly IVostokHostingEnvironment environment;

        private readonly Customization<IHostBuilder> hostCustomization = new Customization<IHostBuilder>();
        private readonly Customization<VostokLoggerProviderSettings> loggerCustomization = new Customization<VostokLoggerProviderSettings>();

        public GenericHostFactory(IVostokHostingEnvironment environment)
            => this.environment = environment;

        public IHost CreateHost()
            => CreateHostBuilder().Build();

        public IHostBuilder CreateHostBuilder()
        {
            var hostBuilder = Host.CreateDefaultBuilder();

            hostBuilder.ConfigureHostConfiguration(config => config.AddDefaultLoggingFilters());

            hostBuilder.ConfigureLogging(log => log.AddVostokLogging(environment, GetLoggerSettings()));

            hostBuilder.ConfigureAppConfiguration(config => config.AddVostokSources(environment));

            hostBuilder.ConfigureServices(
                services =>
                {
                    services.AddSingleton<IHostLifetime, GenericHostEmptyLifetime>();
                    
                    services.AddVostokEnvironment(environment);

                    services.Configure<HostOptions>(options => options.ShutdownTimeout = environment.ShutdownTimeout.Cut(100.Milliseconds(), 0.05));
                });

            hostCustomization.Customize(new GenericHostBuilderWrapper(hostBuilder));

            return hostBuilder;
        }

        public void SetupHost(Action<IHostBuilder> setup)
            => hostCustomization.AddCustomization(setup ?? throw new ArgumentNullException(nameof(setup)));

        public void SetupLogger(Action<VostokLoggerProviderSettings> setup)
            => loggerCustomization.AddCustomization(setup ?? throw new ArgumentNullException(nameof(setup)));

        private VostokLoggerProviderSettings GetLoggerSettings()
            => loggerCustomization.Customize(new VostokLoggerProviderSettings());
    }
}
#endif