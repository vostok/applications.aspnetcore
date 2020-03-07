using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Vostok.Applications.AspNetCore.Middlewares;

namespace Vostok.Applications.AspNetCore.Configuration
{
    /// <summary>
    /// Represents configuration of <see cref="DistributedContextMiddleware"/>.
    /// </summary>
    [PublicAPI]
    public class DistributedContextSettings
    {
        /// <summary>
        /// Additional actions that will be executed during distributed context restoring.
        /// </summary>
        [NotNull]
        public List<Action<HttpRequest>> AdditionalActions { get; set; } = new List<Action<HttpRequest>>();
    }
}