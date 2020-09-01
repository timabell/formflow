using FormFlow.State;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;

namespace FormFlow.ModelBinding
{
    public class InstanceFactoryModelBinderProvider : IModelBinderProvider
    {
        public InstanceFactoryModelBinderProvider()
        {
        }

        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context.Metadata.ModelType == typeof(FormFlowInstanceFactory))
            {
                var stateProvider = context.Services.GetRequiredService<IUserInstanceStateProvider>();
                return new InstanceFactoryModelBinder(stateProvider);
            }

            return null;
        }
    }
}
