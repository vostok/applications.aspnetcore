#if NET6_0
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Vostok.Applications.AspNetCore.Builders;
using Vostok.Applications.AspNetCore.Configuration;
using Vostok.Applications.AspNetCore.Helpers;
using Vostok.Applications.AspNetCore.HostBuilders;
using Vostok.Applications.AspNetCore.Middlewares;
using Vostok.Commons.Threading;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Abstractions.Helpers;
using Vostok.Hosting.Abstractions.Requirements;
using Vostok.Logging.Abstractions;

namespace Vostok.Applications.AspNetCore
{
    /// <summary>
    /// <para><see cref="VostokAspNetCoreWebApplication" /> is the abstract class developers inherit from in order to create Vostok-compatible AspNetCore services.</para>
    /// <para>Implement <see cref="SetupAsync" /> method to configure <see cref="WebApplicationBuilder" />, <see cref="WebApplication" /> and customize built-in Vostok middlewares (see
    /// <see
    ///     cref="IVostokAspNetCoreWebApplicationBuilder" />
    /// ).</para>
    /// <para>Override <see cref="WarmupAsync" /> method to perform any additional initialization after the DI container gets built but before the app gets registered in service discovery.</para>
    /// </summary>
    [PublicAPI]
    [RequiresPort]
    public abstract class VostokAspNetCoreWebApplication : IVostokApplication, IDisposable
    {
        private readonly List<IDisposable> disposables = new List<IDisposable>();
        private readonly AtomicBoolean initialized = new AtomicBoolean(false);
        private volatile GenericHostManager manager;

        public virtual async Task InitializeAsync(IVostokHostingEnvironment environment)
        {
            var log = environment.Log.ForContext<VostokAspNetCoreWebApplication>();

            var builder = new VostokAspNetCoreWebApplicationBuilder(environment, this, disposables);

            builder.SetupPingApi(PingApiSettingsSetup.Get(environment, GetType(), initialized));

            await SetupAsync(builder, environment);

            var webApplication = builder.Build();
            
            disposables.Add(manager = new GenericHostManager(webApplication, log));

            await WarmupServicesAsync(environment, webApplication);

            await manager.StartHostAsync(environment.ShutdownToken, environment.HostExtensions.TryGet<IVostokHostShutdown>(out var shutdown) ? shutdown : null);

            await WarmupAsync(environment, webApplication);

            if (builder.IsMiddlewareEnabled<PingApiMiddleware>())
                await MiddlewaresWarmup.WarmupPingApi(environment);

            initialized.TrySetTrue();
        }

        public Task RunAsync(IVostokHostingEnvironment environment) =>
            manager.RunHostAsync();

        /// <summary>
        /// Override this method to configure <see cref="WebApplicationBuilder" />, <see cref="WebApplication" /> and customize built-in Vostok middleware components.
        /// </summary>
        public abstract Task SetupAsync([NotNull] IVostokAspNetCoreWebApplicationBuilder builder, [NotNull] IVostokHostingEnvironment environment);

        /// <summary>
        /// Override this method to perform any initialization that needs to happen after DI container is built but before host is started.
        /// </summary>
        public virtual Task WarmupServicesAsync([NotNull] IVostokHostingEnvironment environment, [NotNull] WebApplication webApplication) =>
            Task.CompletedTask;

        /// <summary>
        /// Override this method to perform any initialization that needs to happen after DI container is built and host is started, but before registering in service discovery.
        /// </summary>
        public virtual Task WarmupAsync([NotNull] IVostokHostingEnvironment environment, [NotNull] WebApplication webApplication) =>
            Task.CompletedTask;

        /// <summary>
        /// Override this method to perform any dispose actions that needs to happen after host has been stopped.
        /// </summary>
        public virtual Task DoDisposeAsync() =>
            Task.CompletedTask;

        public void Dispose()
        {
            disposables.ForEach(disposable => disposable?.Dispose());
            DoDisposeAsync().GetAwaiter().GetResult();
        }
    }
}
#endif