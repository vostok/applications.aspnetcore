using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Vostok.Applications.AspNetCore.Middlewares;
using Vostok.Applications.AspNetCore.Models;
using Vostok.Clusterclient.Core.Model;
using Vostok.Commons.Time;

namespace Vostok.Applications.AspNetCore.Configuration
{
    /// <summary>
    /// Represents configuration of <see cref="FillRequestInfoMiddleware"/>.
    /// </summary>
    [PublicAPI]
    public class FillRequestInfoSettings
    {
        /// <summary>
        /// <para>A delegate that provides a default value for <see cref="IRequestInfo.Timeout"/>.</para>
        /// <para>Used when the request does not contain any supported headers with timeout value.</para>
        /// <para>Executed after built-in provider and <see cref="AdditionalTimeoutProviders"/>.</para>
        /// </summary>
        [NotNull]
        public Func<HttpRequest, TimeSpan> DefaultTimeoutProvider { get; set; } = _ => 1.Minutes();

        /// <summary>
        /// <para>A delegate that provides a default value for <see cref="IRequestInfo.Priority"/>.</para>
        /// <para>Used when the request does not contain any supported headers with priority value.</para>
        /// <para>Executed after built-in provider and <see cref="AdditionalPriorityProviders"/>.</para>
        /// </summary>
        [NotNull]
        public Func<HttpRequest, RequestPriority> DefaultPriorityProvider { get; set; } = _ => RequestPriority.Ordinary;

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