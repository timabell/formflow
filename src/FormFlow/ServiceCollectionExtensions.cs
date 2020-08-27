using System;
using Microsoft.Extensions.DependencyInjection;

namespace FormFlow
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddFormFlow(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddHttpContextAccessor();
            services.AddSingleton<InstanceProvider>();

            return services;
        }
    }
}
