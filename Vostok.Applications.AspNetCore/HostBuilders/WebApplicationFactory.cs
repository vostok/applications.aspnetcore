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
    internal class WebApplicationFactory
    {
        private readonly IVostokHostingEnvironment environment;
        private readonly IVostokApplication application;

        //private readonly Customization<IHostBuilder> hostCustomization = new Customization<IHostBuilder>();
        private readonly Customization<WebApplicationBuilder> webApplicationBuilderCustomization = new Customization<WebApplicationBuilder>();
        private readonly Customization<WebApplication> webApplicationCustomization = new Customization<WebApplication>();
        private readonly Customization<VostokLoggerProviderSettings> loggerCustomization = new Customization<VostokLoggerProviderSettings>();

        public WebApplicationFactory(IVostokHostingEnvironment environment, IVostokApplication application)
        {
            this.environment = environment;
            this.application = application;
        }

        public WebApplication Create()
        {
            var builder = CreateBuilder();

            var webApplication = builder.Build();

            webApplicationCustomization.Customize(webApplication);

            return webApplication;
        }

        private WebApplicationBuilder CreateBuilder()
        {
            var builder = WebApplication.CreateBuilder();

            builder.Configuration.AddDefaultLoggingFilters();

            builder.Logging.AddVostokLogging(environment, GetLoggerSettings());

            builder.Configuration.AddVostokSources(environment);

            builder.Services
                .AddSingleton<IHostLifetime, GenericHostEmptyLifetime>()
                .AddVostokEnvironment(environment, application)
                .Configure<HostOptions>(options => options.ShutdownTimeout = environment.ShutdownTimeout.Cut(100.Milliseconds(), 0.05));

            webApplicationBuilderCustomization.Customize(builder);

            return builder;
        }

        public void SetupWebApplicationBuilder(Action<WebApplicationBuilder> setup)
            => webApplicationBuilderCustomization.AddCustomization(setup ?? throw new ArgumentNullException(nameof(setup)));
        
        public void SetupWebApplication(Action<WebApplication> setup)
            => webApplicationCustomization.AddCustomization(setup ?? throw new ArgumentNullException(nameof(setup)));

        public void SetupLogger(Action<VostokLoggerProviderSettings> setup)
            => loggerCustomization.AddCustomization(setup ?? throw new ArgumentNullException(nameof(setup)));

        private VostokLoggerProviderSettings GetLoggerSettings()
            => loggerCustomization.Customize(new VostokLoggerProviderSettings());
    }
}
#endif