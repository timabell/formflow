using System;
using System.Collections.Generic;

namespace FormFlow
{
    public class Instance
    {
        private protected Instance(
            string key,
            string instanceId,
            Type stateType,
            object state,
            IReadOnlyDictionary<object, object> properties)
        {
            Key = key ?? throw new ArgumentNullException(nameof(key));
            StateType = stateType ?? throw new ArgumentNullException(nameof(stateType));
            InstanceId = instanceId ?? throw new ArgumentNullException(nameof(instanceId));
            Properties = properties ?? throw new ArgumentNullException(nameof(properties));
            State = state ?? throw new ArgumentNullException(nameof(state));
        }

        public string Key { get; }

        public string InstanceId { get; }

        public IReadOnlyDictionary<object, object> Properties { get; }

        public object State { get; }

        public Type StateType { get; set; }

        public static Instance Create(
            string key,
            string instanceId,
            Type stateType,
            object state,
            IReadOnlyDictionary<object, object> properties)
        {
            var genericType = typeof(Instance<>).MakeGenericType(stateType);
            return (Instance)Activator.CreateInstance(genericType, key, instanceId, state, properties);
        }
    }

    public sealed class Instance<TState> : Instance
    {
        public Instance(
            string key,
            string instanceId,
            TState state,
            IReadOnlyDictionary<object, object> properties)
            : base(key, instanceId, typeof(TState), state, properties)
        {
        }

        public new TState State => (TState)base.State;
    }
}
