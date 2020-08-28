using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FormFlow.State
{
    public interface IInstanceStateProvider
    {
        Task<Instance> CreateInstance(
            string key,
            InstanceId instanceId,
            Type stateType,
            object state,
            IReadOnlyDictionary<object, object> properties);

        Task DeleteInstance(InstanceId instanceId);

        Task<Instance> GetInstance(InstanceId instanceId);

        Task UpdateInstanceState(InstanceId instanceId, object state);
    }
}
