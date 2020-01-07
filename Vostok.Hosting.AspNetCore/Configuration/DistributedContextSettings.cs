using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;

namespace Vostok.Hosting.AspNetCore.Configuration
{
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