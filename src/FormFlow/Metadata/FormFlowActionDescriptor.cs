using System;

namespace FormFlow.Metadata
{
    public class FormFlowActionDescriptor
    {
        public FormFlowActionDescriptor(string key, Type stateType)
        {
            Key = key ?? throw new ArgumentNullException(nameof(key));
            StateType = stateType ?? throw new ArgumentNullException(nameof(stateType));
        }

        public string Key { get; }
        public Type StateType { get; }
    }
}
