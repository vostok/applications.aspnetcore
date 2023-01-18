using Vostok.Clusterclient.Core;
using Vostok.Clusterclient.Core.Topology;
using Vostok.Clusterclient.Transport;
using Vostok.Logging.Abstractions;

namespace Vostok.Applications.AspNetCore.Tests.TestHelpers;

internal class ClusterClientHelper
{
    public static IClusterClient Create(int port, ILog log)
    {
        // ReSharper disable once RedundantNameQualifier
        // full type name currently required due to https://github.com/vostok/clusterclient.datacenters/issues/1
        return new Clusterclient.Core.ClusterClient(
            log.ForContext("TestClusterClient"),
            s =>
            {
                s.ClientApplicationName = "TestClusterClient";
                s.ClusterProvider = new FixedClusterProvider($"http://localhost:{port}");
                s.SetupUniversalTransport();
            });
    }
}