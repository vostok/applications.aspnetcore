using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Vostok.Applications.AspNetCore.Tests.Extensions
{
    internal static class HttpClientExtensions
    {
        public static async Task<T> GetAsync<T>(this HttpClient httpClient, string url)
        {
            var json = await httpClient.GetStringAsync(url);
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}