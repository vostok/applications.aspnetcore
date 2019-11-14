using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Hosting;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.AspNetCore.Builders;
using Vostok.Hosting.AspNetCore.Setup;
using Vostok.Logging.Abstractions;

namespace Vostok.Hosting.AspNetCore
{
    /// <summary>
    /// <para><see cref="VostokAspNetCoreApplication"/> is the abstract class developers implement in order to create Vostok-compatible AspNetCore service.</para>
    /// <para>Doing the following operations:</para>
    /// <para>On <see cref="IVostokApplication.InitializeAsync"/> phase:</para>
    /// <list type="bullet">
    ///     <item><description>Calls <see cref="Setup"/> method, that should be implemented and setup <see cref="IVostokAspNetCoreApplicationBuilder"/>.</description></item>
    ///     <item><description>Builds and run <see cref="IWebHostBuilder"/>.</description></item>
    ///     <item><description>Calls optional <see cref="WarmupAsync"/>.</description></item>
    /// </list>
    /// <para>On <see cref="IVostokApplication.RunAsync"/> phase:</para>
    /// <list type="bullet">
    ///     <item><description>Waits for the <see cref="IVostokHostingEnvironment.ShutdownToken"/> cancellation, and performs gracefully shutdown in this case.</description></item>
    /// </list>
    /// </summary>
    [PublicAPI]
    public abstract class VostokAspNetCoreApplication : IVostokApplication
    {
        private IApplicationLifetime lifetime;
        private ILog log;
        private IWebHost webHost;

        public async Task InitializeAsync(IVostokHostingEnvironment environment)
        {
            var builder = new AspNetCoreApplicationBuilder();

            Setup(builder, environment);

            webHost = builder.Build(environment);

            await StartWebHostAsync(environment).ConfigureAwait(false);

            await WarmupAsync(environment).ConfigureAwait(false);
        }

        public Task RunAsync(IVostokHostingEnvironment environment)
        {
            RunWebHost();

            return Task.CompletedTask;
        }

        /// <summary>
        /// Setup <see cref="IVostokAspNetCoreApplicationBuilder"/> using given <see cref="IVostokHostingEnvironment"/>.
        /// </summary>
        public abstract void Setup(IVostokAspNetCoreApplicationBuilder builder, IVostokHostingEnvironment environment);

        /// <summary>
        /// Warmup <see cref="VostokAspNetCoreApplication"/> before <see cref="IVostokHostingEnvironment.ServiceBeacon"/> will be started.
        /// </summary>
        public virtual Task WarmupAsync(IVostokHostingEnvironment environment)
            => Task.CompletedTask;

        private async Task StartWebHostAsync(IVostokHostingEnvironment environment)
        {
            log = environment.Log.ForContext<VostokAspNetCoreApplication>();

            lifetime = (IApplicationLifetime)webHost.Services.GetService(typeof(IApplicationLifetime));

            environment.ShutdownToken.Register(
                () => webHost
                    .StopAsync()
                    .ContinueWith(t => log.Error(t.Exception, "Failed to stop WebHost."), TaskContinuationOptions.OnlyOnFaulted));

            log.Info("Starting WebHost.");
            await webHost.StartAsync().ConfigureAwait(false);
            lifetime.ApplicationStarted.WaitHandle.WaitOne();
            log.Info("WebHost started.");
        }

        private void RunWebHost()
        {
            lifetime.ApplicationStopping.WaitHandle.WaitOne();
            log.Info("Stopping WebHost.");

            lifetime.ApplicationStopped.WaitHandle.WaitOne();
            log.Info("WebHost stopped.");

            webHost.Dispose();
        }
    }
}