using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Vostok.Commons.Helpers.Extensions;
using Vostok.Hosting.Abstractions;
using Vostok.Logging.Abstractions;
// ReSharper disable MethodSupportsCancellation

namespace Vostok.Applications.AspNetCore.Helpers;

internal class VostokApplicationHostedService<TApplication> : IHostedService
    where TApplication : IVostokApplication
{
    private readonly TApplication application;
    private readonly IVostokHostingEnvironment environment;
    private readonly ILog log;
    private readonly CancellationTokenSource cancel;
    private volatile Task runApplicationTask;

    public VostokApplicationHostedService(TApplication application, IVostokHostingEnvironment environment)
    {
        this.application = application;

        cancel = new CancellationTokenSource();
        this.environment = environment.WithReplacedShutdownToken(cancel.Token);
        
        log = this.environment.Log.ForContext<VostokApplicationHostedService<TApplication>>();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        log.Info("Initializing application..");
        var initializeApplicationTask = Task.Run(() => application.InitializeAsync(environment));
        
        if (await initializeApplicationTask.TryWaitAsync(cancellationToken))
            log.Info("Application initialized.");
        else
            log.Info("Application hasn't initialized.");

        cancellationToken.ThrowIfCancellationRequested();

        log.Info("Running application.");
        runApplicationTask = Task.Run(() => application.RunAsync(environment));
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (runApplicationTask == null)
            return;
        
        log.Info("Stopping application..");
        cancel.Cancel();
        
        if (await runApplicationTask.TryWaitAsync(cancellationToken))
            log.Info("Application stopped.");
        else
            log.Info("Application hasn't stopped.");
    }
}