using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json;
using Vostok.Clusterclient.Core;
using Vostok.Clusterclient.Core.Model;

namespace Vostok.Applications.AspNetCore.Tests.Extensions
{
    internal static class ClusterClientExtensions
    {
        public static async Task<ClusterResult> GetAsync(this IClusterClient client, string url)
        {
            var request = Request.Get(url);
            return await client.SendAsync(request);
        }

        public static async Task<T> GetAsync<T>(this IClusterClient client, string url)
        {
            var request = Request.Get(url);
            var result = await client.SendAsync(request);

            var json = result.Response.Content.ToString();
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static async Task<T> GetResponseOrDie<T>(this Task<ClusterResult> resultTask)
        {
            var result = await resultTask;
            result.Response.IsSuccessful.Should().BeTrue();

            return JsonConvert.DeserializeObject<T>(result.Response.Content.ToString());
        }
    }
}