using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Vostok.Commons.Helpers;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.AspNetCore.Helpers;
using Vostok.Hosting.AspNetCore.Middlewares;
using Vostok.Hosting.AspNetCore.Setup;

namespace Vostok.Hosting.AspNetCore.Builders
{
    internal class AspNetCoreApplicationBuilder : IVostokAspNetCoreApplicationBuilder
    {
        private readonly LoggingMiddlewareBuilder loggingMiddlewareBuilder;
        private readonly FillRequestInfoMiddlewareBuilder fillRequestInfoMiddlewareBuilder;
        private readonly DenyRequestsMiddlewareBuilder denyRequestsMiddlewareBuilder;
        private readonly PingApiMiddlewareBuilder pingApiMiddlewareBuilder;
        private readonly MicrosoftLogBuilder microsoftLogBuilder;
        private readonly MicrosoftConfigurationBuilder microsoftConfigurationBuilder;
        private readonly Customization<IWebHostBuilder> webHostBuilderCustomization;

        public AspNetCoreApplicationBuilder()
        {
            loggingMiddlewareBuilder = new LoggingMiddlewareBuilder();
            fillRequestInfoMiddlewareBuilder = new FillRequestInfoMiddlewareBuilder();
            denyRequestsMiddlewareBuilder = new DenyRequestsMiddlewareBuilder();
            pingApiMiddlewareBuilder = new PingApiMiddlewareBuilder();
            microsoftLogBuilder = new MicrosoftLogBuilder();
            microsoftConfigurationBuilder = new MicrosoftConfigurationBuilder();
            webHostBuilderCustomization = new Customization<IWebHostBuilder>();
        }

        public IWebHost Build(IVostokHostingEnvironment environment)
        {
            var builder = WebHost.CreateDefaultBuilder()
                .UseLog(microsoftLogBuilder.Build(environment))
                .AddConfigurationSource(microsoftConfigurationBuilder.Build(environment))
                .UseUrl(environment)
                .UseUrlPath(environment)
                .RegisterTypes(environment)
                .AddMiddleware(fillRequestInfoMiddlewareBuilder.Build(environment))
                .AddMiddleware(new RestoreDistributedContextMiddleware())
                .AddMiddleware(loggingMiddlewareBuilder.Build(environment))
                .AddMiddleware(denyRequestsMiddlewareBuilder.Build(environment))
                .AddMiddleware(pingApiMiddlewareBuilder.Build(environment));

            webHostBuilderCustomization.Customize(builder);

            return builder.Build();
        }

        #region SetupComponents

        public IVostokAspNetCoreApplicationBuilder SetupWebHost(Action<IWebHostBuilder> setup)
        {
            webHostBuilderCustomization.AddCustomization(setup ?? throw new ArgumentNullException(nameof(setup)));
            return this;
        }

        public IVostokAspNetCoreApplicationBuilder SetupLoggingMiddleware(Action<IVostokLoggingMiddlewareBuilder> setup)
        {
            setup = setup ?? throw new ArgumentNullException(nameof(setup));
            setup(loggingMiddlewareBuilder);
            return this;
        }

        public IVostokAspNetCoreApplicationBuilder SetupDenyRequestsMiddleware(Action<IVostokDenyRequestsMiddlewareBuilder> setup)
        {
            setup = setup ?? throw new ArgumentNullException(nameof(setup));
            setup(denyRequestsMiddlewareBuilder);
            return this;
        }

        public IVostokAspNetCoreApplicationBuilder SetupPingApiMiddleware(Action<IVostokPingApiMiddlewareBuilder> setup)
        {
            setup = setup ?? throw new ArgumentNullException(nameof(setup));
            pingApiMiddlewareBuilder.Enable();
            setup(pingApiMiddlewareBuilder);
            return this;
        }

        public IVostokAspNetCoreApplicationBuilder SetupMicrosoftLog(Action<IVostokMicrosoftLogBuilder> setup)
        {
            setup = setup ?? throw new ArgumentNullException(nameof(setup));
            setup(microsoftLogBuilder);
            return this;
        }

        #endregion
    }
}