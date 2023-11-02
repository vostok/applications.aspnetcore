using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Connections;
using Vostok.Applications.AspNetCore.Middlewares;

namespace Vostok.Applications.AspNetCore.Configuration
{
    /// <summary>
    /// Represents configuration of <see cref="UnhandledExceptionMiddleware"/>.
    /// </summary>
    [PublicAPI]
    public class UnhandledExceptionSettings
    {
        /// <summary>
        /// Error response code to be used when an unhandled exception is observed.
        /// </summary>
        public int ErrorResponseCode { get; set; } = 500;

        /// <summary>
        /// List of exceptions to be ignored
        /// </summary>
        public List<Type> ExceptionsToIgnore = new() {
            typeof(TaskCanceledException), 
            typeof(OperationCanceledException), 
            typeof(ConnectionResetException)};
    }
}