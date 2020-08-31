﻿using System;
using System.Collections.Generic;
using FormFlow.State;
using Moq;
using Xunit;

namespace FormFlow.Tests
{
    public class FormFlowInstanceTests
    {
        [Fact]
        public void Delete_CallsDeleteOnStateProvider()
        {
            // Arrange
            var instanceId = new FormFlowInstanceId("instance", new Microsoft.AspNetCore.Routing.RouteValueDictionary());

            var stateProvider = new Mock<IInstanceStateProvider>();

            var instance = (FormFlowInstance<MyState>)FormFlowInstance.Create(
                stateProvider.Object,
                "key",
                instanceId,
                typeof(MyState),
                new MyState(),
                properties: new Dictionary<object, object>());

            var newState = new MyState();

            // Act
            instance.Delete();

            // Assert
            stateProvider.Verify(mock => mock.DeleteInstance(instanceId));
        }

        [Fact]
        public void UpdateState_DeletedInstance_ThrowsInvalidOperationException()
        {
            // Arrange
            var instanceId = new FormFlowInstanceId("instance", new Microsoft.AspNetCore.Routing.RouteValueDictionary());

            var stateProvider = new Mock<IInstanceStateProvider>();

            var instance = (FormFlowInstance<MyState>)FormFlowInstance.Create(
                stateProvider.Object,
                "key",
                instanceId,
                typeof(MyState),
                new MyState(),
                properties: new Dictionary<object, object>());

            var newState = new MyState();

            instance.Delete();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => instance.UpdateState(newState));
        }

        [Fact]
        public void UpdateState_CallsUpdateStateOnStateProvider()
        {
            // Arrange
            var instanceId = new FormFlowInstanceId("instance", new Microsoft.AspNetCore.Routing.RouteValueDictionary());

            var stateProvider = new Mock<IInstanceStateProvider>();

            var instance = (FormFlowInstance<MyState>)FormFlowInstance.Create(
                stateProvider.Object,
                "key",
                instanceId,
                typeof(MyState),
                new MyState(),
                properties: new Dictionary<object, object>());

            var newState = new MyState();

            // Act
            instance.UpdateState(newState);

            // Assert
            stateProvider.Verify(mock => mock.UpdateInstanceState(instanceId, newState));
            Assert.Same(newState, instance.State);
        }

        public class MyState { }
    }
}
