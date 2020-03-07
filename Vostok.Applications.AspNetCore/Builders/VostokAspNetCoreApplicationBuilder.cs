using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Vostok.Applications.AspNetCore.Configuration;
using Vostok.Applications.AspNetCore.Helpers;
using Vostok.Applications.AspNetCore.Models;
using Vostok.Applications.AspNetCore.StartupFilters;
using Vostok.Commons.Helpers;
using Vostok.Commons.Threading;
using Vostok.Commons.Time;
using Vostok.Context;
using Vostok.Hosting.Abstractions;
using Vostok.Logging.Microsoft;
using Vostok.ServiceDiscovery.Abstractions;

namespace Vostok.Applications.AspNetCore.Builders
{
    internal class VostokAspNetCoreApplicationBuilder<TStartup> : IVostokAspNetCoreApplicationBuilder
        where TStartup : class
    {
        private readonly IVostokHostingEnvironment environment;
        private readonly AtomicBoolean webHostEnabled;
        private readonly VostokNetCoreApplicationBuilder innerBuilder;
        private readonly VostokMiddlewaresBuilder middlewaresBuilder;
        private readonly Customization<IWebHostBuilder> webHostBuilderCustomization;
        private readonly Customization<KestrelSettings> kestrelCustomization;

        private readonly VostokThrottlingBuilder throttlingBuilder;

        public VostokAspNetCoreApplicationBuilder(IVostokHostingEnvironment environment, List<IDisposable> disposables, AtomicBoolean initialized)
        {
            this.environment = environment;

            webHostEnabled = true;

            innerBuilder = new VostokNetCoreApplicationBuilder(environment);
            innerBuilder.SetupMicrosoftLog(
                s => s.IgnoredScopes = new HashSet<string>
                {
                    MicrosoftConstants.ActionLogScope,
                    MicrosoftConstants.HostingLogScope,
                    MicrosoftConstants.ConnectionLogScope
                });

            throttlingBuilder = new VostokThrottlingBuilder(environment, disposables);
            middlewaresBuilder = new VostokMiddlewaresBuilder(throttlingBuilder, initialized);

            webHostBuilderCustomization = new Customization<IWebHostBuilder>();
            kestrelCustomization = new Customization<KestrelSettings>();
        }

        public IHost Build()
        {
            using (FlowingContext.Globals.Use(environment))
            {
                var hostBuilder = innerBuilder.CreateHostBuilder();

                if (webHostEnabled)
                {
                    hostBuilder.ConfigureServices(middlewaresBuilder.Register);

                    hostBuilder.ConfigureWebHostDefaults(
                        webHostBuilder =>
                        {
                            ConfigureUrl(webHostBuilder, environment);

                            var urlsBefore = webHostBuilder.GetSetting(WebHostDefaults.ServerUrlsKey);

                            webHostBuilder.UseKestrel(ConfigureKestrel);
                            webHostBuilder.UseSockets(ConfigureSocketTransport);
                            webHostBuilder.UseShutdownTimeout(environment.ShutdownTimeout.Cut(100.Milliseconds(), 0.05));

                            if (typeof(TStartup) != typeof(EmptyStartup))
                                webHostBuilder.UseStartup<TStartup>();

                            webHostBuilderCustomization.Customize(webHostBuilder);

                            var urlsAfter = webHostBuilder.GetSetting(WebHostDefaults.ServerUrlsKey);
                            EnsureUrlsNotChanged(urlsBefore, urlsAfter);
                        });
                }

                return hostBuilder.Build();
            }
        }

        private static void ConfigureUrl(IWebHostBuilder builder, IVostokHostingEnvironment environment)
        {
            if (!environment.ServiceBeacon.ReplicaInfo.TryGetUrl(out var url))
                throw new Exception("Port or url should be configured in ServiceBeacon using VostokHostingEnvironmentSetup.");

            builder = builder.UseUrls($"{url.Scheme}://*:{url.Port}/");

            builder.ConfigureServices(services => services.AddTransient<IStartupFilter>(_ => new UrlPathStartupFilter(environment)));
        }

        private static void ConfigureSocketTransport(SocketTransportOptions options)
            => options.NoDelay = true;

        private void EnsureUrlsNotChanged(string urlsBefore, string urlsAfter)
        {
            if (urlsAfter.Contains(urlsBefore))
                return;

            throw new Exception(
                "Application url should be configured in ServiceBeacon instead of WebHostBuilder.\n" +
                $"ServiceBeacon url: '{urlsBefore}'. WebHostBuilder urls: '{urlsAfter}'.\n" +
                "To configure application port (without url) use VostokHostingEnvironmentSetup extension: `vostokHostingEnvironmentSetup.SetPort(...)`.\n" +
                "To configure application url use VostokHostingEnvironmentSetup: `vostokHostingEnvironmentSetup.SetupServiceBeacon(serviceBeaconBuilder => serviceBeaconBuilder.SetupReplicaInfo(replicaInfo => replicaInfo.SetUrl(...)))`.");
        }

        private void ConfigureKestrel(WebHostBuilderContext builderContext, KestrelServerOptions options)
        {
            var settings = kestrelCustomization.Customize(new KestrelSettings());

            options.AddServerHeader = false;
            options.AllowSynchronousIO = false;

            options.Limits.MaxConcurrentConnections = null;
            options.Limits.MaxRequestBufferSize = 256 * 1024;
            options.Limits.MaxResponseBufferSize = 256 * 1024;
            options.Limits.Http2.MaxStreamsPerConnection = 1000;

            options.Limits.MaxRequestBodySize = settings.MaxRequestBodySize;
            options.Limits.MaxRequestLineSize = settings.MaxRequestLineSize;
            options.Limits.MaxRequestHeadersTotalSize = settings.MaxRequestHeadersSize;
            options.Limits.MaxConcurrentUpgradedConnections = settings.MaxConcurrentWebSocketConnections;

            if (settings.KeepAliveTimeout.HasValue)
                options.Limits.KeepAliveTimeout = settings.KeepAliveTimeout.Value;

            if (settings.RequestHeadersTimeout.HasValue)
                options.Limits.RequestHeadersTimeout = settings.RequestHeadersTimeout.Value;
        }

        #region SetupComponents

        public IVostokAspNetCoreApplicationBuilder SetupGenericHost(Action<IHostBuilder> setup)
        {
            innerBuilder.SetupGenericHost(setup);
            return this;
        }

        public IVostokAspNetCoreApplicationBuilder DisableWebHost()
        {
            webHostEnabled.Value = false;
            return this;
        }

        public IVostokAspNetCoreApplicationBuilder SetupWebHost(Action<IWebHostBuilder> setup)
        {
            webHostBuilderCustomization.AddCustomization(setup ?? throw new ArgumentNullException(nameof(setup)));
            return this;
        }

        public IVostokAspNetCoreApplicationBuilder SetupKestrel(Action<KestrelSettings> setup)
        {
            kestrelCustomization.AddCustomization(setup ?? throw new ArgumentNullException(nameof(setup)));
            return this;
        }

        public IVostokAspNetCoreApplicationBuilder SetupDatacenterAwareness(Action<DatacenterAwarenessSettings> setup)
        {
            middlewaresBuilder.Customize(setup);
            return this;
        }

        public IVostokAspNetCoreApplicationBuilder SetupRequestInfoFilling(Action<FillRequestInfoSettings> setup)
        {
            middlewaresBuilder.Customize(setup); 
            return this;
        }

        public IVostokAspNetCoreApplicationBuilder SetupDistributedContext(Action<DistributedContextSettings> setup)
        {
            middlewaresBuilder.Customize(setup); 
            return this;
        }

        public IVostokAspNetCoreApplicationBuilder SetupTracing(Action<TracingSettings> setup)
        {
            middlewaresBuilder.Customize(setup);
            return this;
        }

        public IVostokAspNetCoreApplicationBuilder SetupLogging(Action<LoggingSettings> setup)
        {
            middlewaresBuilder.Customize(setup);
            return this;
        }

        public IVostokAspNetCoreApplicationBuilder SetupPingApi(Action<PingApiSettings> setup)
        {
            middlewaresBuilder.Customize(setup);
            return this;
        }

        public IVostokAspNetCoreApplicationBuilder SetupThrottling(Action<IVostokThrottlingBuilder> setup)
        {
            setup(throttlingBuilder);
            return this;
        }

        public IVostokAspNetCoreApplicationBuilder SetupMicrosoftLog(Action<VostokLoggerProviderSettings> setup)
        {
            innerBuilder.SetupMicrosoftLog(setup);
            return this;
        }

        #endregion
    }
}