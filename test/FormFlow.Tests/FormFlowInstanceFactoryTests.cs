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

            var stateProvider = new Mock<IUserInstanceStateProvider>();

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

            var feature = httpContext.Features.Get<FormFlowInstanceFeature>();
            Assert.NotNull(feature);
            Assert.Same(instance, feature.Instance);
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

        [Fact]
        public void GetOrCreateInstance_StateTypeDoesNotMatchDescriptor_ThrowsInvalidOperationException()
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

            var stateProvider = new Mock<IUserInstanceStateProvider>();

            var instanceFactory = new FormFlowInstanceFactory(flowDescriptor, actionContext, stateProvider.Object);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => instanceFactory.GetOrCreateInstance(() => new AnotherTestState()));
        }

        [Fact]
        public void GetOrCreateInstance_InstanceAlreadyExists_ReturnsExistingInstance()
        {
            // Arrange
            var key = "test-flow";
            var stateType = typeof(TestState);
            var state = new TestState();

            var flowDescriptor = new FormFlowDescriptor(key, stateType, IdGenerationSource.RandomId);

            var stateProvider = new InMemoryInstanceStateProvider();

            var instanceId = FormFlowInstanceId.GenerateForRandomId();

            var existingInstance = FormFlowInstance.Create(
                stateProvider,
                key,
                instanceId,
                stateType,
                state,
                new Dictionary<object, object>());

            var httpContext = new DefaultHttpContext();
            httpContext.Features.Set(new FormFlowInstanceFeature(existingInstance));

            var routeData = new RouteData();

            var actionDescriptor = new ActionDescriptor();
            actionDescriptor.SetProperty(flowDescriptor);

            var actionContext = new ActionContext(httpContext, routeData, actionDescriptor);

            var instanceFactory = new FormFlowInstanceFactory(flowDescriptor, actionContext, stateProvider);

            // Act
            var instance = instanceFactory.GetOrCreateInstance(() => new TestState());

            // Assert
            Assert.Same(existingInstance, instance);
        }

        [Fact]
        public void GetOrCreateInstance_InstanceDoesNotAlreadyExist_CreatesNewInstance()
        {
            // Arrange
            var key = "test-flow";
            var stateType = typeof(TestState);
            var state = new TestState();

            var flowDescriptor = new FormFlowDescriptor(key, stateType, IdGenerationSource.RandomId);

            var stateProvider = new InMemoryInstanceStateProvider();

            var httpContext = new DefaultHttpContext();

            var routeData = new RouteData();

            var actionDescriptor = new ActionDescriptor();
            actionDescriptor.SetProperty(flowDescriptor);

            var actionContext = new ActionContext(httpContext, routeData, actionDescriptor);

            var instanceFactory = new FormFlowInstanceFactory(flowDescriptor, actionContext, stateProvider);

            var newState = new TestState();

            // Act
            var instance = instanceFactory.GetOrCreateInstance(() => newState);

            // Assert
            Assert.NotNull(instance);
            Assert.Same(newState, instance.State);
        }

        [Fact]
        public async Task GetOrCreateInstanceAsync_StateTypeDoesNotMatchDescriptor_ThrowsInvalidOperationException()
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

            var stateProvider = new Mock<IUserInstanceStateProvider>();

            var instanceFactory = new FormFlowInstanceFactory(flowDescriptor, actionContext, stateProvider.Object);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => instanceFactory.GetOrCreateInstanceAsync(() => Task.FromResult(new AnotherTestState())));
        }

        [Fact]
        public async Task GetOrCreateInstanceAsync_InstanceAlreadyExists_ReturnsExistingInstance()
        {
            // Arrange
            var key = "test-flow";
            var stateType = typeof(TestState);
            var state = new TestState();

            var flowDescriptor = new FormFlowDescriptor(key, stateType, IdGenerationSource.RandomId);

            var stateProvider = new InMemoryInstanceStateProvider();

            var instanceId = FormFlowInstanceId.GenerateForRandomId();

            var existingInstance = FormFlowInstance.Create(
                stateProvider,
                key,
                instanceId,
                stateType,
                state,
                new Dictionary<object, object>());

            var httpContext = new DefaultHttpContext();
            httpContext.Features.Set(new FormFlowInstanceFeature(existingInstance));

            var routeData = new RouteData();

            var actionDescriptor = new ActionDescriptor();
            actionDescriptor.SetProperty(flowDescriptor);

            var actionContext = new ActionContext(httpContext, routeData, actionDescriptor);

            var instanceFactory = new FormFlowInstanceFactory(flowDescriptor, actionContext, stateProvider);

            // Act
            var instance = await instanceFactory.GetOrCreateInstanceAsync(() => Task.FromResult(new TestState()));

            // Assert
            Assert.Same(existingInstance, instance);
        }

        [Fact]
        public async Task GetOrCreateInstanceAsync_InstanceDoesNotAlreadyExist_CreatesNewInstance()
        {
            // Arrange
            var key = "test-flow";
            var stateType = typeof(TestState);
            var state = new TestState();

            var flowDescriptor = new FormFlowDescriptor(key, stateType, IdGenerationSource.RandomId);

            var stateProvider = new InMemoryInstanceStateProvider();

            var httpContext = new DefaultHttpContext();

            var routeData = new RouteData();

            var actionDescriptor = new ActionDescriptor();
            actionDescriptor.SetProperty(flowDescriptor);

            var actionContext = new ActionContext(httpContext, routeData, actionDescriptor);

            var instanceFactory = new FormFlowInstanceFactory(flowDescriptor, actionContext, stateProvider);

            var newState = new TestState();

            // Act
            var instance = await instanceFactory.GetOrCreateInstanceAsync(() => Task.FromResult(newState));

            // Assert
            Assert.NotNull(instance);
            Assert.Same(newState, instance.State);
        }

        private class TestState { }

        private class AnotherTestState { }
    }
}
