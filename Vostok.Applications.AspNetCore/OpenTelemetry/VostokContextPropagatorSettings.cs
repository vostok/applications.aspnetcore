#if NET6_0_OR_GREATER
using System;
using JetBrains.Annotations;

namespace Vostok.Applications.AspNetCore.OpenTelemetry;

[PublicAPI]
public class VostokContextPropagatorSettings
{
    [CanBeNull]
    public Action<string, Exception> ErrorCallback { get; set; }
}
#endif