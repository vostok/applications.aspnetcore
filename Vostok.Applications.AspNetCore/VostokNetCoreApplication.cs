#if NETCOREAPP3_1
using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Hosting;
using Vostok.Applications.AspNetCore.Builders;
using Vostok.Applications.AspNetCore.Helpers;
using Vostok.Hosting.Abstractions;
using Vostok.Logging.Abstractions;

namespace Vostok.Applications.AspNetCore
{
    /// <summary>
    /// <para><see cref="VostokNetCoreApplication"/> is the abstract class developers inherit from in order to create Vostok-compatible .Net Core applications based on Microsoft generic host.</para>
    /// <para>Implement <see cref="Setup"/> method to configure <see cref="IHostBuilder"/> (see <see cref="IVostokNetCoreApplicationBuilder"/>).</para>
    /// </summary>
    [PublicAPI]
    public abstract class VostokNetCoreApplication : IVostokApplication, IDisposable
    {
        private volatile GenericHostManager manager;

        public async Task InitializeAsync(IVostokHostingEnvironment environment)
        {
            var log = environment.Log.ForContext<VostokNetCoreApplication>();

            var hostBuilder = new GenericHostFactory(environment, this);

            var applicationBuilder = new VostokNetCoreApplicationBuilder(hostBuilder);

            Setup(applicationBuilder, environment);

            manager = new GenericHostManager(hostBuilder.CreateHost(), log);

            await manager.StartHostAsync(environment.ShutdownToken);
        }

        public Task RunAsync(IVostokHostingEnvironment environment) =>
            manager.RunHostAsync();

        /// <summary>
        /// Implement this method to configure <see cref="IHostBuilder"/>.
        /// </summary>
        public abstract void Setup([NotNull] IVostokNetCoreApplicationBuilder builder, [NotNull] IVostokHostingEnvironment environment);

        /// <summary>
        /// Override this method to perform any dispose actions that needs to happen after host has been stopped.
        /// </summary>
        public virtual void DoDispose()
        {
        }

        public void Dispose()
        {
            manager?.Dispose();
            DoDispose();
        }
    }
}
#endif