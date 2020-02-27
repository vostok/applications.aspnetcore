using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Hosting;
using Vostok.Applications.AspNetCore.Builders;
using Vostok.Commons.Helpers.Extensions;
using Vostok.Hosting.Abstractions;
using Vostok.Logging.Abstractions;

namespace Vostok.Applications.AspNetCore
{
    /// <summary>
    /// <para><see cref="VostokNetCoreApplication"/> is the abstract class developers inherit from in order to create Vostok-compatible NetCore applications.</para>
    /// <para>Implement <see cref="Setup"/> method to configure <see cref="IHostBuilder"/> (see <see cref="IVostokNetCoreApplicationBuilder"/>).</para>
    /// </summary>
    [PublicAPI]
    public abstract class VostokNetCoreApplication : IVostokApplication, IDisposable
    {
        private readonly List<IDisposable> disposables = new List<IDisposable>();

        protected volatile IHostApplicationLifetime Lifetime;
        protected volatile IHost Host;
        protected volatile ILog Log;

        public async Task InitializeAsync(IVostokHostingEnvironment environment)
        {
            Log = environment.Log.ForContext<VostokNetCoreApplication>();
            
            var builder = new VostokNetCoreApplicationBuilder(environment);

            Setup(builder, environment);

            disposables.Add(Host = builder.Build());

            await StartHostAsync(environment).ConfigureAwait(false);
        }

        public Task RunAsync(IVostokHostingEnvironment environment) =>
            RunHostAsync();

        /// <summary>
        /// Override this method to configure <see cref="IHostBuilder"/> and customize built-in Vostok middleware components.
        /// </summary>
        public virtual void Setup([NotNull] IVostokNetCoreApplicationBuilder builder, [NotNull] IVostokHostingEnvironment environment)
        {
        }

        public void Dispose()
            => disposables.ForEach(disposable => disposable?.Dispose());

        protected async Task StartHostAsync(IVostokHostingEnvironment environment)
        {
            Lifetime = (IHostApplicationLifetime)Host.Services.GetService(typeof(IHostApplicationLifetime));

            disposables.Add(
                environment.ShutdownToken.Register(
                    () => Host
                        .StopAsync()
                        .ContinueWith(t => Log.Error(t.Exception, "Failed to stop Host."), TaskContinuationOptions.OnlyOnFaulted)));

            Log.Info("Starting Host.");

            await Host.StartAsync(environment.ShutdownToken).ConfigureAwait(false);

            await Lifetime.ApplicationStarted.WaitAsync().ConfigureAwait(false);

            Log.Info("Host started.");
        }

        private async Task RunHostAsync()
        {
            await Lifetime.ApplicationStopping.WaitAsync().ConfigureAwait(false);
            Log.Info("Stopping Host.");

            await Lifetime.ApplicationStopped.WaitAsync().ConfigureAwait(false);
            Log.Info("Host stopped.");

            Host.Dispose();
        }
    }
}