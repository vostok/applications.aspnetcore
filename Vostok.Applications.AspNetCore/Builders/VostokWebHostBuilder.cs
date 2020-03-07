using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Vostok.Applications.AspNetCore.Models;
using Vostok.Applications.AspNetCore.StartupFilters;
using Vostok.Commons.Helpers;
using Vostok.Commons.Threading;
using Vostok.Commons.Time;
using Vostok.Hosting.Abstractions;
using Vostok.ServiceDiscovery.Abstractions;

namespace Vostok.Applications.AspNetCore.Builders
{
    internal class VostokWebHostBuilder<TStartup>
        where TStartup : class
    {
        private readonly IVostokHostingEnvironment environment;
        private readonly VostokKestrelBuilder kestrelBuilder;
        private readonly VostokMiddlewaresBuilder middlewaresBuilder;

        private readonly AtomicBoolean webHostEnabled = true;
        private readonly Customization<IWebHostBuilder> webHostCustomization = new Customization<IWebHostBuilder>();

        public VostokWebHostBuilder(
            IVostokHostingEnvironment environment, 
            VostokKestrelBuilder kestrelBuilder, 
            VostokMiddlewaresBuilder middlewaresBuilder)
        {
            this.environment = environment;
            this.kestrelBuilder = kestrelBuilder;
            this.middlewaresBuilder = middlewaresBuilder;
        }

        public void Disable()
            => webHostEnabled.Value = false;

        public void Customize(Action<IWebHostBuilder> customization)
            => webHostCustomization.AddCustomization(customization);

        public void ConfigureWebHost(IHostBuilder genericHostBuilder)
        {
            if (webHostEnabled)
            {
                genericHostBuilder.ConfigureServices(middlewaresBuilder.Register);

                genericHostBuilder.ConfigureWebHostDefaults(ConfigureWebHost);
            }
        }

        private void ConfigureWebHost(IWebHostBuilder webHostBuilder)
        {
            ConfigureUrl(webHostBuilder);

            var urlsBefore = webHostBuilder.GetSetting(WebHostDefaults.ServerUrlsKey);

            webHostBuilder.UseKestrel(kestrelBuilder.ConfigureKestrel);
            webHostBuilder.UseSockets(kestrelBuilder.ConfigureSocketTransport);
            webHostBuilder.UseShutdownTimeout(environment.ShutdownTimeout.Cut(100.Milliseconds(), 0.05));

            if (typeof(TStartup) != typeof(EmptyStartup))
                webHostBuilder.UseStartup<TStartup>();

            webHostCustomization.Customize(webHostBuilder);

            EnsureUrlsNotChanged(urlsBefore, webHostBuilder.GetSetting(WebHostDefaults.ServerUrlsKey));
        }

        private void ConfigureUrl(IWebHostBuilder webHostBuilder)
        {
            if (!environment.ServiceBeacon.ReplicaInfo.TryGetUrl(out var url))
                throw new Exception("Port or url should be configured in ServiceBeacon using VostokHostingEnvironmentSetup.");

            webHostBuilder = webHostBuilder.UseUrls($"{url.Scheme}://*:{url.Port}/");

            webHostBuilder.ConfigureServices(services => services.AddTransient<IStartupFilter>(_ => new UrlPathStartupFilter(environment)));
        }

        private static void EnsureUrlsNotChanged(string urlsBefore, string urlsAfter)
        {
            if (urlsAfter.Contains(urlsBefore))
                return;

            throw new Exception(
                "Application url should be configured in ServiceBeacon instead of WebHostBuilder.\n" +
                $"ServiceBeacon url: '{urlsBefore}'. WebHostBuilder urls: '{urlsAfter}'.\n" +
                "To configure application port (without url) use VostokHostingEnvironmentSetup extension: `vostokHostingEnvironmentSetup.SetPort(...)`.\n" +
                "To configure application url use VostokHostingEnvironmentSetup: `vostokHostingEnvironmentSetup.SetupServiceBeacon(serviceBeaconBuilder => serviceBeaconBuilder.SetupReplicaInfo(replicaInfo => replicaInfo.SetUrl(...)))`.");
        }
    }
}
