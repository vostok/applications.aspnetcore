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
        private readonly MicrosoftLogBuilder microsoftLogBuilder;
        private readonly Customization<IWebHostBuilder> webHostBuilderCustomization;

        public AspNetCoreApplicationBuilder()
        {
            loggingMiddlewareBuilder = new LoggingMiddlewareBuilder();
            fillRequestInfoMiddlewareBuilder = new FillRequestInfoMiddlewareBuilder();
            microsoftLogBuilder = new MicrosoftLogBuilder();
            webHostBuilderCustomization = new Customization<IWebHostBuilder>();
        }

        public IWebHost Build(IVostokHostingEnvironment environment)
        {
            var builder = WebHost.CreateDefaultBuilder()
                .ConfigureLog(microsoftLogBuilder.Build(environment))
                .ConfigureUrl(environment)
                .ConfigureUrlPath(environment)
                .RegisterTypes(environment)
                .AddMiddleware(new RestoreDistributedContextMiddleware())
                .AddMiddleware(fillRequestInfoMiddlewareBuilder.Build(environment))
                .AddMiddleware(loggingMiddlewareBuilder.Build(environment));

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

        public IVostokAspNetCoreApplicationBuilder SetupMicrosoftLog(Action<IVostokMicrosoftLogBuilder> setup)
        {
            setup = setup ?? throw new ArgumentNullException(nameof(setup));
            setup(microsoftLogBuilder);
            return this;
        }

        #endregion
    }
}