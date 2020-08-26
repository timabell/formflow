using System;
using System.Threading.Tasks;
using FormFlow.Metadata;
using FormFlow.State;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace FormFlow.ModelBinding
{
    public class InstanceFactoryModelBinder : IModelBinder
    {
        private readonly IInstanceStateProvider _stateProvider;

        public InstanceFactoryModelBinder(IInstanceStateProvider stateProvider)
        {
            _stateProvider = stateProvider ?? throw new ArgumentNullException(nameof(stateProvider));
        }

        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext.ModelType != typeof(InstanceFactory))
            {
                return Task.CompletedTask;
            }

            var flowDescriptor = bindingContext.ActionContext.ActionDescriptor.GetProperty<FormFlowActionDescriptor>();
            if (flowDescriptor == null)
            {
                return Task.CompletedTask;
            }

            var instanceFactory = new InstanceFactory(flowDescriptor, bindingContext.ActionContext, _stateProvider);
            bindingContext.Result = ModelBindingResult.Success(instanceFactory);

            return Task.CompletedTask;
        }
    }
}
