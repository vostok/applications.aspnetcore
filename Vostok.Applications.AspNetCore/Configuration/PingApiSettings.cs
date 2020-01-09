using System;
using System.Reflection;
using JetBrains.Annotations;
using Vostok.Commons.Environment;

namespace Vostok.Applications.AspNetCore.Configuration
{
    [PublicAPI]
    public class PingApiSettings
    {
        /// <summary>
        /// A delegate that returns current application status.
        /// </summary>
        [NotNull]
        public Func<string> StatusProvider { get; set; } = () => "Ok";

        /// <summary>
        /// <para>An optional delegate that returns application commit hash.</para>
        /// <para>By default, commit hash is extracted from <see cref="AssemblyTitleAttribute"/> of the entry assembly.</para>
        /// </summary>
        [CanBeNull]
        public Func<string> CommitHashProvider { get; set; }
    }
}