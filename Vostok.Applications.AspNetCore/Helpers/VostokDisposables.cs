using System;
using System.Collections.Generic;
using System.Linq;
using Vostok.Logging.Abstractions;

namespace Vostok.Applications.AspNetCore.Helpers;

internal class VostokDisposables : IDisposable
{
    private readonly List<IDisposable> disposables = new List<IDisposable>();

    public void Add(IDisposable disposable)
    {
        lock (disposables)
            disposables.Add(disposable);
    }

    public void Dispose()
    {
        lock (disposables)
            if (disposables.Any())
            {
                LogProvider.Get().ForContext("VostokHostingEnvironment").Info("Disposing of Disposables list ({Count} element(s))..", disposables.Count);
                disposables.ForEach(disposable => disposable?.Dispose());
                disposables.Clear();
            }
    }
}