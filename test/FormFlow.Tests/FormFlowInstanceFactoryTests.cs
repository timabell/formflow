using System;
using System.Collections.Generic;
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
    public class FormFlowInstanceFactoryTests
    {
        [Fact]
        public void CreateInstance_StateTypeDoesNotMatchDescriptor_ThrowsInvalidOperationException()
        {
            // Arrange
            var key = "test-flow";
            var stateType = typeof(TestState);
            var state = new TestState();

            var flowDescriptor = new FormFlowDescriptor(key, stateType, IdGenerationSource.RandomId);

            var httpContext = new DefaultHttpContext();

            var routeData = new RouteData();

            var actionDescriptor = new ActionDescriptor();
            actionDescriptor.SetProperty(flowDescriptor);

            var actionContext = new ActionContext(httpContext, routeData, actionDescriptor);

            var stateProvider = new Mock<IInstanceStateProvider>();

            var instanceFactory = new FormFlowInstanceFactory(flowDescriptor, actionContext, stateProvider.Object);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => instanceFactory.CreateInstance(new AnotherTestState()));
        }

        [Fact]
        public void CreateInstance_CreatesInstanceWithStateProvider()
        {
            // Arrange
            var key = "test-flow";
            var stateType = typeof(TestState);

            var flowDescriptor = new FormFlowDescriptor(key, stateType, IdGenerationSource.RandomId);

            var httpContext = new DefaultHttpContext();

            var routeData = new RouteData();

            var actionDescriptor = new ActionDescriptor();
            actionDescriptor.SetProperty(flowDescriptor);

            var actionContext = new ActionContext(httpContext, routeData, actionDescriptor);

            var stateProvider = new InMemoryInstanceStateProvider();

            var instanceFactory = new FormFlowInstanceFactory(flowDescriptor, actionContext, stateProvider);

            var state = new TestState();
            var properties = new Dictionary<object, object>()
            {
                { "foo", 42 },
                { "bar", "baz" }
            };

            // Act
            var instance = instanceFactory.CreateInstance(state, properties);

            // Assert
            Assert.NotNull(instance);
            Assert.Equal(key, instance.Key);
            Assert.Equal(stateType, instance.StateType);
            Assert.Same(state, instance.State);
            Assert.Equal(2, instance.Properties.Count);
            Assert.Equal(42, instance.Properties["foo"]);
            Assert.Equal("baz", instance.Properties["bar"]);
        }

        [Fact]
        public void CreateInstance_RouteValuesIdGenerationSource_CreatesInstanceWithGeneratedId()
        {
            // Arrange
            var key = "test-flow";
            var stateType = typeof(TestState);

            var flowDescriptor = new FormFlowDescriptor(
                key,
                stateType,
                IdGenerationSource.RouteValues,
                idRouteParameterNames: new[] { "id1", "id2" });

            var httpContext = new DefaultHttpContext();

            var routeData = new RouteData(new RouteValueDictionary()
            {
                { "id1", "foo" },
                { "id2", "bar" }
            });

            var actionDescriptor = new ActionDescriptor();
            actionDescriptor.SetProperty(flowDescriptor);

            var actionContext = new ActionContext(httpContext, routeData, actionDescriptor);

            var stateProvider = new InMemoryInstanceStateProvider();

            var instanceFactory = new FormFlowInstanceFactory(flowDescriptor, actionContext, stateProvider);

            var state = new TestState();

            // Act
            var instance = instanceFactory.CreateInstance(state);

            // Assert
            Assert.NotNull(instance);
            Assert.Equal("test-flow?id1=foo&id2=bar", instance.InstanceId);
        }

        private class TestState { }

        private class AnotherTestState { }
    }
}
