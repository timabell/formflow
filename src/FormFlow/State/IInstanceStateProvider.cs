using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FormFlow.State
{
    public interface IInstanceStateProvider
    {
        Task<Instance> CreateInstance(
            string key,
            string instanceId,
            Type stateType,
            object state,
            IReadOnlyDictionary<object, object> properties);

        Task<Instance> GetInstance(string instanceId);
    }
}
