using System;
using Vostok.Commons.Helpers;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.AspNetCore.Middlewares;
using Vostok.Hosting.AspNetCore.Setup;
// ReSharper disable ParameterHidesMember

namespace Vostok.Hosting.AspNetCore.Builders
{
    internal class TracingMiddlewareBuilder : IVostokTracingMiddlewareBuilder
    {
        private readonly Customization<TracingMiddlewareSettings> settingsCustomization;

        public TracingMiddlewareBuilder()
        {
            settingsCustomization = new Customization<TracingMiddlewareSettings>();
        }

        public TracingMiddleware Build(IVostokHostingEnvironment environment)
        {
            var settings = new TracingMiddlewareSettings(environment.Tracer);

            settingsCustomization.Customize(settings);

            return new TracingMiddleware(settings);
        }

        public IVostokTracingMiddlewareBuilder CustomizeSettings(Action<TracingMiddlewareSettings> settingsCustomization)
        {
            this.settingsCustomization.AddCustomization(settingsCustomization ?? throw new ArgumentNullException(nameof(settingsCustomization)));
            return this;
        }
    }
}