#if NETCOREAPP
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Vostok.Applications.AspNetCore.HostBuilders
{
    internal class GenericHostEmptyLifetime : IHostLifetime
    {
        public Task StopAsync(CancellationToken cancellationToken) =>
            Task.CompletedTask;

        public Task WaitForStartAsync(CancellationToken cancellationToken) =>
            Task.CompletedTask;
    }
}
#endif