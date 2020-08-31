using System;
using FormFlow.Metadata;
using FormFlow.State;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;

namespace FormFlow
{
    internal class InstanceResolver
    {
        private readonly IInstanceStateProvider _stateProvider;

        public InstanceResolver(IInstanceStateProvider stateProvider)
        {
            _stateProvider = stateProvider ?? throw new ArgumentNullException(nameof(stateProvider));
        }

        public FormFlowInstance Resolve(ActionContext actionContext)
        {
            if (actionContext == null)
            {
                throw new ArgumentNullException(nameof(actionContext));
            }

            var feature = actionContext.HttpContext.Features.Get<FormFlowInstanceFeature>();
            if (feature != null)
            {
                return feature.Instance;
            }
            
            var flowDescriptor = actionContext.ActionDescriptor.GetProperty<FormFlowDescriptor>();
            if (flowDescriptor == null)
            {
                return null;
            }

            if (!FormFlowInstanceId.TryResolve(
                flowDescriptor,
                actionContext.HttpContext.Request,
                actionContext.RouteData,
                out var instanceId))
            {
                return null;
            }

            var instance = _stateProvider.GetInstance(instanceId);
            if (instance == null)
            {
                return null;
            }

            if (instance.Key != flowDescriptor.Key)
            {
                return null;
            }

            if (instance.StateType != flowDescriptor.StateType)
            {
                return null;
            }

            actionContext.HttpContext.Features.Set(new FormFlowInstanceFeature(instance));

            return instance;
        }
    }
}
