using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace FormFlow.State
{
    public class SessionInstanceStateProvider : IInstanceStateProvider
    {
        private readonly IStateSerializer _stateSerializer;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SessionInstanceStateProvider(
            IStateSerializer stateSerializer,
            IHttpContextAccessor httpContextAccessor)
        {
            _stateSerializer = stateSerializer ?? throw new ArgumentNullException(nameof(stateSerializer));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public Task<Instance> CreateInstance(
            string key,
            InstanceId instanceId,
            Type stateType,
            object state,
            IReadOnlyDictionary<object, object> properties)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (stateType == null)
            {
                throw new ArgumentNullException(nameof(stateType));
            }

            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            properties ??= new Dictionary<object, object>();

            var entry = new SessionEntry()
            {
                Key = key,
                StateTypeAssemblyQualifiedName = stateType.AssemblyQualifiedName,
                State = state,
                Properties = properties
            };
            var serialized = _stateSerializer.Serialize(entry);

            var session = _httpContextAccessor.HttpContext.Session;
            var sessionKey = GetSessionKeyForInstance(instanceId);
            session.Set(sessionKey, serialized);

            var instance = Instance.Create(key, instanceId, stateType, state, properties);
            return Task.FromResult(instance);
        }

        public Task<Instance> GetInstance(InstanceId instanceId)
        {
            var session = _httpContextAccessor.HttpContext.Session;
            var sessionKey = GetSessionKeyForInstance(instanceId);
            
            if (session.TryGetValue(sessionKey, out var serialized))
            {
                var entry = (SessionEntry)_stateSerializer.Deserialize(serialized);

                var stateType = Type.GetType(entry.StateTypeAssemblyQualifiedName);

                var instance = Instance.Create(
                    entry.Key,
                    instanceId,
                    stateType,
                    entry.State,
                    entry.Properties);
                return Task.FromResult(instance);
            }
            else
            {
                return null;
            }
        }

        // TODO Make this configurable
        private static string GetSessionKeyForInstance(string instanceId) =>
            $"FormFlowState:{instanceId}";

        private class SessionEntry
        {
            public string Key { get; set; }
            public string StateTypeAssemblyQualifiedName { get; set; }
            public object State { get; set; }
            public IReadOnlyDictionary<object, object> Properties { get; set; }
        }
    }
}
