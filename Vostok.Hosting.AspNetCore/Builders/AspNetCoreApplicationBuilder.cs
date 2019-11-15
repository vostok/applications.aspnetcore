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
        private readonly TracingMiddlewareBuilder tracingMiddlewareBuilder;
        private readonly FillRequestInfoMiddlewareBuilder fillRequestInfoMiddlewareBuilder;
        private readonly DenyRequestsMiddlewareBuilder denyRequestsMiddlewareBuilder;
        private readonly PingApiMiddlewareBuilder pingApiMiddlewareBuilder;
        private readonly MicrosoftLogBuilder microsoftLogBuilder;
        private readonly MicrosoftConfigurationBuilder microsoftConfigurationBuilder;
        private readonly Customization<IWebHostBuilder> webHostBuilderCustomization;

        public AspNetCoreApplicationBuilder()
        {
            loggingMiddlewareBuilder = new LoggingMiddlewareBuilder();
            tracingMiddlewareBuilder = new TracingMiddlewareBuilder();
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
                // TODO(kungurtsev): throttling middleware should go here.
                .AddMiddleware(new RestoreDistributedContextMiddleware())
                .AddMiddleware(tracingMiddlewareBuilder.Build(environment))
                .AddMiddleware(loggingMiddlewareBuilder.Build(environment))
                .AddMiddleware(denyRequestsMiddlewareBuilder.Build(environment))
                .AddMiddleware(pingApiMiddlewareBuilder.Build(environment));

            var urlsBefore = builder.GetSetting(WebHostDefaults.ServerUrlsKey);

            webHostBuilderCustomization.Customize(builder);

            var urlsAfter = builder.GetSetting(WebHostDefaults.ServerUrlsKey);
            EnsureNotChanged(urlsBefore, urlsAfter);

            return builder.Build();
        }

        private void EnsureNotChanged(string urlsBefore, string urlsAfter)
        {
            if (urlsAfter.Contains(urlsBefore))
                return;

            throw new Exception("Application url should be configured via ServiceBeacon instead of WebHostBuilder.\n" +
                                $"ServiceBeacon url: {urlsBefore}. WebHostBuilder urls: {urlsAfter}.");
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

        public IVostokAspNetCoreApplicationBuilder SetupTracingMiddleware(Action<IVostokTracingMiddlewareBuilder> setup)
        {
            setup = setup ?? throw new ArgumentNullException(nameof(setup));
            setup(tracingMiddlewareBuilder);
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