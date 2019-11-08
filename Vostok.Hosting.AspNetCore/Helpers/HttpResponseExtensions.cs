using System.Text;
using Microsoft.AspNetCore.Http;
using Vostok.Clusterclient.Core.Model;

namespace Vostok.Hosting.AspNetCore.Helpers
{
    internal static class HttpResponseExtensions
    {
        public static long? GetBodySize(this HttpResponse response)
        {
            if (long.TryParse(response.Headers[HeaderNames.ContentLength], out var length))
                return length;
            return null;
        }

        public static string FormatHeaders(this HttpResponse response)
        {
            var builder = new StringBuilder();

            foreach (var header in response.Headers)
            {
                builder.AppendLine();
                builder.Append("\t");
                builder.Append(header.Key);
                builder.Append(": ");
                builder.Append(header.Value);
            }

            return builder.ToString();
        }
    }
}