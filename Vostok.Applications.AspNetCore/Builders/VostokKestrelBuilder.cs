using System;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Vostok.Applications.AspNetCore.Configuration;
using Vostok.Commons.Helpers;

namespace Vostok.Applications.AspNetCore.Builders
{
    internal class VostokKestrelBuilder
    {
        public const int MaxResponseBufferSize = 64 * 1024;

        private const int MaxRequestBufferSize = 128 * 1024;

        private readonly Customization<KestrelSettings> kestrelCustomization = new Customization<KestrelSettings>();

        public void Customize(Action<KestrelSettings> customization)
            => kestrelCustomization.AddCustomization(customization);

        public void ConfigureKestrel(KestrelServerOptions options)
        {
            var settings = kestrelCustomization.Customize(new KestrelSettings());

            options.AddServerHeader = false;

            options.Limits.MaxRequestBufferSize = MaxRequestBufferSize;
            options.Limits.MaxResponseBufferSize = MaxResponseBufferSize;

            options.Limits.MaxRequestBodySize = settings.MaxRequestBodySize;
            options.Limits.MaxRequestLineSize = settings.MaxRequestLineSize;
            options.Limits.MaxRequestHeadersTotalSize = settings.MaxRequestHeadersSize;
            options.Limits.MaxConcurrentUpgradedConnections = settings.MaxConcurrentWebSocketConnections;

            if (settings.KeepAliveTimeout.HasValue)
                options.Limits.KeepAliveTimeout = settings.KeepAliveTimeout.Value;

            if (settings.RequestHeadersTimeout.HasValue)
                options.Limits.RequestHeadersTimeout = settings.RequestHeadersTimeout.Value;
        }
    }
}