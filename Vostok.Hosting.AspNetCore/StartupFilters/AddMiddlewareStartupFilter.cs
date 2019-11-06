using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace Vostok.Hosting.AspNetCore.StartupFilters
{
    internal class AddMiddlewareStartupFilter<T> : IStartupFilter
    {
        private readonly T middleware;

        public AddMiddlewareStartupFilter(T middleware)
        {
            this.middleware = middleware;
        }

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return app =>
            {
                app.UseMiddleware<T>();
                next(app);
            };
        }
    }
}