using System;
using JetBrains.Annotations;

namespace Vostok.Applications.AspNetCore.OpenTelemetry;

// note (ponomaryovigor, 28.11.2024): Partially copied from Singular.
internal static class TraceParentHeaderHelper
{
    private static readonly char[] Format = {'n'};
    private const int HeaderLength = 2 + 1 + 32 + 1 + 16 + 1 + 2;

    public static bool TryParse([CanBeNull] string traceParent, out Guid traceId, out Guid spanId)
    {
        traceId = Guid.Empty;
        spanId = Guid.Empty;

        if (traceParent == null || traceParent.Length != HeaderLength)
            return false;

        if (traceParent[2] != '-' || traceParent[35] != '-' || traceParent[52] != '-')
            return false;

        var version = traceParent[0] == '0' && traceParent[1] == '0' ? 0 : -1;
        if (version != 0)
            return false;

#if NETSTANDARD2_0
        if (!Guid.TryParseExact(traceParent.Substring(3, 32), "n", out traceId))
#else
        if (!Guid.TryParseExact(traceParent.AsSpan(3, 32), Format.AsSpan(), out traceId))
#endif
            return false;

        var parentId = traceParent.AsSpan(36, 16);
        Span<char> parentSpan = stackalloc char[32];
        parentSpan.Fill('0');
        parentId.CopyTo(parentSpan);
#if NETSTANDARD2_0
        if (!Guid.TryParseExact(parentSpan.ToString(), "N", out spanId))
#else
        if (!Guid.TryParseExact(parentSpan, Format.AsSpan(), out spanId))
#endif
            return false;

        return traceId != Guid.Empty && spanId != Guid.Empty;
    }
}