using System;
using Vostok.Commons.Helpers;
using Vostok.Datacenters;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.AspNetCore.Middlewares;
using Vostok.Hosting.AspNetCore.Setup;

// ReSharper disable ParameterHidesMember

namespace Vostok.Hosting.AspNetCore.Builders
{
    internal class DenyRequestsMiddlewareBuilder
    {
        private int? denyResponseCode;
        
        public void AllowRequestsIfNotInActiveDatacenter()
        {
            denyResponseCode = null;
        }

        public void DenyRequestsIfNotInActiveDatacenter(int denyResponseCode)
        {
            this.denyResponseCode = denyResponseCode;
        }

        public DenyRequestsMiddleware Build(IVostokHostingEnvironment environment)
        {
            if (denyResponseCode == null)
                return null;

            var settings = new DenyRequestsMiddlewareSettings(
                () => !environment.Datacenters.LocalDatacenterIsActive(),
                denyResponseCode.Value);
            
            return new DenyRequestsMiddleware(settings, environment.Log);
        }
    }
}