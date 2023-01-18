using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Vostok.Applications.AspNetCore.Helpers;
using Vostok.Commons.Helpers;
using Vostok.Hosting.Abstractions;
using Vostok.Logging.Microsoft;

namespace Vostok.Applications.AspNetCore.HostBuilders
{
    internal class WebHostFactory
    {
        private readonly IVostokHostingEnvironment environment;
        private readonly IVostokApplication application;
        private readonly VostokDisposables disposables;

        private readonly Customization<VostokLoggerProviderSettings> loggerCustomization = new Customization<VostokLoggerProviderSettings>();

        public WebHostFactory(IVostokHostingEnvironment environment, IVostokApplication application, VostokDisposables disposables)
        {
            this.environment = environment;
            this.application = application;
            this.disposables = disposables;
        }

        public IWebHostBuilder CreateHostBuilder()
        {
            var hostBuilder = WebHost.CreateDefaultBuilder();

            hostBuilder.ConfigureLogging(log => log.AddVostokLogging(environment, GetLoggerSettings()));

            hostBuilder.ConfigureAppConfiguration(config => config.AddVostokSources(environment));

            hostBuilder.ConfigureServices(services =>
            {
                services.AddSingleton(disposables);
                services.AddVostokEnvironment(environment, application);
            });

            return hostBuilder;
        }

        public void SetupLogger(Action<VostokLoggerProviderSettings> setup)
            => loggerCustomization.AddCustomization(setup ?? throw new ArgumentNullException(nameof(setup)));

        private VostokLoggerProviderSettings GetLoggerSettings()
            => loggerCustomization.Customize(new VostokLoggerProviderSettings());
    }
}