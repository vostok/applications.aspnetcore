using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Http;
using Vostok.Clusterclient.Core.Model;
using Vostok.Commons.Time;
using Vostok.Hosting.AspNetCore.Middlewares;

namespace Vostok.Hosting.AspNetCore.Helpers
{
    internal static class HttpRequestExtensions
    {
        public static IPAddress GetClientIpAddress(this HttpRequest request) =>
            request.HttpContext.Connection.RemoteIpAddress;

        public static string GetClientConnectionInfo(this HttpRequest request)
        {
            var connection = request.HttpContext.Connection;
            return $"{connection.RemoteIpAddress}:{connection.RemotePort}";
        }

        public static string GetClientApplicationIdentity(this HttpRequest request)
        {
            return request.Headers[HeaderNames.ApplicationIdentity];
        }

        public static RequestPriority? GetPriority(this HttpRequest request)
        {
            if (Enum.TryParse(request.Headers[HeaderNames.RequestPriority], out RequestPriority priority))
                return priority;
            return null;
        }

        public static TimeSpan? GetTimeout(this HttpRequest request)
        {
            if (!double.TryParse(request.Headers[HeaderNames.RequestTimeout], NumberStyles.Any, CultureInfo.InvariantCulture, out var seconds))
                return null;

            return seconds.Seconds();
        }

        public static string FormatPath(this HttpRequest request, LoggingCollectionMiddlewareSettings logQueryStringSettings)
        {
            var builder = new StringBuilder();

            builder.Append(request.Method);
            builder.Append(" ");

            builder.Append(request.Path);

            if (logQueryStringSettings.IsEnabledForRequest(request))
            {
                if (logQueryStringSettings.IsEnabledForAllKeys())
                {
                    builder.Append(request.QueryString);
                }
                else
                {
                    var filtered = request.Query.Where(kvp => logQueryStringSettings.IsEnabledForKey(kvp.Key)).ToList();

                    for (var i = 0; i < filtered.Count; i++)
                    {
                        if (i == 0)
                            builder.Append("?");
                        builder.Append($"{filtered[i].Key}={filtered[i].Value}");
                    }
                }
            }
            
            return builder.ToString();
        }

        public static string FormatHeaders(this HttpRequest request, LoggingCollectionMiddlewareSettings settingsLogRequestHeaders)
        {
            var builder = new StringBuilder();

            foreach (var header in request.Headers)
            {
                if (!settingsLogRequestHeaders.IsEnabledForKey(header.Key))
                    continue;

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