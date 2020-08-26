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
        private readonly IdResolver _idResolver = new IdResolver();

        public InstanceModelBinder(IInstanceStateProvider stateProvider)
        {
            _stateProvider = stateProvider ?? throw new ArgumentNullException(nameof(stateProvider));
        }

        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext.ModelType != typeof(Instance) &&
                !(bindingContext.ModelType.IsGenericType && bindingContext.ModelType.GetGenericTypeDefinition() == typeof(Instance<>)))
            {
                return;
            }

            var flowDescriptor = bindingContext.ActionContext.ActionDescriptor.GetProperty<FormFlowActionDescriptor>();
            if (flowDescriptor == null)
            {
                return;
            }

            var instanceId = _idResolver.ResolveId(bindingContext.ActionContext, flowDescriptor);
            if (string.IsNullOrEmpty(instanceId))
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
