using System;
using JetBrains.Annotations;
using Vostok.Commons.Environment;

namespace Vostok.Hosting.AspNetCore.Configuration
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
        /// A delegate that returns application commit hash.
        /// </summary>
        [NotNull]
        public Func<string> CommitHashProvider { get; set; } = AssemblyCommitHashExtractor.ExtractFromEntryAssembly;
    }
}