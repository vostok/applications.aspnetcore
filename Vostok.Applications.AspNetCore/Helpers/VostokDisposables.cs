using System;
using System.Collections.Generic;
using Vostok.Logging.Abstractions;

namespace Vostok.Applications.AspNetCore.Helpers;

internal class VostokDisposables : IDisposable
{
    private readonly ILog log;
    private readonly List<IDisposable> disposables = new List<IDisposable>();

    public VostokDisposables(ILog log) =>
        this.log = log;

    public void Add(IDisposable disposable)
    {
        lock (disposables)
            disposables.Add(disposable);
    }

    public void Dispose()
    {
        lock (disposables)
        {
            log.ForContext("VostokHostingEnvironment").Info("Disposing of Disposables list ({Count} element(s))..", disposables.Count);
            disposables.ForEach(disposable => disposable?.Dispose());
            disposables.Clear();
        }
    }
}