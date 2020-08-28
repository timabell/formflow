using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FormFlow.State
{
    public interface IInstanceStateProvider
    {
        Task<FormFlowInstance> CreateInstance(
            string key,
            FormFlowInstanceId instanceId,
            Type stateType,
            object state,
            IReadOnlyDictionary<object, object> properties);

        Task DeleteInstance(FormFlowInstanceId instanceId);

        Task<FormFlowInstance> GetInstance(FormFlowInstanceId instanceId);

        Task UpdateInstanceState(FormFlowInstanceId instanceId, object state);
    }
}
