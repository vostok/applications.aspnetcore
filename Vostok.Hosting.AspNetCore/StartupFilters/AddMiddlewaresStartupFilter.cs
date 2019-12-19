using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace Vostok.Hosting.AspNetCore.StartupFilters
{
    internal class AddMiddlewaresStartupFilter : IStartupFilter
    {
        private readonly Type[] middlewaresTypes;

        public AddMiddlewaresStartupFilter(Type[] middlewaresTypes)
        {
            this.middlewaresTypes = middlewaresTypes;
        }

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return app =>
            {
                foreach (var type in middlewaresTypes)
                    app.UseMiddleware(type);

                next(app);
            };
        }
    }
}