using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Vostok.Applications.AspNetCore.Configuration;

namespace Vostok.Applications.AspNetCore.Middlewares
{
    /// <summary>
    /// Handles a wide range of diagnostic info requests under <c>/_diagnostic</c> prefix.
    /// </summary>
    [PublicAPI]
    public class DiagnosticApiMiddleware
    {
        private readonly RequestDelegate next;
        private readonly DiagnosticApiSettings options;

        public DiagnosticApiMiddleware(
            [NotNull] RequestDelegate next,
            [NotNull] IOptions<DiagnosticApiSettings> options)
        {
            this.next = next ?? throw new ArgumentNullException(nameof(next));
            this.options = (options ?? throw new ArgumentNullException(nameof(options))).Value;
        }

        public Task InvokeAsync(HttpContext context)
        {
            // TODO(iloktionov): implement

            return next.Invoke(context);
        }
    }
}
