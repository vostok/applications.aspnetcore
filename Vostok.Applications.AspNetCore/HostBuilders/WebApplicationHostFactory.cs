#if NET6_0
using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Vostok.Applications.AspNetCore.Helpers;
using Vostok.Commons.Helpers;
using Vostok.Commons.Time;
using Vostok.Hosting.Abstractions;
using Vostok.Logging.Microsoft;

namespace Vostok.Applications.AspNetCore.HostBuilders
{
    internal class WebApplicationHostFactory
    {
        private readonly IVostokHostingEnvironment environment;
        private readonly IVostokApplication application;

        private readonly Customization<WebApplicationBuilder> builderCustomization = new Customization<WebApplicationBuilder>();
        private readonly Customization<WebApplication> applicationCustomization = new Customization<WebApplication>();
        private readonly Customization<VostokLoggerProviderSettings> loggerCustomization = new Customization<VostokLoggerProviderSettings>();

        public WebApplicationHostFactory(IVostokHostingEnvironment environment, IVostokApplication application)
        {
            this.environment = environment;
            this.application = application;
        }

        public WebApplication CreateHost()
        {
            var webApplication = CreateHostBuilder().Build();

            applicationCustomization.Customize(webApplication);
            
            return webApplication;
        }

        private WebApplicationBuilder CreateHostBuilder()
        {
            var builder = WebApplication.CreateBuilder();

            builder.Host.ConfigureHostConfiguration(config => config.AddDefaultLoggingFilters());

            builder.Logging.AddVostokLogging(environment, GetLoggerSettings());

            builder.Configuration.AddVostokSources(environment);
            
            builder.Services.AddSingleton<IHostLifetime, GenericHostEmptyLifetime>();
            builder.Services.AddVostokEnvironment(environment, application);
            builder.Services.Configure<HostOptions>(options => options.ShutdownTimeout = environment.ShutdownTimeout.Cut(100.Milliseconds(), 0.05));

            // note (kungurtsev, 04.01.2022): seems impossible to create wrapper and reject calling Build 
            builderCustomization.Customize(builder);

            return builder;
        }

        public void SetupBuilder(Action<WebApplicationBuilder> setup)
            => builderCustomization.AddCustomization(setup ?? throw new ArgumentNullException(nameof(setup)));
        
        public void SetupApplication(Action<WebApplication> setup)
            => applicationCustomization.AddCustomization(setup ?? throw new ArgumentNullException(nameof(setup)));
        
        public void SetupHost(Action<IHostBuilder> setup)
            => builderCustomization.AddCustomization(b => 
                (setup ?? throw new ArgumentNullException(nameof(setup)))
                    .Invoke(new GenericHostBuilderWrapper(b.Host)));

        public void SetupLogger(Action<VostokLoggerProviderSettings> setup)
            => loggerCustomization.AddCustomization(setup ?? throw new ArgumentNullException(nameof(setup)));

        private VostokLoggerProviderSettings GetLoggerSettings()
            => loggerCustomization.Customize(new VostokLoggerProviderSettings());
    }
}
#endif