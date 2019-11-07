using System;
using Vostok.Commons.Helpers;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.AspNetCore.Middlewares;
using Vostok.Hosting.AspNetCore.Setup;
// ReSharper disable ParameterHidesMember

namespace Vostok.Hosting.AspNetCore.Builders
{
    internal class LoggingMiddlewareBuilder : IVostokLoggingMiddlewareBuilder
    {
        private readonly Customization<LoggingMiddlewareSettings> settingsCustomization;

        public LoggingMiddlewareBuilder()
        {
            settingsCustomization = new Customization<LoggingMiddlewareSettings>();
        }

        public IVostokLoggingMiddlewareBuilder CustomizeSettings(Action<LoggingMiddlewareSettings> settingsCustomization)
        {
            this.settingsCustomization.AddCustomization(settingsCustomization ?? throw new ArgumentNullException(nameof(settingsCustomization)));
            return this;
        }

        public LoggingMiddleware Build(IVostokHostingEnvironment environment)
        {
            var settings = new LoggingMiddlewareSettings();

            settingsCustomization.Customize(settings);

            return new LoggingMiddleware(environment.Log, settings);
        }
    }
}