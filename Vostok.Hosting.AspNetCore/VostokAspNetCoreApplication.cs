using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Abstractions.Requirements;
using Vostok.Hosting.AspNetCore.Builders;
using Vostok.Logging.Abstractions;

namespace Vostok.Hosting.AspNetCore
{
    /// <summary>
    /// <para><see cref="VostokAspNetCoreApplication"/> is the abstract class developers inherit from in order to create Vostok-compatible AspNetCore service.</para>
    /// <para>Implement <see cref="Setup"/> method to configure <see cref="IWebHostBuilder"/> and customize built-in Vostok middlewares (see <see cref="IVostokAspNetCoreApplicationBuilder"/>).</para>
    /// <para>Override <see cref="WarmupAsync"/> method to perform any additional initialization after the DI container gets built but before the app gets registered in service discovery.</para>
    /// </summary>
    [PublicAPI]
    [RequiresPort]
    public abstract class VostokAspNetCoreApplication : IVostokApplication, IDisposable
    {
        private IHostApplicationLifetime lifetime;
        private ILog log;
        private IHost webHost;

        public async Task InitializeAsync(IVostokHostingEnvironment environment)
        {
            var builder = new AspNetCoreApplicationBuilder();

            Setup(builder, environment);

            webHost = builder.Build(environment);

            await StartWebHostAsync(environment).ConfigureAwait(false);

            // CR(iloktionov): А почему warmup вызывается после того, как приложение уже начало слушать порт? Оно может быть ещё не готово к этому.
            // CR(kungurtsev): Чтобы иметь возможность подёргать за контроллеры и прогреть их.
            await WarmupAsync(environment, webHost.Services).ConfigureAwait(false);
        }

        public Task RunAsync(IVostokHostingEnvironment environment)
        {
            RunWebHost();

            return Task.CompletedTask;
        }

        /// <summary>
        /// Implement this method to configure <see cref="IWebHostBuilder"/> and customize built-in Vostok middleware components.
        /// </summary>
        public abstract void Setup([NotNull] IVostokAspNetCoreApplicationBuilder builder, [NotNull] IVostokHostingEnvironment environment);

        /// <summary>
        /// Override this method to perform any initialization that needs to happen after DI container is built, but before registering in service discovery.
        /// </summary>
        public virtual Task WarmupAsync([NotNull] IVostokHostingEnvironment environment, [NotNull] IServiceProvider serviceProvider) =>
            Task.CompletedTask;

        private async Task StartWebHostAsync(IVostokHostingEnvironment environment)
        {
            log = environment.Log.ForContext<VostokAspNetCoreApplication>();

            lifetime = (IHostApplicationLifetime)webHost.Services.GetService(typeof(IHostApplicationLifetime));

            // CR(iloktionov): Результат вот этой регистрации по-хорошему тоже надо диспоузить в конце.
            environment.ShutdownToken.Register(
                () => webHost
                    .StopAsync()
                    .ContinueWith(t => log.Error(t.Exception, "Failed to stop WebHost."), TaskContinuationOptions.OnlyOnFaulted));

            log.Info("Starting WebHost.");

            // CR(iloktionov): А почему в StartAsync не передается ShutdownToken?
            await webHost.StartAsync().ConfigureAwait(false);

            // CR(iloktionov): Давай сделаем асинхронно. Какой-нибудь экстеншн с TaskCompletionSource, например. Относится и к остальным таким местам.
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

        public void Dispose()
        {
            webHost?.Dispose();
        }
    }
}
