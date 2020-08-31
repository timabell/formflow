using System;
using FormFlow.State;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

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
            services.AddSingleton<FormFlowInstanceProvider>();
            services.TryAddSingleton<IInstanceStateProvider, SessionInstanceStateProvider>();

            return services;
        }

        public static IServiceCollection AddFormFlow(
            this IServiceCollection services,
            Action<FormFlowOptions> configure)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            services.Configure(configure);
            services.AddFormFlow();

            return services;
        }
    }
}
