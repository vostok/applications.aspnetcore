using System;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.AspNetCore.Middlewares;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Microsoft;
using Vostok.ServiceDiscovery.Abstractions;

namespace Vostok.Hosting.AspNetCore
{
    // CR(iloktionov): 1. Нет смысла перечислять всё, что это штука делает, в xml-доке. Есть смысл описать, как ей правильно пользоваться.

    // CR(iloktionov): 2. Если единственный обязательный шаг здесь — указать Startup, то, может, сделаем так, чтобы этого нельзя было случайно избежать?
    // CR(iloktionov):    Можно, например, сделать его generic-параметром нашего базового класса и регать самим, если забить на сценарий доставания из внешней сборки.
    // CR(iloktionov):    Или просто попытаться автоматически найти Startup в сборке: если он один, то зарегать его.
    // CR(iloktionov):    Кстати, я обнаружил, что в Startup можно инжектить через конструктор IVostokHostingEnvironment: неочевидная вещь и полезный совет.

    // CR(iloktionov): 3. Интерфейсы билдеров для каждого middleware выглядят избыточными и порождают двойную вложенность лямбд, которой можно и избежать.
    // CR(iloktionov):    Некоторые из них совсем тривиальные (SetupLoggingMiddleware, SetupTracingMiddleware).
    // CR(iloktionov):    Может, заменить их паттерном Action<Settings> для упрощения кода?

    // CR(iloktionov): 4. У нас местами очень длинные названия. Например, из названий методов Setup можно безопасно убрать слово Middleware :)

    /// <summary>
    /// <para>Represents a configuration of <see cref="VostokAspNetCoreApplication"/> builder which must be filled during <see cref="VostokAspNetCoreApplication.Setup"/>.</para>
    /// <para>It is required to setup <see cref="IWebHostBuilder"/> with custom <see cref="IStartup"/> class.</para>
    /// <para>Doing the following:</para>
    /// <list type="bullet">
    ///     <item><description>Configures url and url path from <see cref="IServiceBeacon"/>.</description></item>
    ///     <item><description>Registers Vostok <see cref="ILog"/> as Microsoft <see cref="ILogger"/>.</description></item>
    ///     <item><description>Registers Vostok <see cref="Configuration.Abstractions.IConfigurationSource"/> as Microsoft <see cref="Microsoft.Extensions.Configuration.IConfigurationSource"/>.</description></item>
    ///     <item><description>Registers all the fields from <see cref="IVostokHostingEnvironment"/> to <see cref="IServiceCollection"/>.</description></item>
    ///     <item><description>Adds <see cref="FillRequestInfoMiddleware"/>.</description></item>
    ///     <item><description>Adds <see cref="RestoreDistributedContextMiddleware"/>.</description></item>
    ///     <item><description>Adds <see cref="TracingMiddleware"/>.</description></item>
    ///     <item><description>Adds <see cref="LoggingMiddleware"/>.</description></item>
    ///     <item><description>Adds <see cref="DenyRequestsIfNotInActiveDatacenterMiddleware"/> (if configured).</description></item>
    ///     <item><description>Adds <see cref="PingApiMiddleware"/>.</description></item>
    ///     <item><description>Applies user given <see cref="IWebHostBuilder"/> configuration.</description></item>
    /// </list>
    /// </summary>
    [PublicAPI]
    public interface IVostokAspNetCoreApplicationBuilder
    {
        /// <summary>
        /// Delegate which configures <see cref="IWebHostBuilder"/>.
        /// </summary>
        IVostokAspNetCoreApplicationBuilder SetupWebHost([NotNull] Action<IWebHostBuilder> setup);

        /// <summary>
        /// Delegate which configures <see cref="LoggingMiddlewareSettings"/>.
        /// </summary>
        IVostokAspNetCoreApplicationBuilder SetupLogging([NotNull] Action<LoggingMiddlewareSettings> setup);

        /// <summary>
        /// Delegate which configures <see cref="TracingMiddlewareSettings"/>.
        /// </summary>
        IVostokAspNetCoreApplicationBuilder SetupTracing([NotNull] Action<TracingMiddlewareSettings> setup);

        /// <summary>
        /// <para>Denies request processing, if local datacenter is not active.</para>
        /// <para>Use this option only if your application hosted in multiple datacenters.</para>
        /// </summary>
        IVostokAspNetCoreApplicationBuilder DenyRequestsIfNotInActiveDatacenter(int denyResponseCode = (int)Clusterclient.Core.Model.ResponseCode.ServiceUnavailable);

        /// <summary>
        /// Delegate which configures <see cref="PingApiMiddlewareSettings"/>.
        /// </summary>
        IVostokAspNetCoreApplicationBuilder SetupPingApi([NotNull] Action<PingApiMiddlewareSettings> setup);

        /// <summary>
        /// Delegate which configures <see cref="FillRequestInfoMiddlewareSettings"/>.
        /// </summary>
        IVostokAspNetCoreApplicationBuilder SetupFillRequestInfo([NotNull] Action<FillRequestInfoMiddlewareSettings> setup);

        /// <summary>
        /// Delegate which configures <see cref="RestoreDistributedContextMiddlewareSettings"/>.
        /// </summary>
        IVostokAspNetCoreApplicationBuilder SetupRestoreDistributedContext([NotNull] Action<RestoreDistributedContextMiddlewareSettings> setup);

        /// <summary>
        /// Delegate which configures <see cref="VostokLoggerProviderSettings"/>.
        /// </summary>
        IVostokAspNetCoreApplicationBuilder SetupMicrosoftLog([NotNull] Action<VostokLoggerProviderSettings> setup);
    }
}