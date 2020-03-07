using System;
using System.Reflection;
using JetBrains.Annotations;

namespace Vostok.Applications.AspNetCore.Configuration
{
    [PublicAPI]
    public class PingApiSettings
    {
        /// <summary>
        /// <para>An optional delegate that returns <c>true</c> if the application is already initialized or <c>false</c> if warmup is still in progress.</para>
        /// <para>By default, application is always considered initialized.</para>
        /// </summary>
        [CanBeNull]
        public Func<bool> InitializationCheck { get; set; }

        /// <summary>
        /// <para>An optional delegate that returns <c>true</c> if the application is currently healthy or <c>false</c> if there are some warnings.</para>
        /// <para>By default, application is always considered healthy.</para>
        /// </summary>
        [CanBeNull]
        public Func<bool> HealthCheck { get; set; }

        /// <summary>
        /// <para>An optional delegate that returns application commit hash.</para>
        /// <para>By default, commit hash is extracted from <see cref="AssemblyTitleAttribute"/> of the entry assembly.</para>
        /// </summary>
        [CanBeNull]
        public Func<string> CommitHashProvider { get; set; }
    }
}