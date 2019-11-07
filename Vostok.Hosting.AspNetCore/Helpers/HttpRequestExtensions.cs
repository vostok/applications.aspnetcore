using System;
using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Http;
using Vostok.Clusterclient.Core.Model;
using Vostok.Commons.Time;

namespace Vostok.Hosting.AspNetCore.Helpers
{
    internal static class HttpRequestExtensions
    {
        public static string GetClientConnectionInfo(this HttpRequest request)
        {
            var connection = request.HttpContext.Connection;
            return $"{connection.RemoteIpAddress}:{connection.RemotePort}";
        }

        public static string GetClientIdentity(this HttpRequest request)
        {
            return request.Headers[HeaderNames.ApplicationIdentity];
        }

        public static TimeSpan? GetTimeout(this HttpRequest request)
        {
            if (!double.TryParse(request.Headers[HeaderNames.RequestTimeout], NumberStyles.Any, CultureInfo.InvariantCulture, out var seconds))
                return null;

            return seconds.Seconds();
        }

        public static string FormatPath(this HttpRequest request, bool includeQuery)
        {
            var builder = new StringBuilder();

            builder.Append(request.Method);
            builder.Append(" ");

            builder.Append(request.Path);

            if (includeQuery)
            {
                builder.Append(request.QueryString);
            }
            
            return builder.ToString();
        }

        public static string FormatHeaders(this HttpRequest request)
        {
            var builder = new StringBuilder();

            foreach (var header in request.Headers)
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