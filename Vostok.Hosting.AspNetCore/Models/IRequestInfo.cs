using System;
using System.Net;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Model;

namespace Vostok.Hosting.AspNetCore.Models
{
    /// <summary>
    /// Represents an incoming HTTP request information.
    /// </summary>
    [PublicAPI]
    public interface IRequestInfo
    {
        // CR(iloktionov): Кажется, бюджет удобнее, чем таймаут. Иначе всем самим придётся трекать, сколько осталось. Можно дополнить просто свойством RemainingTimeout, например.
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
        string ClientApplicationIdentity { get; }

        /// <summary>
        /// Ip address of request sender.
        /// </summary>
        [NotNull]
        IPAddress ClientIpAddress { get; }
    }
}