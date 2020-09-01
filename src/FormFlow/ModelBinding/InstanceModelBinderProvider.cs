using FormFlow.State;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;

namespace FormFlow.ModelBinding
{
    public class InstanceModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (FormFlowInstance.IsFormFlowInstanceType(context.Metadata.ModelType))
            {
                var stateProvider = context.Services.GetRequiredService<IUserInstanceStateProvider>();
                return new InstanceModelBinder(stateProvider);
            }

            return null;
        }
    }
}
