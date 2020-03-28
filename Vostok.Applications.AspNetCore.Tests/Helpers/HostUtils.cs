using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Vostok.Applications.AspNetCore.Tests.Extensions;
using Vostok.Applications.AspNetCore.Tests.Models;
using Vostok.Clusterclient.Core;

namespace Vostok.Applications.AspNetCore.Tests.Helpers
{
    internal static class HostUtils
    {
        public static int GetFreePort()
        {
            var tcpListener = new TcpListener(IPAddress.Loopback, 0);
            tcpListener.Start();
            var port = ((IPEndPoint)tcpListener.LocalEndpoint).Port;
            tcpListener.Stop();
            return port;
        }

        public static async Task WaitUntilInitialized(IClusterClient clusterClient, TimeSpan? timeout = null)
        {
            var deadline = DateTime.UtcNow.Add(timeout ?? TimeSpan.FromSeconds(1));

            while (DateTime.UtcNow < deadline)
            {
                try
                {
                    var result = await clusterClient.GetAsync<PingApiResponse>("/_status/ping");
                    if (result.Status == "Ok")
                        return;
                }
                catch
                {
                    Thread.Sleep(50);
                }
            }

            throw new TimeoutException("Host didn't start");
        }
    }
}