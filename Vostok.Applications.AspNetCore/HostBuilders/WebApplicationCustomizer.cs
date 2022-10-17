#if NET6_0_OR_GREATER
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
    internal class WebApplicationCustomizer
    {
        private readonly IVostokHostingEnvironment environment;

        private readonly Customization<WebApplicationOptions> webApplicationOptionsCustomization = new Customization<WebApplicationOptions>();
        private readonly Customization<WebApplicationBuilder> webApplicationBuilderCustomization = new Customization<WebApplicationBuilder>();
        private readonly Customization<WebApplication> webApplicationCustomization = new Customization<WebApplication>();
        private readonly Customization<VostokLoggerProviderSettings> loggerCustomization = new Customization<VostokLoggerProviderSettings>();

        public WebApplicationCustomizer(IVostokHostingEnvironment environment)
        {
            this.environment = environment;
        }

        public void SetupWebApplicationOptions(Func<WebApplicationOptions, WebApplicationOptions> customization)
            => webApplicationOptionsCustomization.AddCustomization(customization ?? throw new ArgumentNullException(nameof(customization)));

        public void SetupWebApplicationBuilder(Action<WebApplicationBuilder> setup)
            => webApplicationBuilderCustomization.AddCustomization(setup ?? throw new ArgumentNullException(nameof(setup)));

        public void SetupWebApplication(Action<WebApplication> setup)
            => webApplicationCustomization.AddCustomization(setup ?? throw new ArgumentNullException(nameof(setup)));

        public void SetupLogger(Action<VostokLoggerProviderSettings> setup)
            => loggerCustomization.AddCustomization(setup ?? throw new ArgumentNullException(nameof(setup)));
        
        public WebApplicationOptions CustomizeWebApplicationOptions(WebApplicationOptions options)
            => webApplicationOptionsCustomization.Customize(options);

        public void CustomizeWebApplication(WebApplication webApplication)
            => webApplicationCustomization.Customize(webApplication);

        public void CustomizeWebApplicationBuilder(WebApplicationBuilder builder)
        {
            builder.Services.Configure<WebApplicationOptions>(opts => CustomizeWebApplicationOptions(opts));

            builder.Configuration.AddDefaultLoggingFilters();

            builder.Logging.AddVostokLogging(environment, GetLoggerSettings());

            builder.Configuration.AddVostokSources(environment);

            builder.Services
                .Configure<HostOptions>(options => options.ShutdownTimeout = environment.ShutdownTimeout.Cut(100.Milliseconds(), 0.05));

            webApplicationBuilderCustomization.Customize(builder);
        }

        private VostokLoggerProviderSettings GetLoggerSettings()
            => loggerCustomization.Customize(new VostokLoggerProviderSettings());
    }
}
#endif