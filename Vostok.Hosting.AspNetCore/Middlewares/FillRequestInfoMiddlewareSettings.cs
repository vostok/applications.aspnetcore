using System;
using System.Net;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Vostok.Clusterclient.Core.Model;
using Vostok.Hosting.AspNetCore.Helpers;
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
        public Func<HttpRequest, TimeSpan?> TimeoutProvider { get; set; } = request => request.GetTimeout();

        /// <summary>
        /// <para>A delegate that obtains <see cref="IRequestInfo.Priority"/> from <see cref="HttpRequest"/>.</para>
        /// <para>By default, parses value from <see cref="HeaderNames.RequestPriority"/> header.</para>
        /// </summary>
        [CanBeNull]
        public Func<HttpRequest, RequestPriority?> PriorityProvider { get; set; } = request => request.GetPriority();

        /// <summary>
        /// <para>A delegate that obtains <see cref="IRequestInfo.ClientApplicationIdentity"/> from <see cref="HttpRequest"/>.</para>
        /// <para>By default, parses value from <see cref="HeaderNames.ApplicationIdentity"/> header.</para>
        /// </summary>
        [CanBeNull]
        public Func<HttpRequest, string> ClientApplicationIdentityProvider { get; set; } = request => request.GetClientApplicationIdentity();

        // CR(iloktionov): Зачем это?
        /// <summary>
        /// <para>A delegate that obtains <see cref="IRequestInfo.ClientIpAddress"/> from <see cref="HttpRequest"/>.</para>
        /// <para>By default, returns value from <see cref="HttpContext.Connection.RemoteIpAddress"/>.</para>
        /// </summary>
        [CanBeNull]
        public Func<HttpRequest, IPAddress> ClientIpAddressProvider { get; set; } = request => request.GetClientIpAddress();
    }
}