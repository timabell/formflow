using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FormFlow.State;

namespace FormFlow.Tests
{
    public class InMemoryInstanceStateProvider : IInstanceStateProvider
    {
        private readonly Dictionary<string, Entry> _instances;

        public InMemoryInstanceStateProvider()
        {
            _instances = new Dictionary<string, Entry>();
        }

        public void Clear() => _instances.Clear();

        public Task<Instance> CreateInstance(
            string key,
            InstanceId instanceId,
            Type stateType,
            object state,
            IReadOnlyDictionary<object, object> properties)
        {
            _instances.Add(instanceId, new Entry()
            {
                Key = key,
                StateType = stateType,
                State = state,
                Properties = properties
            });

            var instance = Instance.Create(
                this,
                key,
                instanceId,
                stateType,
                state,
                properties ?? new Dictionary<object, object>());

            return Task.FromResult(instance);
        }

        public Task DeleteInstance(InstanceId instanceId)
        {
            _instances.Remove(instanceId);

            return Task.CompletedTask;
        }

        public Task<Instance> GetInstance(InstanceId instanceId)
        {
            _instances.TryGetValue(instanceId, out var entry);

            var instance = entry != null ?
                Instance.Create(this, entry.Key, instanceId, entry.StateType, entry.State, entry.Properties) :
                null;

            return Task.FromResult(instance);
        }

        public Task UpdateInstanceState(InstanceId instanceId, object state)
        {
            _instances[instanceId].State = state;

            return Task.CompletedTask;
        }

        private class Entry
        {
            public string Key { get; set; }
            public IReadOnlyDictionary<object, object> Properties { get; set; }
            public object State { get; set; }
            public Type StateType { get; set; }
        }
    }
}
