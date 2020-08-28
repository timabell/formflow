using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FormFlow.State;

namespace FormFlow
{
    public class FormFlowInstance
    {
        private readonly IInstanceStateProvider _stateProvider;
        private bool _isDeleted;

        private protected FormFlowInstance(
            IInstanceStateProvider stateProvider,
            string key,
            FormFlowInstanceId instanceId,
            Type stateType,
            object state,
            IReadOnlyDictionary<object, object> properties)
        {
            _stateProvider = stateProvider ?? throw new ArgumentNullException(nameof(stateProvider));
            Key = key ?? throw new ArgumentNullException(nameof(key));
            StateType = stateType ?? throw new ArgumentNullException(nameof(stateType));
            InstanceId = instanceId;
            Properties = properties ?? throw new ArgumentNullException(nameof(properties));
            State = state ?? throw new ArgumentNullException(nameof(state));
        }

        public string Key { get; }

        public FormFlowInstanceId InstanceId { get; }

        public IReadOnlyDictionary<object, object> Properties { get; }

        public object State { get; private set; }

        public Type StateType { get; }

        public static FormFlowInstance Create(
            IInstanceStateProvider stateProvider,
            string key,
            FormFlowInstanceId instanceId,
            Type stateType,
            object state,
            IReadOnlyDictionary<object, object> properties)
        {
            var genericType = typeof(Instance<>).MakeGenericType(stateType);
            return (FormFlowInstance)Activator.CreateInstance(genericType, stateProvider, key, instanceId, state, properties);
        }

        public async Task Delete()
        {
            if (_isDeleted)
            {
                return;
            }

            await _stateProvider.DeleteInstance(InstanceId);
            _isDeleted = true;
        }

        protected async Task UpdateState(object state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            if (state.GetType() != StateType)
            {
                throw new ArgumentException($"State must be type: '{StateType.FullName}'.", nameof(state));
            }

            if (_isDeleted)
            {
                throw new InvalidOperationException("Instance has been deleted.");
            }

            await _stateProvider.UpdateInstanceState(InstanceId, state);
            State = state;
        }
    }

    public sealed class Instance<TState> : FormFlowInstance
    {
        public Instance(
            IInstanceStateProvider stateProvider,
            string key,
            FormFlowInstanceId instanceId,
            TState state,
            IReadOnlyDictionary<object, object> properties)
            : base(stateProvider, key, instanceId, typeof(TState), state, properties)
        {
        }

        public new TState State => (TState)base.State;

        public Task UpdateState(TState state) => UpdateState((object)state);

        public Task UpdateState(Func<TState, TState> update)
        {
            if (update == null)
            {
                throw new ArgumentNullException(nameof(update));
            }

            var newState = update(State);
            return UpdateState(newState);
        }
    }
}
