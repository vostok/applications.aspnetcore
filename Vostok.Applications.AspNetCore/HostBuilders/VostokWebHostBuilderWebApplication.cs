#if NET6_0
using System;
using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Vostok.Commons.Time;
using Vostok.ServiceDiscovery.Abstractions;

// ReSharper disable UnusedTypeParameter

namespace Vostok.Applications.AspNetCore.HostBuilders
{
    internal partial class VostokWebHostBuilder
    {
        public void ConfigureWebHost(WebApplicationBuilder builder)
        {
            if (webHostEnabled)
            {
                RegisterBasePath(builder.Services);

                CreateHealthCheckBuilder().Register(builder.Services);

                middlewaresBuilder.Register(builder.Services);

                ConfigureWebHostInternal(builder);
            }
        }

        private void ConfigureWebHostInternal(WebApplicationBuilder builder)
        {
            if (!environment.ServiceBeacon.ReplicaInfo.TryGetUrl(out var url))
                throw new Exception("Port or url should be configured in ServiceBeacon using VostokHostingEnvironmentSetup.");

            builder.WebHost.UseKestrel(options =>
            {
                kestrelBuilder.ConfigureKestrel(options);
                options.Listen(IPAddress.Any, url.Port);
            });
            builder.WebHost.UseSockets();
            builder.WebHost.UseShutdownTimeout(environment.ShutdownTimeout.Cut(100.Milliseconds(), 0.05));
        }
    }
}
#endif