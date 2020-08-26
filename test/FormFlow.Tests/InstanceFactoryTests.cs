using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FormFlow.Metadata;
using FormFlow.State;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;
using Moq;
using Xunit;

namespace FormFlow.Tests
{
    public class InstanceFactoryTests
    {
        [Fact]
        public async Task CreateInstance_StateTypeDoesNotMatchDescriptor_ThrowsInvalidOperationException()
        {
            // Arrange
            var key = "test-flow";
            var stateType = typeof(TestState);
            var state = new TestState();

            var flowDescriptor = new FormFlowActionDescriptor(key, stateType);

            var httpContext = new DefaultHttpContext();

            var routeData = new RouteData();

            var actionDescriptor = new ActionDescriptor();
            actionDescriptor.SetProperty(flowDescriptor);

            var actionContext = new ActionContext(httpContext, routeData, actionDescriptor);

            var stateProvider = new Mock<IInstanceStateProvider>();

            var instanceFactory = new InstanceFactory(flowDescriptor, actionContext, stateProvider.Object);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => instanceFactory.CreateInstance(new AnotherTestState()));
        }

        [Fact]
        public async Task CreateInstance_CreatesInstanceWithStateProvider()
        {
            // Arrange
            var key = "test-flow";
            var stateType = typeof(TestState);

            var flowDescriptor = new FormFlowActionDescriptor(key, stateType);

            var httpContext = new DefaultHttpContext();

            var routeData = new RouteData();

            var actionDescriptor = new ActionDescriptor();
            actionDescriptor.SetProperty(flowDescriptor);

            var actionContext = new ActionContext(httpContext, routeData, actionDescriptor);

            var stateProvider = new InMemoryInstanceStateProvider();

            var instanceFactory = new InstanceFactory(flowDescriptor, actionContext, stateProvider);

            var state = new TestState();
            var properties = new Dictionary<object, object>()
            {
                { "foo", 42 },
                { "bar", "baz" }
            };

            // Act
            var instance = await instanceFactory.CreateInstance(state, properties);

            // Assert
            Assert.NotNull(instance);
            Assert.Equal(key, instance.Key);
            Assert.Equal(stateType, instance.StateType);
            Assert.Same(state, instance.State);
            Assert.Equal(2, instance.Properties.Count);
            Assert.Equal(42, instance.Properties["foo"]);
            Assert.Equal("baz", instance.Properties["bar"]);
        }

        private class TestState { }

        private class AnotherTestState { }
    }
}
