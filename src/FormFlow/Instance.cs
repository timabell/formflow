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

        public static Instance Create(
            string key,
            string instanceId,
            Type stateType,
            object state,
            IReadOnlyDictionary<string, object> items)
        {
            var genericType = typeof(Instance<>).MakeGenericType(stateType);
            return (Instance)Activator.CreateInstance(genericType, key, instanceId, state, items);
        }
    }

    public sealed class Instance<TState> : Instance
    {
        public Instance(
            string key,
            string instanceId,
            TState state,
            IReadOnlyDictionary<string, object> items)
            : base(key, instanceId, typeof(TState), state, items)
        {
        }

        public new object State => (TState)base.State;
    }
}
