using Microsoft.AspNetCore.Http;
using Vostok.Applications.AspNetCore.OpenTelemetry;
using Vostok.Context;
using Vostok.Tracing.Abstractions;

namespace Vostok.Applications.AspNetCore.Configuration;

internal static class DistributedContextSetup
{
    private const string TraceParentHeader = "traceparent";

    public static void RestoreOpenTelemetryTracingContext(HttpRequest request)
    {
        if (FlowingContext.Globals.Get<TraceContext>() != null)
            return;

        if (!request.Headers.TryGetValue(TraceParentHeader, out var header))
            return;

        if (!TraceParentHeaderHelper.TryParseV0(header, out var traceId, out var spanId))
            return;

        FlowingContext.Globals.Set(new TraceContext(traceId, spanId));
    }
}