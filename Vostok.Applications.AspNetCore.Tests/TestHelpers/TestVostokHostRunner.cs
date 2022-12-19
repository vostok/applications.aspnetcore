using System.Threading.Tasks;
using Vostok.Hosting;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Setup;

namespace Vostok.Applications.AspNetCore.Tests.TestHelpers
{
    public class TestVostokHostRunner : ITestHostRunner
    {
        private readonly VostokHost host;

        public TestVostokHostRunner(IVostokApplication application, VostokHostingEnvironmentSetup setup)
        {
            var hostSettings = new VostokHostSettings(application, setup);
            host = new VostokHost(hostSettings);
        }

        public Task StartAsync() =>
            host.StartAsync();

        public Task StopAsync() => 
            host.StopAsync();
    }
}