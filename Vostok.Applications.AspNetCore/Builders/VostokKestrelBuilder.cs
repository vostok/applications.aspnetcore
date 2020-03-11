using System;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets;
using Vostok.Applications.AspNetCore.Configuration;
using Vostok.Commons.Helpers;

namespace Vostok.Applications.AspNetCore.Builders
{
    internal class VostokKestrelBuilder
    {
        private readonly Customization<KestrelSettings> kestrelCustomization = new Customization<KestrelSettings>();

        public void Customize(Action<KestrelSettings> customization)
            => kestrelCustomization.AddCustomization(customization);

        public void ConfigureKestrel(KestrelServerOptions options)
        {
            var settings = kestrelCustomization.Customize(new KestrelSettings());

            options.AddServerHeader = false;
            options.AllowSynchronousIO = false;

            options.Limits.MaxConcurrentConnections = null;
            options.Limits.MaxRequestBufferSize = 256 * 1024;
            options.Limits.MaxResponseBufferSize = 256 * 1024;

            #if NETCOREAPP3_1
            options.Limits.Http2.MaxStreamsPerConnection = 1000;
            #endif

            options.Limits.MaxRequestBodySize = settings.MaxRequestBodySize;
            options.Limits.MaxRequestLineSize = settings.MaxRequestLineSize;
            options.Limits.MaxRequestHeadersTotalSize = settings.MaxRequestHeadersSize;
            options.Limits.MaxConcurrentUpgradedConnections = settings.MaxConcurrentWebSocketConnections;

            if (settings.KeepAliveTimeout.HasValue)
                options.Limits.KeepAliveTimeout = settings.KeepAliveTimeout.Value;

            if (settings.RequestHeadersTimeout.HasValue)
                options.Limits.RequestHeadersTimeout = settings.RequestHeadersTimeout.Value;
        }

        public void ConfigureSocketTransport(SocketTransportOptions options)
        {
            #if NETCOREAPP3_1
            options.NoDelay = true;
            #endif
        } 
    }
}
