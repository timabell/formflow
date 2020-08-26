using System;
using FormFlow.Metadata;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace FormFlow
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public sealed class FormFlowActionAttribute : Attribute, IActionModelConvention, IControllerModelConvention
    {
        public FormFlowActionAttribute(string key, Type stateType)
        {
            Key = key ?? throw new ArgumentNullException(nameof(key));
            StateType = stateType ?? throw new ArgumentNullException(nameof(stateType));
        }

        public string Key { get; }

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
            var descriptor = new FormFlowActionDescriptor(Key, StateType);

            action.Properties.Add(typeof(FormFlowActionDescriptor), descriptor);
        }
    }
}
