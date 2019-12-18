using System;
using Vostok.Commons.Helpers;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.AspNetCore.Middlewares;
using Vostok.Hosting.AspNetCore.Setup;

namespace Vostok.Hosting.AspNetCore.Builders
{
    internal class RestoreDistributedContextMiddlewareBuilder : IVostokRestoreDistributedContextMiddlewareBuilder
    {
        private readonly Customization<RestoreDistributedContextMiddlewareSettings> settingsCustomization;

        public RestoreDistributedContextMiddlewareBuilder()
        {
            settingsCustomization = new Customization<RestoreDistributedContextMiddlewareSettings>();
        }

        public IVostokRestoreDistributedContextMiddlewareBuilder CustomizeSettings(Action<RestoreDistributedContextMiddlewareSettings> settingsCustomization)
        {
            this.settingsCustomization.AddCustomization(settingsCustomization ?? throw new ArgumentNullException(nameof(settingsCustomization)));
            return this;
        }

        public RestoreDistributedContextMiddleware Build(IVostokHostingEnvironment environment)
        {
            var settings = new RestoreDistributedContextMiddlewareSettings();

            settingsCustomization.Customize(settings);

            return new RestoreDistributedContextMiddleware(settings);
        }
    }
}