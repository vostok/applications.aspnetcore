#if NET6_0_OR_GREATER
using JetBrains.Annotations;

namespace Vostok.Applications.AspNetCore.OpenTelemetry;

[PublicAPI]
public class VostokOpenTelemetryMeterProviderOptions
{
    public bool AddService { get; set; }

    public bool AddProject { get; set; } = true;
    public bool AddSubproject { get; set; } = true;
    public bool AddEnvironment { get; set; } = true;
    public bool AddApplication { get; set; } = true;
    public bool AddInstance { get; set; } = true;
}
#endif