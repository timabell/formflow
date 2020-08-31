using System;
using System.Threading.Tasks;
using FormFlow.Metadata;
using FormFlow.State;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.Extensions.Logging;

namespace FormFlow
{
    public class FormFlowInstanceLoader
    {
        private readonly IInstanceStateProvider _stateProvider;
        private readonly ILogger<FormFlowInstanceLoader> _logger;

        public FormFlowInstanceLoader(IInstanceStateProvider stateProvider, ILogger<FormFlowInstanceLoader> logger)
        {
            _stateProvider = stateProvider ?? throw new ArgumentNullException(nameof(stateProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<FormFlowInstance> Resolve(ActionContext actionContext)
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
                _logger.LogWarning("Cannot find flow metadata on action.");
                return null;
            }

            if (!FormFlowInstanceId.TryResolve(
                actionContext.HttpContext.Request,
                actionContext.RouteData,
                flowDescriptor,
                out var instanceId))
            {
                _logger.LogWarning(
                    "Failed to extract ID from request.\n" +
                    "  Key: '{FlowKey}'",
                    flowDescriptor.Key);
                return null;
            }

            var instance = await _stateProvider.GetInstance(instanceId);
            if (instance == null)
            {
                _logger.LogWarning(
                    "Could not find instance in state store.\n" +
                    "  Key: '{FlowKey}'\n" +
                    "  Instance ID: '{InstanceId}'",
                    flowDescriptor.Key,
                    instanceId);
                return null;
            }

            if (instance.Key != flowDescriptor.Key)
            {
                _logger.LogWarning(
                    "Mismatched instance keys.\n" +
                    "  Key: '{FlowKey}'\n" +
                    "  Instance ID: '{InstanceId}'\n",
                    "  Persisted instance key: '{PersistedInstanceId}'",
                    flowDescriptor.Key,
                    instanceId,
                    instance.Key);
                return null;
            }

            if (instance.StateType != flowDescriptor.StateType)
            {
                _logger.LogWarning(
                    "Mismatched state types.\n" +
                    "  Key: '{FlowKey}'\n" +
                    "  Instance ID: '{InstanceId}'\n",
                    "  State type: '{StateType}'\n" +
                    "  Persisted instance type: '{PersistedStateType}'",
                    flowDescriptor.Key,
                    instanceId,
                    flowDescriptor.StateType,
                    instance.StateType);
                return null;
            }

            actionContext.HttpContext.Features.Set(new FormFlowInstanceFeature(instance));

            return instance;
        }
    }
}
