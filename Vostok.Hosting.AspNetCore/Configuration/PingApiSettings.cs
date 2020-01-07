using System;
using JetBrains.Annotations;
using Vostok.Commons.Environment;
using Vostok.Hosting.AspNetCore.Middlewares;

namespace Vostok.Hosting.AspNetCore.Configuration
{
    /// <summary>
    /// Configuration of the <see cref="PingApiMiddleware"/>.
    /// </summary>
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