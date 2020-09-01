using System;
using System.Threading.Tasks;
using FormFlow.State;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace FormFlow.ModelBinding
{
    public class InstanceModelBinder : IModelBinder
    {
        private readonly IUserInstanceStateProvider _stateProvider;

        public InstanceModelBinder(IUserInstanceStateProvider stateProvider)
        {
            _stateProvider = stateProvider ?? throw new ArgumentNullException(nameof(stateProvider));
        }

        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (!FormFlowInstance.IsFormFlowInstanceType(bindingContext.ModelType))
            {
                return Task.CompletedTask;
            }

            var resolver = new InstanceResolver(_stateProvider);
            var instance = resolver.Resolve(bindingContext.ActionContext);

            if (instance != null)
            {
                bindingContext.Result = ModelBindingResult.Success(instance);
            }

            return Task.CompletedTask;
        }
    }
}
