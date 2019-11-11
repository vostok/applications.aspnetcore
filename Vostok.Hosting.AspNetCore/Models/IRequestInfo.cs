using System;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Model;

namespace Vostok.Hosting.AspNetCore.Models
{
    /// <summary>
    /// Represents incoming HTTP request information.
    /// </summary>
    [PublicAPI]
    public interface IRequestInfo
    {
        /// <summary>
        /// Request timeout.
        /// </summary>
        [CanBeNull]
        TimeSpan? Timeout { get; }

        /// <summary>
        /// Request priority.
        /// </summary>
        [CanBeNull]
        RequestPriority? Priority { get; }

        /// <summary>
        /// Application name of request sender.
        /// </summary>
        [CanBeNull]
        string ApplicationIdentity { get; }
    }
}