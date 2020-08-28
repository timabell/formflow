using FormFlow.State;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;

namespace FormFlow.ModelBinding
{
    public class InstanceModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context.Metadata.ModelType == typeof(FormFlowInstance) ||
                (context.Metadata.ModelType.IsGenericType && context.Metadata.ModelType.GetGenericTypeDefinition() == typeof(Instance<>)))
            {
                var stateProvider = context.Services.GetRequiredService<IInstanceStateProvider>();
                return new InstanceModelBinder(stateProvider);
            }

            return null;
        }
    }
}
