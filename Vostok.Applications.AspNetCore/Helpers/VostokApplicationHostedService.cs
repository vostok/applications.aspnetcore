using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Vostok.Hosting.Abstractions;
using Vostok.Logging.Abstractions;

namespace Vostok.Applications.AspNetCore.Helpers;

internal class VostokApplicationHostedService<TApplication> : IHostedService, IDisposable
    where TApplication : IVostokApplication
{
    private readonly TApplication application;
    private readonly IVostokHostingEnvironment environment;
    private readonly ILog log;
    private readonly CancellationTokenSource cancel;
    private volatile Task task;

    public VostokApplicationHostedService(TApplication application, IVostokHostingEnvironment environment)
    {
        this.application = application;

        cancel = new CancellationTokenSource();
        this.environment = environment.WithReplacedShutdownToken(cancel.Token);
        
        log = this.environment.Log.ForContext<VostokApplicationHostedService<TApplication>>();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        log.Info("Initializing application.");
        await application.InitializeAsync(environment);

        cancellationToken.ThrowIfCancellationRequested();

        log.Info("Running application.");
        // ReSharper disable once MethodSupportsCancellation
        task = Task.Run(() => application.RunAsync(environment));
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        log.Info("Stopping application..");
        await (task ?? Task.CompletedTask);
        log.Info("Application stopped.");
    }

    public void Dispose()
    {
        cancel.Dispose();
        (application as IDisposable)?.Dispose();
    }
}