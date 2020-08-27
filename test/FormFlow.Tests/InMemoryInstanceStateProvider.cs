using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FormFlow.State;

namespace FormFlow.Tests
{
    public class InMemoryInstanceStateProvider : IInstanceStateProvider
    {
        private readonly Dictionary<string, Instance> _instances;

        public InMemoryInstanceStateProvider()
        {
            _instances = new Dictionary<string, Instance>();
        }

        public void Clear() => _instances.Clear();

        public Task<Instance> CreateInstance(
            string key,
            InstanceId instanceId,
            Type stateType,
            object state,
            IReadOnlyDictionary<object, object> properties)
        {
            var instance = Instance.Create(
                key,
                instanceId,
                stateType,
                state,
                properties ?? new Dictionary<object, object>());

            _instances.Add(instanceId, instance);

            return Task.FromResult(instance);
        }

        public Task<Instance> GetInstance(InstanceId instanceId)
        {
            _instances.TryGetValue(instanceId, out var instance);

            return Task.FromResult(instance);
        }
    }
}
