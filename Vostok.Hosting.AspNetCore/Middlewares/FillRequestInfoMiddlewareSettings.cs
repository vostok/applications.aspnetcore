using System;
using System.Globalization;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Vostok.Clusterclient.Core.Model;
using Vostok.Commons.Time;
using Vostok.Hosting.AspNetCore.Models;

#pragma warning disable 1584,1581,1580, 1574

namespace Vostok.Hosting.AspNetCore.Middlewares
{
    /// <summary>
    /// Configuration of the <see cref="FillRequestInfoMiddleware"/>.
    /// </summary>
    [PublicAPI]
    public class FillRequestInfoMiddlewareSettings
    {
        // CR(iloktionov): Неудобно комбинировать (надо не забыть перевызвать старый func).
        /// <summary>
        /// <para>A delegate that obtains <see cref="IRequestInfo.Timeout"/> from <see cref="HttpRequest"/>.</para>
        /// <para>By default, parses value from <see cref="HeaderNames.RequestTimeout"/> header.</para>
        /// </summary>
        [CanBeNull]
        public Func<HttpRequest, TimeSpan?> TimeoutProvider { get; set; } = request =>
            double.TryParse(request.Headers[HeaderNames.RequestTimeout], NumberStyles.Any, CultureInfo.InvariantCulture, out var seconds) ? seconds.Seconds() : (TimeSpan?)null;

        /// <summary>
        /// <para>A delegate that obtains <see cref="IRequestInfo.Priority"/> from <see cref="HttpRequest"/>.</para>
        /// <para>By default, parses value from <see cref="HeaderNames.RequestPriority"/> header.</para>
        /// </summary>
        [CanBeNull]
        public Func<HttpRequest, RequestPriority?> PriorityProvider { get; set; } = request =>
            Enum.TryParse(request.Headers[HeaderNames.RequestPriority], true, out RequestPriority priority) ? (RequestPriority?)priority : null;

        /// <summary>
        /// <para>A delegate that obtains <see cref="IRequestInfo.ClientApplicationIdentity"/> from <see cref="HttpRequest"/>.</para>
        /// <para>By default, parses value from <see cref="HeaderNames.ApplicationIdentity"/> header.</para>
        /// </summary>
        [CanBeNull]
        public Func<HttpRequest, string> ClientApplicationIdentityProvider { get; set; } = request =>
            request.Headers[HeaderNames.ApplicationIdentity];
    }
}