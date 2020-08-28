using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

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

        public Task<FormFlowInstance> CreateInstance(
            string key,
            FormFlowInstanceId instanceId,
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

            var instance = FormFlowInstance.Create(this, key, instanceId, stateType, state, properties);
            return Task.FromResult(instance);
        }

        public Task DeleteInstance(FormFlowInstanceId instanceId)
        {
            var session = _httpContextAccessor.HttpContext.Session;
            var sessionKey = GetSessionKeyForInstance(instanceId);

            session.Remove(sessionKey);

            return Task.CompletedTask;
        }

        public Task<FormFlowInstance> GetInstance(FormFlowInstanceId instanceId)
        {
            var session = _httpContextAccessor.HttpContext.Session;
            var sessionKey = GetSessionKeyForInstance(instanceId);
            
            if (session.TryGetValue(sessionKey, out var serialized))
            {
                var entry = (SessionEntry)_stateSerializer.Deserialize(serialized);

                var stateType = Type.GetType(entry.StateTypeAssemblyQualifiedName);

                var instance = FormFlowInstance.Create(
                    this,
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

        public Task UpdateInstanceState(FormFlowInstanceId instanceId, object state)
        {
            var session = _httpContextAccessor.HttpContext.Session;
            var sessionKey = GetSessionKeyForInstance(instanceId);

            if (session.TryGetValue(sessionKey, out var serialized))
            {
                var entry = (SessionEntry)_stateSerializer.Deserialize(serialized);

                entry.State = state;

                var updateSerialized = _stateSerializer.Serialize(entry);
                session.Set(sessionKey, updateSerialized);
            }
            else
            {
                throw new ArgumentException("Instance does not exist.", nameof(instanceId));
            }

            return Task.CompletedTask;
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
