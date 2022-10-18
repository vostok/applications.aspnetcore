using System.Threading.Tasks;
using Vostok.Hosting;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Setup;

namespace Vostok.Applications.AspNetCore.Tests
{
    public class TestVostokApplicationRunner : IApplicationRunner
    {
        private readonly IVostokApplication application;
        private readonly VostokHostingEnvironmentSetup setup;
        private VostokHost testHost;

        public TestVostokApplicationRunner(
            IVostokApplication application,
            VostokHostingEnvironmentSetup setup
        )
        {
            this.application = application;
            this.setup = setup;
        }

        public async Task RunAsync()
        {
            testHost = await StartHost();
        }

        public Task StopAsync()
            => testHost?.StopAsync();

        private async Task<VostokHost> StartHost()
        {
            var hostSettings = new VostokHostSettings(application, setup);
            var host = new VostokHost(hostSettings);

            await host.StartAsync();

            return host;
        }
    }
}