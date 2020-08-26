using System;
using System.Collections.Generic;

namespace FormFlow
{
    public class Instance
    {
        public Instance(
            string key,
            string instanceId,
            Type stateType,
            object state,
            IReadOnlyDictionary<string, object> items)
        {
            Key = key ?? throw new ArgumentNullException(nameof(key));
            StateType = stateType ?? throw new ArgumentNullException(nameof(stateType));
            InstanceId = instanceId ?? throw new ArgumentNullException(nameof(instanceId));
            Items = items ?? throw new ArgumentNullException(nameof(items));
            State = state ?? throw new ArgumentNullException(nameof(state));
        }

        public string Key { get; }

        public string InstanceId { get; }

        public IReadOnlyDictionary<string, object> Items { get; }

        public object State { get; }

        public Type StateType { get; set; }
    }
}
