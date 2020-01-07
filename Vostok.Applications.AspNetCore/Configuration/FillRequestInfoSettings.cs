using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Vostok.Applications.AspNetCore.Models;
using Vostok.Clusterclient.Core.Model;

namespace Vostok.Applications.AspNetCore.Configuration
{
    [PublicAPI]
    public class FillRequestInfoSettings
    {
        /// <summary>
        /// <para>A list of optional delegates that obtain <see cref="IRequestInfo.Timeout"/> from <see cref="HttpRequest"/>.</para>
        /// <para>By default, the middleware parses the value of <see cref="HeaderNames.RequestTimeout"/> header.</para>
        /// <para>Provided delegates are not expected to throw exceptions; <c>null</c> should be returned instead.</para>
        /// </summary>
        [NotNull]
        public List<Func<HttpRequest, TimeSpan?>> AdditionalTimeoutProviders { get; } = new List<Func<HttpRequest, TimeSpan?>>();

        /// <summary>
        /// <para>A list of optional delegates that obtain <see cref="IRequestInfo.Priority"/> from <see cref="HttpRequest"/>.</para>
        /// <para>By default, the middleware parses the value of <see cref="HeaderNames.RequestPriority"/> header.</para>
        /// <para>Provided delegates are not expected to throw exceptions; <c>null</c> should be returned instead.</para>
        /// </summary>
        [NotNull]
        public List<Func<HttpRequest, RequestPriority?>> AdditionalPriorityProviders { get; } = new List<Func<HttpRequest, RequestPriority?>>();

        /// <summary>
        /// <para>A list of optional delegates that obtain <see cref="IRequestInfo.ClientApplicationIdentity"/> from <see cref="HttpRequest"/>.</para>
        /// <para>By default, the middleware parses the value of<see cref="HeaderNames.ApplicationIdentity"/> header.</para>
        /// <para>Provided delegates are not expected to throw exceptions; <c>null</c> should be returned instead.</para>
        /// </summary>
        [NotNull]
        public List<Func<HttpRequest, string>> AdditionalClientIdentityProviders { get; set; } = new List<Func<HttpRequest, string>>();
    }
}
