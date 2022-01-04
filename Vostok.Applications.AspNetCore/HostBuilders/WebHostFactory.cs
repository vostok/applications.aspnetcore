﻿using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
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

        private readonly Customization<IWebHostBuilder> hostCustomization = new Customization<IWebHostBuilder>();
        private readonly Customization<VostokLoggerProviderSettings> loggerCustomization = new Customization<VostokLoggerProviderSettings>();

        public WebHostFactory(IVostokHostingEnvironment environment, IVostokApplication application)
        {
            this.environment = environment;
            this.application = application;
        }

        public IWebHost CreateHost()
            => CreateHostBuilder().Build();
        
        private IWebHostBuilder CreateHostBuilder()
        {
            var hostBuilder = WebHost.CreateDefaultBuilder();

            hostBuilder.ConfigureLogging(log => log.AddVostokLogging(environment, GetLoggerSettings()));

            hostBuilder.ConfigureAppConfiguration(config => config.AddVostokSources(environment));

            hostBuilder.ConfigureServices(services => services.AddVostokEnvironment(environment, application));

            hostCustomization.Customize(hostBuilder);
            
            return hostBuilder;
        }

        public void SetupHost(Action<IWebHostBuilder> setup)
            => hostCustomization.AddCustomization(setup ?? throw new ArgumentNullException(nameof(setup)));
        
        public void SetupLogger(Action<VostokLoggerProviderSettings> setup)
            => loggerCustomization.AddCustomization(setup ?? throw new ArgumentNullException(nameof(setup)));

        private VostokLoggerProviderSettings GetLoggerSettings()
            => loggerCustomization.Customize(new VostokLoggerProviderSettings());
    }
}