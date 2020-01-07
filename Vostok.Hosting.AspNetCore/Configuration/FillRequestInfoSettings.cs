using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Vostok.Clusterclient.Core.Model;
using Vostok.Hosting.AspNetCore.Middlewares;
using Vostok.Hosting.AspNetCore.Models;

namespace Vostok.Hosting.AspNetCore.Configuration
{
    /// <summary>
    /// Configuration of the <see cref="FillRequestInfoMiddleware"/>.
    /// </summary>
    [PublicAPI]
    public class FillRequestInfoSettings
    {
        /// <summary>
        /// <para>A delegates that obtain <see cref="IRequestInfo.Timeout"/> from <see cref="HttpRequest"/>.</para>
        /// <para>By default, parses value from <see cref="HeaderNames.RequestTimeout"/> header.</para>
        /// </summary>
        [NotNull]
        public List<Func<HttpRequest, TimeSpan?>> AdditionalTimeoutProviders { get; } = new List<Func<HttpRequest, TimeSpan?>>();

        /// <summary>
        /// <para>A delegates that obtain <see cref="IRequestInfo.Priority"/> from <see cref="HttpRequest"/>.</para>
        /// <para>By default, parses value from <see cref="HeaderNames.RequestPriority"/> header.</para>
        /// </summary>
        [NotNull]
        public List<Func<HttpRequest, RequestPriority?>> AdditionalPriorityProviders { get; } = new List<Func<HttpRequest, RequestPriority?>>();

        /// <summary>
        /// <para>A delegates that obtain <see cref="IRequestInfo.ClientApplicationIdentity"/> from <see cref="HttpRequest"/>.</para>
        /// <para>By default, parses value from <see cref="HeaderNames.ApplicationIdentity"/> header.</para>
        /// </summary>
        [NotNull]
        public List<Func<HttpRequest, string>> AdditionalClientApplicationIdentityProviders { get; set; } = new List<Func<HttpRequest, string>>();
    }
}