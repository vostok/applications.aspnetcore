using System;
using Vostok.Commons.Helpers;
using Vostok.Datacenters;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.AspNetCore.Middlewares;
using Vostok.Hosting.AspNetCore.Setup;

// ReSharper disable ParameterHidesMember

namespace Vostok.Hosting.AspNetCore.Builders
{
    internal class DenyRequestsMiddlewareBuilder : IVostokDenyRequestsMiddlewareBuilder
    {
        private readonly Customization<DenyRequestsMiddlewareSettings> settingsCustomization;
        private volatile bool denyRequestsIfNotInActiveDatacenter;

        public DenyRequestsMiddlewareBuilder()
        {
            settingsCustomization = new Customization<DenyRequestsMiddlewareSettings>();
        }

        public IVostokDenyRequestsMiddlewareBuilder CustomizeSettings(Action<DenyRequestsMiddlewareSettings> settingsCustomization)
        {
            this.settingsCustomization.AddCustomization(settingsCustomization ?? throw new ArgumentNullException(nameof(settingsCustomization)));
            return this;
        }

        public IVostokDenyRequestsMiddlewareBuilder AllowRequestsIfNotInActiveDatacenter()
        {
            denyRequestsIfNotInActiveDatacenter = false;
            return this;
        }

        public IVostokDenyRequestsMiddlewareBuilder DenyRequestsIfNotInActiveDatacenter()
        {
            denyRequestsIfNotInActiveDatacenter = true;
            return this;
        }

        public DenyRequestsMiddleware Build(IVostokHostingEnvironment environment)
        {
            if (!denyRequestsIfNotInActiveDatacenter)
                return null;

            var settings = new DenyRequestsMiddlewareSettings(
                () => !environment.Datacenters.LocalDatacenterIsActive());

            settingsCustomization.Customize(settings);

            return new DenyRequestsMiddleware(settings, environment.Log);
        }
    }
}