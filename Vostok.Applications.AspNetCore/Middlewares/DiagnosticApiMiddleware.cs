using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Options;
using Vostok.Applications.AspNetCore.Configuration;
using Vostok.Clusterclient.Core.Model;
using Vostok.Configuration;
using Vostok.Configuration.Printing;
using Vostok.Hosting.Abstractions.Diagnostics;
using Vostok.Logging.Abstractions;

namespace Vostok.Applications.AspNetCore.Middlewares
{
    /// <summary>
    /// Handles a wide range of diagnostic info requests under <c>/_diagnostic</c> prefix.
    /// </summary>
    [PublicAPI]
    public class DiagnosticApiMiddleware
    {
        private readonly RequestDelegate next;
        private readonly DiagnosticApiSettings options;
        private readonly IDiagnosticInfo info;
        private readonly ILog log;
        private readonly string pathPrefix;

        public DiagnosticApiMiddleware(
            [NotNull] RequestDelegate next,
            [NotNull] IDiagnosticInfo info,
            [NotNull] IOptions<DiagnosticApiSettings> options,
            [NotNull] ILog log)
        {
            this.next = next ?? throw new ArgumentNullException(nameof(next));
            this.info = info ?? throw new ArgumentNullException(nameof(info));
            this.options = (options ?? throw new ArgumentNullException(nameof(options))).Value;
            this.log = (log ?? throw new ArgumentNullException(nameof(log))).ForContext<DiagnosticApiMiddleware>();

            pathPrefix = this.options.PathPrefix.TrimEnd('/');
        }

        public Task InvokeAsync(HttpContext context)
        {
            var diagnosticPath = GetDiagnosticPath(context);
            if (diagnosticPath == null)
                return next.Invoke(context);

            if (diagnosticPath == string.Empty)
                return HandleListRequest(context);

            if (DiagnosticEntry.TryParse(diagnosticPath, out var entry) && info.TryQuery(entry, out var payload))
                return HandleInfoRequest(context, payload);

            return next.Invoke(context);
        }

        private Task HandleInfoRequest(HttpContext context, object payload)
        {
            if (!TryAuthorize(context))
                return Task.CompletedTask;

            var printSettings = new PrintSettings { Format = PrintFormat.JSON };
            var responseBody = Encoding.UTF8.GetBytes(ConfigurationPrinter.Print(payload, printSettings));

            context.Response.StatusCode = 200;
            context.Response.ContentLength = responseBody.Length;
            context.Response.ContentType = "application/json";

            return context.Response.Body.WriteAsync(responseBody, 0, responseBody.Length);
        }

        private Task HandleListRequest(HttpContext context)
        {
            if (!TryAuthorize(context))
                return Task.CompletedTask;

            var responseBody = Encoding.UTF8.GetBytes(ComposeListPage(context));

            context.Response.StatusCode = 200;
            context.Response.ContentLength = responseBody.Length;
            context.Response.ContentType = "text/html";
            context.Response.Headers[HeaderNames.CacheControl] = "no-cache";

            return context.Response.Body.WriteAsync(responseBody, 0, responseBody.Length);
        }

        [CanBeNull]
        private string GetDiagnosticPath(HttpContext context)
        {
            var requestPath = context.Request.Path.Value;

            if (!requestPath.StartsWith(pathPrefix, StringComparison.OrdinalIgnoreCase))
                return null;

            return requestPath.Substring(pathPrefix.Length).Trim('/');
        }

        [NotNull]
        private string ComposeListPage(HttpContext context)
        {
            var builder = new StringBuilder();

            foreach (var group in info.ListAll().GroupBy(entry => entry.Component, StringComparer.OrdinalIgnoreCase))
            {
                builder.AppendLine($"<h2>{group.Key}</h2>");
                builder.AppendLine("<ul>");

                foreach (var entry in group.OrderBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase))
                {
                    builder.AppendLine($"<li><a href=\"{CreateInfoUrl(context.Request, entry)}\">{entry.Name}</a></li>");
                }

                builder.AppendLine("</ul>");
                builder.AppendLine("<br/>");
            }

            return builder.ToString();
        }

        private string CreateInfoUrl(HttpRequest request, DiagnosticEntry entry)
            => UriHelper.BuildAbsolute(
                request.Scheme,
                request.Host,
                request.PathBase,
                request.Path + new PathString('/' + entry.ToString()));

        private bool TryAuthorize(HttpContext context)
        {
            if (IsAuthorized(context))
                return true;

            log.Warn("Unauthorized request to '{Path}' from {Address}.", context.Request.Path, context.Connection.RemoteIpAddress);

            context.Response.StatusCode = 403;
            context.Response.ContentLength = 0;

            return false;
        }

        private bool IsAuthorized(HttpContext context)
        {
            if (options.AllowOnlyLocalRequests && !IsLocalRequest(context))
                return false;

            foreach (var header in options.ProhibitedHeaders)
                if (context.Request.Headers.ContainsKey(header))
                    return false;

            return true;
        }

        private bool IsLocalRequest(HttpContext context)
            => context.Connection.RemoteIpAddress.Equals(context.Connection.LocalIpAddress) ||
               IPAddress.IsLoopback(context.Connection.RemoteIpAddress);
    }
}
