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

        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (!FormFlowInstance.IsFormFlowInstanceType(bindingContext.ModelType))
            {
                return Task.CompletedTask;
            }

            var flowDescriptor = bindingContext.ActionContext.ActionDescriptor.GetProperty<FormFlowDescriptor>();
            if (flowDescriptor == null)
            {
                return Task.CompletedTask;
            }

            if (!FormFlowInstanceId.TryResolve(
                flowDescriptor,
                bindingContext.ActionContext.HttpContext.Request,
                bindingContext.ActionContext.RouteData,
                out var instanceId))
            {
                return Task.CompletedTask;
            }

            var instance = _stateProvider.GetInstance(instanceId);
            if (instance == null)
            {
                return Task.CompletedTask;
            }

            bindingContext.Result = ModelBindingResult.Success(instance);
            return Task.CompletedTask;
        }
    }
}
