using System;
using JetBrains.Annotations;
using Vostok.Applications.AspNetCore.Configuration;

namespace Vostok.Applications.AspNetCore.Builders;

[PublicAPI]
public interface IVostokKestrelBuilder
{
    /// <summary>
    /// Allows to customize advanced settings <see cref="KestrelSettings"/> for Kestrel server setup.
    /// </summary>
    void Customize(Action<KestrelSettings> setup);
}