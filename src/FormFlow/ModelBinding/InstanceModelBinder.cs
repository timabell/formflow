using System;
using System.Threading.Tasks;
using FormFlow.Metadata;
using FormFlow.State;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace FormFlow.ModelBinding
{
    public class InstanceModelBinder : IModelBinder
    {
        private readonly IInstanceStateProvider _stateProvider;

        public InstanceModelBinder(IInstanceStateProvider stateProvider)
        {
            _stateProvider = stateProvider ?? throw new ArgumentNullException(nameof(stateProvider));
        }

        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext.ModelType != typeof(FormFlowInstance) &&
                !(bindingContext.ModelType.IsGenericType && bindingContext.ModelType.GetGenericTypeDefinition() == typeof(Instance<>)))
            {
                return;
            }

            var flowDescriptor = bindingContext.ActionContext.ActionDescriptor.GetProperty<FormFlowDescriptor>();
            if (flowDescriptor == null)
            {
                return;
            }

            if (!FormFlowInstanceId.TryResolve(bindingContext.ActionContext, flowDescriptor, out var instanceId))
            {
                return;
            }

            var instance = await _stateProvider.GetInstance(instanceId);
            if (instance == null)
            {
                return;
            }

            bindingContext.Result = ModelBindingResult.Success(instance);
        }
    }
}
