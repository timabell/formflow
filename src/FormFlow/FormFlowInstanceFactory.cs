using System;
using System.Collections.Generic;
using FormFlow.Metadata;
using FormFlow.State;
using Microsoft.AspNetCore.Mvc;

namespace FormFlow
{
    public class FormFlowInstanceFactory
    {
        private readonly FormFlowDescriptor _flowDescriptor;
        private readonly ActionContext _actionContext;
        private readonly IInstanceStateProvider _stateProvider;

        public FormFlowInstanceFactory(
            FormFlowDescriptor flowDescriptor,
            ActionContext actionContext,
            IInstanceStateProvider stateProvider)
        {
            _flowDescriptor = flowDescriptor ?? throw new ArgumentNullException(nameof(flowDescriptor));
            _actionContext = actionContext ?? throw new ArgumentNullException(nameof(actionContext));
            _stateProvider = stateProvider ?? throw new ArgumentNullException(nameof(stateProvider));
        }
        
        public FormFlowInstance<TState> CreateInstance<TState>(
            TState state,
            IReadOnlyDictionary<object, object> properties = null)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            if (typeof(TState) != _flowDescriptor.StateType)
            {
                throw new InvalidOperationException(
                    $"{typeof(TState).Name} is not compatible with {_flowDescriptor.StateType.Name}.");
            }

            var instanceId = FormFlowInstanceId.Generate(
                _actionContext.HttpContext.Request,
                _actionContext.RouteData,
                _flowDescriptor);

            return (FormFlowInstance<TState>)_stateProvider.CreateInstance(
                _flowDescriptor.Key,
                instanceId,
                _flowDescriptor.StateType,
                state, properties);
        }
    }
}
