using System.Threading.Tasks;
using Vostok.Clusterclient.Core;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Topology;
using Vostok.Clusterclient.Tracing;
using Vostok.Clusterclient.Transport;
using Vostok.Commons.Time;
using Vostok.Hosting.Abstractions;
using Vostok.Logging.Abstractions;
using Vostok.ServiceDiscovery.Abstractions;

namespace Vostok.Applications.AspNetCore.Helpers
{
    internal static class MiddlewaresWarmup
    {
        public static async Task WarmupPingApi(IVostokHostingEnvironment environment)
        {
            if (environment.ServiceBeacon.ReplicaInfo.TryGetUrl(out var url))
            {
                var client = new ClusterClient(
                    environment.Log,
                    configuration =>
                    {
                        configuration.ClusterProvider = new FixedClusterProvider(url);
                        configuration.SetupUniversalTransport();
                        configuration.SetupDistributedTracing(environment.Tracer);
                    });

                await client.SendAsync(Request.Get("_status/ping"), 20.Seconds());
            }
            else
                environment.Log.Warn("Unable to warmup middlewares. Couldn't obtain replica url.");
        }
    }
}