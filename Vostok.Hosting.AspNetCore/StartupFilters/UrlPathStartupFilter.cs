using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Vostok.Hosting.Abstractions;
using Vostok.ServiceDiscovery.Abstractions;

namespace Vostok.Hosting.AspNetCore.StartupFilters
{
    internal class UrlPathStartupFilter : IStartupFilter
    {
        private const string Slash = "/";
        private readonly string urlPath;

        public UrlPathStartupFilter(IVostokHostingEnvironment environment)
        {
            if (environment.ServiceBeacon.ReplicaInfo.TryGetUrl(out var url))
                urlPath = url.AbsolutePath;
        }

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            if (string.IsNullOrEmpty(urlPath) || urlPath == Slash)
                return next;

            return app =>
            {
                app.UsePathBase(urlPath);
                next(app);
            };
        }
    }
}