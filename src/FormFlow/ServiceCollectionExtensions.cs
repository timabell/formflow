using System;
using System.Linq;
using System.Reflection;
using FormFlow.Filters;
using FormFlow.ModelBinding;
using FormFlow.State;
using Microsoft.AspNetCore.Mvc;
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
            services.AddSingleton<FormFlowInstanceProvider>();
            services.AddSingleton<IStateSerializer, JsonStateSerializer>();
            services.AddSingleton<IUserInstanceStateProvider, UserInstanceStateProvider>();
            services.AddSingleton<IUserInstanceStateStore, SessionUserInstanceStateStore>();

            services.Configure<MvcOptions>(options =>
            {
                options.Filters.Add(new MissingInstanceActionFilter());

                options.ModelBinderProviders.Insert(0, new InstanceFactoryModelBinderProvider());
                options.ModelBinderProviders.Insert(0, new InstanceModelBinderProvider());
            });

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

        public static IServiceCollection AddFormFlowStateTypes(
            this IServiceCollection services,
            Assembly assembly)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            var stateTypes = assembly.GetTypes()
                .Where(t => t.IsPublic && !t.IsAbstract && t.GetCustomAttribute<FormFlowStateAttribute>() != null);

            foreach (var type in stateTypes)
            {
                var instanceType = typeof(FormFlowInstance<>).MakeGenericType(type);

                services.AddTransient(instanceType, sp =>
                {
                    var instanceProvider = sp.GetRequiredService<FormFlowInstanceProvider>();
                    return instanceProvider.GetInstance();
                });
            }

            return services;
        }

        public static IServiceCollection AddFormFlowStateTypes(
            this IServiceCollection services,
            Type fromAssemblyContainingType)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (fromAssemblyContainingType == null)
            {
                throw new ArgumentNullException(nameof(fromAssemblyContainingType));
            }

            return AddFormFlowStateTypes(services, fromAssemblyContainingType.Assembly);
        }
    }
}
