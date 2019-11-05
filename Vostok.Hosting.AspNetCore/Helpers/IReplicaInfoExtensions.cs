using System;
using JetBrains.Annotations;
using Vostok.ServiceDiscovery.Abstractions;

namespace Vostok.Hosting.AspNetCore.Helpers
{
    internal static class IReplicaInfoExtensions
    {
        [CanBeNull]
        public static Uri GetUrl([NotNull] this IReplicaInfo replicaInfo) =>
            !Uri.TryCreate(replicaInfo.Replica, UriKind.Absolute, out var url) ? null : url;
    }
}