﻿using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Hosting;
using Vostok.Applications.AspNetCore.Builders;
using Vostok.Applications.AspNetCore.Configuration;
using Vostok.Applications.AspNetCore.Helpers;
using Vostok.Applications.AspNetCore.Middlewares;
using Vostok.Commons.Threading;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Abstractions.Helpers;
using Vostok.Hosting.Abstractions.Requirements;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Context;
#if NETCOREAPP
using HostManager = Vostok.Applications.AspNetCore.HostBuilders.GenericHostManager;

#else
using HostManager = Vostok.Applications.AspNetCore.HostBuilders.WebHostManager;
#endif

namespace Vostok.Applications.AspNetCore
{
    /// <summary>
    /// <para><see cref="VostokAspNetCoreApplication{TStartup}" /> is the abstract class developers inherit from in order to create Vostok-compatible AspNetCore services.</para>
    /// <para>Implement <see cref="Setup" /> method to configure <see cref="IWebHostBuilder" /> and customize built-in Vostok middlewares (see
    /// <see
    ///     cref="IVostokAspNetCoreApplicationBuilder" />
    /// ).</para>
    /// <para>Override <see cref="WarmupAsync" /> method to perform any additional initialization after the DI container gets built but before the app gets registered in service discovery.</para>
    /// </summary>
    [PublicAPI]
    [RequiresPort]
    public abstract class VostokAspNetCoreApplication<TStartup> : IVostokApplication, IDisposable
        where TStartup : class
    {
        private readonly AtomicBoolean initialized = new AtomicBoolean(false);
        private volatile HostManager manager;
        private volatile VostokDisposables disposables;
        
        public virtual async Task InitializeAsync(IVostokHostingEnvironment environment)
        {
            disposables = new VostokDisposables(environment.Log);
            var log = environment.Log.ForContext<VostokAspNetCoreApplication>();

            var builder = new VostokAspNetCoreApplicationBuilder<TStartup>(environment, this, disposables);

            builder.SetupPingApi(PingApiSettingsSetup.Get(environment, GetType(), initialized));
            builder.SetupDistributedContext(settings => settings.AdditionalActions.Add(DistributedContextSetup.RestoreOpenTelemetryTracingContext));

            Setup(builder, environment);

            disposables.Add(manager = new HostManager(builder.BuildHost(), log));

            using (new OperationContextToken("Warmup"))
                await WarmupServicesAsync(environment, manager.Services);

            await manager.StartHostAsync(environment.ShutdownToken, environment.HostExtensions.TryGet<IVostokHostShutdown>(out var shutdown) ? shutdown : null);

            using (new OperationContextToken("Warmup"))
            {
                await WarmupAsync(environment, manager.Services);

                if (builder.IsMiddlewareEnabled<PingApiMiddleware>())
                    await MiddlewaresWarmup.WarmupPingApi(environment);
            }

            initialized.TrySetTrue();
        }

        public Task RunAsync(IVostokHostingEnvironment environment) =>
            manager.RunHostAsync();

        /// <summary>
        /// Override this method to configure <see cref="IWebHostBuilder" /> and customize built-in Vostok middleware components.
        /// </summary>
        public virtual void Setup([NotNull] IVostokAspNetCoreApplicationBuilder builder, [NotNull] IVostokHostingEnvironment environment)
        {
        }

        /// <summary>
        /// Override this method to perform any initialization that needs to happen after DI container is built but before host is started.
        /// </summary>
        public virtual Task WarmupServicesAsync([NotNull] IVostokHostingEnvironment environment, [NotNull] IServiceProvider serviceProvider) =>
            Task.CompletedTask;

        /// <summary>
        /// Override this method to perform any initialization that needs to happen after DI container is built and host is started, but before registering in service discovery.
        /// </summary>
        public virtual Task WarmupAsync([NotNull] IVostokHostingEnvironment environment, [NotNull] IServiceProvider serviceProvider) =>
            Task.CompletedTask;

        /// <summary>
        /// Override this method to perform any dispose actions that needs to happen after host has been stopped.
        /// </summary>
        public virtual void DoDispose()
        {
        }

        /// <summary>
        /// Override this method to perform any Async dispose actions that needs to happen after host has been stopped.
        /// </summary>
        public virtual Task DoDisposeAsync()
        {
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            disposables?.Dispose();
            DoDisposeAsync().GetAwaiter().GetResult();
            DoDispose();
        }
    }
}