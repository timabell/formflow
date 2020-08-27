using System;
using System.Collections.Generic;
using FormFlow.Metadata;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace FormFlow
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public sealed class FormFlowActionAttribute : Attribute, IActionModelConvention, IControllerModelConvention
    {
        public FormFlowActionAttribute(string key, Type stateType, params string[] idRouteParameterNames)
        {
            Key = key ?? throw new ArgumentNullException(nameof(key));
            StateType = stateType ?? throw new ArgumentNullException(nameof(stateType));
            IdGenerationSource = idRouteParameterNames.Length == 0 ? IdGenerationSource.RandomId : IdGenerationSource.RouteValues;
            IdRouteParameterNames = idRouteParameterNames;
        }

        public string Key { get; }

        public IdGenerationSource IdGenerationSource { get; }

        public IReadOnlyCollection<string> IdRouteParameterNames { get; }

        public Type StateType { get; }

        void IActionModelConvention.Apply(ActionModel action)
        {
            AddMetadataToAction(action);
        }

        void IControllerModelConvention.Apply(ControllerModel controller)
        {
            foreach (var action in controller.Actions)
            {
                AddMetadataToAction(action);
            }
        }

        private void AddMetadataToAction(ActionModel action)
        {
            var descriptor = new FormFlowActionDescriptor(Key, StateType, IdGenerationSource, IdRouteParameterNames);

            action.Properties.Add(typeof(FormFlowActionDescriptor), descriptor);
        }
    }
}
