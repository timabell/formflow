using System;
using FormFlow.State;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace FormFlow
{
    public class FormFlowInstanceProvider
    {
        private readonly IUserInstanceStateProvider _stateProvider;
        private readonly IActionContextAccessor _actionContextAccessor;

        public FormFlowInstanceProvider(
            IUserInstanceStateProvider stateProvider,
            IActionContextAccessor actionContextAccessor)
        {
            _stateProvider = stateProvider ?? throw new ArgumentNullException(nameof(stateProvider));
            _actionContextAccessor = actionContextAccessor ?? throw new ArgumentNullException(nameof(actionContextAccessor));
        }

        public FormFlowInstance GetInstance()
        {
            var actionContext = _actionContextAccessor.ActionContext;
            if (actionContext == null)
            {
                throw new InvalidOperationException("No ActionContext set.");
            }

            var httpContext = actionContext.HttpContext;

            var feature = httpContext.Features.Get<FormFlowInstanceFeature>();
            if (feature != null)
            {
                return feature.Instance;
            }

            var instanceResolver = new InstanceResolver(_stateProvider);
            var instance = instanceResolver.Resolve(actionContext);

            if (instance != null)
            {
                httpContext.Features.Set(new FormFlowInstanceFeature(instance));
            }

            return instance;
        }

        public FormFlowInstance<T> GetInstance<T>()
        {
            return (FormFlowInstance<T>)GetInstance();
        }
    }
}
