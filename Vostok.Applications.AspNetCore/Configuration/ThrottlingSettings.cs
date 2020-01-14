using System;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Vostok.Throttling;
using Vostok.Throttling.Metrics;
using Vostok.Throttling.Quotas;

namespace Vostok.Applications.AspNetCore.Configuration
{
    [PublicAPI]
    public class ThrottlingSettings
    {
        /// <summary>
        /// <para>HTTP code to respond with when a request fails to pass throttling.</para>
        /// <para>Note that in some circumstances (expired timeout, large request body) client connection may be aborted instead.</para>
        /// </summary>
        public int RejectionResponseCode { get; set; } = 429;

        /// <summary>
        /// If set to <c>true</c>, the middleware will pass <see cref="WellKnownThrottlingProperties.Consumer"/> property to <see cref="IThrottlingProvider"/>.
        /// </summary>
        public bool AddConsumerProperty { get; set; } = true;

        /// <summary>
        /// If set to <c>true</c>, the middleware will pass <see cref="WellKnownThrottlingProperties.Priority"/> property to <see cref="IThrottlingProvider"/>.
        /// </summary>
        public bool AddPriorityProperty { get; set; } = true;

        /// <summary>
        /// If set to <c>true</c>, the middleware will pass <see cref="WellKnownThrottlingProperties.Method"/> property to <see cref="IThrottlingProvider"/>.
        /// </summary>
        public bool AddMethodProperty { get; set; } = true;

        /// <summary>
        /// If set to <c>true</c>, the middleware will pass <see cref="WellKnownThrottlingProperties.Url"/> property to <see cref="IThrottlingProvider"/>.
        /// </summary>
        public bool AddUrlProperty { get; set; }

        /// <summary>
        /// <para>If set to <c>true</c>, adds an instance of <see cref="ThreadPoolOverloadQuota"/> to the throttling provider.</para>
        /// </summary>
        public bool UseThreadPoolOverloadQuota { get; set; } = true;

        /// <summary>
        /// <para>If set to <c>true</c>, disables throttling entirely for web socket requests.</para>
        /// <para>Use <see cref="KestrelSettings.MaxConcurrentWebSocketConnections"/> to limit web socket parallelism.</para>
        /// <para>Has precedence over <see cref="Enabled"/>.</para>
        /// </summary>
        public bool DisableForWebSockets { get; set; } = true;

        /// <summary>
        /// An optional delegate to decide whether to do (<c>true</c> return value) or skip (<c>false</c> return value) request throttling.
        /// </summary>
        [CanBeNull]
        public Func<HttpContext, bool> Enabled { get; set; }

        /// <summary>
        /// <para>Configuration of application's throttling metrics, enabled by default.</para>
        /// <para>Set this property to <c>null</c> to disable throttling metrics.</para>
        /// </summary>
        [CanBeNull]
        public ThrottlingMetricsOptions Metrics = new ThrottlingMetricsOptions();
    }
}
