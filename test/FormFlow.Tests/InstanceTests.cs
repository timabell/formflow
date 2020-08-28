using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FormFlow.State;
using Moq;
using Xunit;

namespace FormFlow.Tests
{
    public class InstanceTests
    {
        [Fact]
        public async Task Delete_CallsDeleteOnStateProvider()
        {
            // Arrange
            var instanceId = new InstanceId("instance", new Microsoft.AspNetCore.Routing.RouteValueDictionary());

            var stateProvider = new Mock<IInstanceStateProvider>();

            var instance = (Instance<MyState>)Instance.Create(
                stateProvider.Object,
                "key",
                instanceId,
                typeof(MyState),
                new MyState(),
                properties: new Dictionary<object, object>());

            var newState = new MyState();

            // Act
            await instance.Delete();

            // Assert
            stateProvider.Verify(mock => mock.DeleteInstance(instanceId));
        }

        [Fact]
        public async Task UpdateState_DeletedInstance_ThrowsInvalidOperationException()
        {
            // Arrange
            var instanceId = new InstanceId("instance", new Microsoft.AspNetCore.Routing.RouteValueDictionary());

            var stateProvider = new Mock<IInstanceStateProvider>();

            var instance = (Instance<MyState>)Instance.Create(
                stateProvider.Object,
                "key",
                instanceId,
                typeof(MyState),
                new MyState(),
                properties: new Dictionary<object, object>());

            var newState = new MyState();

            await instance.Delete();

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => instance.UpdateState(newState));
        }

        [Fact]
        public async Task UpdateState_CallsUpdateStateOnStateProvider()
        {
            // Arrange
            var instanceId = new InstanceId("instance", new Microsoft.AspNetCore.Routing.RouteValueDictionary());

            var stateProvider = new Mock<IInstanceStateProvider>();

            var instance = (Instance<MyState>)Instance.Create(
                stateProvider.Object,
                "key",
                instanceId,
                typeof(MyState),
                new MyState(),
                properties: new Dictionary<object, object>());

            var newState = new MyState();

            // Act
            await instance.UpdateState(newState);

            // Assert
            stateProvider.Verify(mock => mock.UpdateInstanceState(instanceId, newState));
            Assert.Same(newState, instance.State);
        }

        public class MyState { }
    }
}
