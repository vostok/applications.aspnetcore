using System;
using System.Net;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Vostok.Clusterclient.Core.Model;
using Vostok.Hosting.AspNetCore.Helpers;
using Vostok.Hosting.AspNetCore.Models;

namespace Vostok.Hosting.AspNetCore.Middlewares
{
    [PublicAPI]
    public class FillRequestInfoMiddlewareSettings
    {
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

        /// <summary>
        /// <para>A delegate that obtains <see cref="IRequestInfo.ClientIpAddress"/> from <see cref="HttpRequest"/>.</para>
        /// <para>By default, returns value from <c>HttpContext.Connection.RemoteIpAddress/></c>.</para>
        /// </summary>
        [CanBeNull]
        public Func<HttpRequest, IPAddress> ClientIpAddressProvider { get; set; } = request => request.GetClientIpAddress();
    }
}