using System;
using Vostok.Commons.Helpers;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.AspNetCore.Middlewares;
using Vostok.Hosting.AspNetCore.Setup;

namespace Vostok.Hosting.AspNetCore.Builders
{
    internal class FillRequestInfoMiddlewareBuilder : IVostokFillRequestInfoMiddlewareBuilder
    {
        private readonly Customization<FillRequestInfoMiddlewareSettings> settingsCustomization;

        public FillRequestInfoMiddlewareBuilder()
        {
            settingsCustomization = new Customization<FillRequestInfoMiddlewareSettings>();
        }

        public IVostokFillRequestInfoMiddlewareBuilder CustomizeSettings(Action<FillRequestInfoMiddlewareSettings> settingsCustomization)
        {
            this.settingsCustomization.AddCustomization(settingsCustomization ?? throw new ArgumentNullException(nameof(settingsCustomization)));
            return this;
        }

        public FillRequestInfoMiddleware Build(IVostokHostingEnvironment environment)
        {
            var settings = new FillRequestInfoMiddlewareSettings();

            settingsCustomization.Customize(settings);

            return new FillRequestInfoMiddleware(settings);
        }
    }
}