using System;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.AspNetCore.Middlewares;
using Vostok.Logging.Abstractions;
using Vostok.ServiceDiscovery.Abstractions;

namespace Vostok.Hosting.AspNetCore.Setup
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
    ///     <item><description>Adds <see cref="DenyRequestsMiddleware"/> (if configured).</description></item>
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
        /// Delegate which configures <see cref="IVostokLoggingMiddlewareBuilder"/>.
        /// </summary>
        IVostokAspNetCoreApplicationBuilder SetupLoggingMiddleware([NotNull] Action<IVostokLoggingMiddlewareBuilder> setup);

        /// <summary>
        /// Delegate which configures <see cref="IVostokTracingMiddlewareBuilder"/>.
        /// </summary>
        IVostokAspNetCoreApplicationBuilder SetupTracingMiddleware([NotNull] Action<IVostokTracingMiddlewareBuilder> setup);

        /// <summary>
        /// Allows request processing, even if current datacenter is not active.
        /// </summary>
        IVostokAspNetCoreApplicationBuilder AllowRequestsIfNotInActiveDatacenter();

        // CR(iloktionov): 1. Здесь каноничный код — 503.
        // CR(iloktionov): 2. Давай это будет не generic middleware, который отсекает что угодно по Func'у, а именно штука про дата-центры?
        // CR(iloktionov): 3. Зачем нам и allow, и deny? Каково умолчание?
        /// <summary>
        /// Denies request processing, if current datacenter is not active.
        /// </summary>
        IVostokAspNetCoreApplicationBuilder DenyRequestsIfNotInActiveDatacenter(int denyResponseCode = (int)Clusterclient.Core.Model.ResponseCode.Gone);

        /// <summary>
        /// Delegate which configures <see cref="IVostokPingApiMiddlewareBuilder"/>.
        /// </summary>
        IVostokAspNetCoreApplicationBuilder SetupPingApiMiddleware([NotNull] Action<IVostokPingApiMiddlewareBuilder> setup);

        /// <summary>
        /// Delegate which configures <see cref="IVostokFillRequestInfoMiddlewareBuilder"/>.
        /// </summary>
        IVostokAspNetCoreApplicationBuilder SetupFillRequestInfoMiddleware([NotNull] Action<IVostokFillRequestInfoMiddlewareBuilder> setup);

        /// <summary>
        /// Delegate which configures <see cref="IVostokRestoreDistributedContextMiddlewareBuilder"/>.
        /// </summary>
        IVostokAspNetCoreApplicationBuilder SetupRestoreDistributedContextMiddleware([NotNull] Action<IVostokRestoreDistributedContextMiddlewareBuilder> setup);

        /// <summary>
        /// Delegate which configures <see cref="IVostokMicrosoftLogBuilder"/>.
        /// </summary>
        IVostokAspNetCoreApplicationBuilder SetupMicrosoftLog([NotNull] Action<IVostokMicrosoftLogBuilder> setup);
    }
}