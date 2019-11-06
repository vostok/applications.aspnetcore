using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.AspNetCore.Helpers;

namespace Vostok.Hosting.AspNetCore.StartupFilters
{
    internal class UrlPathStartupFilter : IStartupFilter
    {
        private const string Slash = "/";
        private readonly string urlPath;

        public UrlPathStartupFilter(IVostokHostingEnvironment environment)
        {
            urlPath = environment.ServiceBeacon.ReplicaInfo.GetUrl()?.AbsolutePath;
        }

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            if (urlPath == Slash)
                return next;

            return app =>
            {
                app.UsePathBase(urlPath);
                next(app);
            };
        }
    }
}