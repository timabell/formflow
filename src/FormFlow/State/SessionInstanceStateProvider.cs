using System;
using System.Collections.Generic;
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

        public FormFlowInstance CreateInstance(
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
                Properties = properties,
                Completed = false
            };
            var serialized = _stateSerializer.Serialize(entry);

            var session = _httpContextAccessor.HttpContext.Session;
            var sessionKey = GetSessionKeyForInstance(instanceId);
            session.Set(sessionKey, serialized);

            return FormFlowInstance.Create(this, key, instanceId, stateType, state, properties);
        }

        public void CompleteInstance(FormFlowInstanceId instanceId)
        {
            var session = _httpContextAccessor.HttpContext.Session;
            var sessionKey = GetSessionKeyForInstance(instanceId);

            if (session.TryGetValue(sessionKey, out var serialized))
            {
                var entry = (SessionEntry)_stateSerializer.Deserialize(serialized);

                entry.Completed = true;

                var updateSerialized = _stateSerializer.Serialize(entry);
                session.Set(sessionKey, updateSerialized);
            }
            else
            {
                throw new ArgumentException("Instance does not exist.", nameof(instanceId));
            }
        }

        public FormFlowInstance GetInstance(FormFlowInstanceId instanceId)
        {
            var session = _httpContextAccessor.HttpContext.Session;
            var sessionKey = GetSessionKeyForInstance(instanceId);
            
            if (session.TryGetValue(sessionKey, out var serialized))
            {
                var entry = (SessionEntry)_stateSerializer.Deserialize(serialized);

                var stateType = Type.GetType(entry.StateTypeAssemblyQualifiedName);

                return FormFlowInstance.Create(
                    this,
                    entry.Key,
                    instanceId,
                    stateType,
                    entry.State,
                    entry.Properties,
                    entry.Completed);
            }
            else
            {
                return null;
            }
        }

        public void UpdateInstanceState(FormFlowInstanceId instanceId, object state)
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
            public bool Completed { get; set; }
        }
    }
}
