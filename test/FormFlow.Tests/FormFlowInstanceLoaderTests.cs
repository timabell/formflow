using System.Collections.Generic;
using FormFlow.Metadata;
using FormFlow.State;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace FormFlow.Tests
{
    public class FormFlowInstanceLoaderTests
    {
        [Fact]
        public void Resolve_ActionDescriptorHasNoFormFlowActionDescriptor_ReturnsNull()
        {
            // Arrange
            var stateProvider = new Mock<IInstanceStateProvider>();

            var instanceResolver = new FormFlowInstanceLoader(
                stateProvider.Object,
                NullLogger<FormFlowInstanceLoader>.Instance);

            var httpContext = new DefaultHttpContext();
            var routeData = new RouteData();
            var actionDescriptor = new ActionDescriptor();

            var actionContext = new ActionContext(httpContext, routeData, actionDescriptor);

            // Act
            var result = instanceResolver.Resolve(actionContext);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Resolve_CannotExtractId_ReturnsNull()
        {
            // Arrange
            var key = "test-flow";
            var instanceId = new FormFlowInstanceId("the-instance", new RouteValueDictionary());
            var stateType = typeof(TestState);
            var state = new TestState();

            var stateProvider = new Mock<IInstanceStateProvider>();
            stateProvider
                .Setup(s => s.GetInstance(instanceId))
                .Returns(FormFlowInstance.Create(stateProvider.Object, key, instanceId, stateType, state, properties: new Dictionary<object, object>()));

            var instanceResolver = new FormFlowInstanceLoader(
                stateProvider.Object,
                NullLogger<FormFlowInstanceLoader>.Instance);

            var httpContext = new DefaultHttpContext();
            httpContext.Request.QueryString = new QueryString($"?");

            var routeData = new RouteData();

            var actionDescriptor = new ActionDescriptor();
            actionDescriptor.SetProperty(new FormFlowDescriptor(key, stateType, IdGenerationSource.RandomId));

            var actionContext = new ActionContext(httpContext, routeData, actionDescriptor);

            // Act
            var result = instanceResolver.Resolve(actionContext);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Resolve_InstanceDoesNotExistInStateStore_ReturnsNull()
        {
            // Arrange
            var key = "test-flow";
            var instanceId = "the-instance";
            var stateType = typeof(TestState);

            var stateProvider = new Mock<IInstanceStateProvider>();

            var instanceResolver = new FormFlowInstanceLoader(
                stateProvider.Object,
                NullLogger<FormFlowInstanceLoader>.Instance);

            var httpContext = new DefaultHttpContext();
            httpContext.Request.QueryString = new QueryString($"?ffiid={instanceId}");

            var routeData = new RouteData(new RouteValueDictionary()
            {
                { "ffiid", instanceId }
            });

            var actionDescriptor = new ActionDescriptor();
            actionDescriptor.SetProperty(new FormFlowDescriptor(key, stateType, IdGenerationSource.RandomId));

            var actionContext = new ActionContext(httpContext, routeData, actionDescriptor);

            // Act
            var result = instanceResolver.Resolve(actionContext);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Resolve_MismatchingKeys_ReturnsNull()
        {
            // Arrange
            var key = "test-flow";
            var instanceId = new FormFlowInstanceId("the-instance", new RouteValueDictionary());
            var stateType = typeof(TestState);
            var state = new TestState();

            var stateProvider = new Mock<IInstanceStateProvider>();
            stateProvider
                .Setup(s => s.GetInstance(instanceId))
                .Returns(FormFlowInstance.Create(stateProvider.Object, key, instanceId, stateType, state, properties: new Dictionary<object, object>()));

            var instanceResolver = new FormFlowInstanceLoader(
                stateProvider.Object,
                NullLogger<FormFlowInstanceLoader>.Instance);

            var httpContext = new DefaultHttpContext();
            httpContext.Request.QueryString = new QueryString($"?ffiid={instanceId}");

            var routeData = new RouteData(new RouteValueDictionary()
            {
                { "ffiid", instanceId }
            });

            var actionDescriptor = new ActionDescriptor();
            actionDescriptor.SetProperty(new FormFlowDescriptor("another-key", stateType, IdGenerationSource.RandomId));

            var actionContext = new ActionContext(httpContext, routeData, actionDescriptor);

            // Act
            var result = instanceResolver.Resolve(actionContext);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Resolve_MismatchingStateType_ReturnsNull()
        {
            // Arrange
            var key = "test-flow";
            var instanceId = new FormFlowInstanceId("the-instance", new RouteValueDictionary());
            var stateType = typeof(TestState);
            var state = new TestState();

            var stateProvider = new Mock<IInstanceStateProvider>();
            stateProvider
                .Setup(s => s.GetInstance(instanceId))
                .Returns(FormFlowInstance.Create(stateProvider.Object, key, instanceId, stateType, state, properties: new Dictionary<object, object>()));

            var instanceResolver = new FormFlowInstanceLoader(
                stateProvider.Object,
                NullLogger<FormFlowInstanceLoader>.Instance);

            var httpContext = new DefaultHttpContext();
            httpContext.Request.QueryString = new QueryString($"?ffiid={instanceId}");

            var routeData = new RouteData(new RouteValueDictionary()
            {
                { "ffiid", instanceId }
            });

            var actionDescriptor = new ActionDescriptor();
            actionDescriptor.SetProperty(new FormFlowDescriptor(key, typeof(AnotherTestState), IdGenerationSource.RandomId));

            var actionContext = new ActionContext(httpContext, routeData, actionDescriptor);

            // Act
            var result = instanceResolver.Resolve(actionContext);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Resolve_ValidRequest_ReturnsInstanceAndAddsFeature()
        {
            // Arrange
            var key = "test-flow";
            var instanceId = new FormFlowInstanceId("the-instance", new RouteValueDictionary());
            var stateType = typeof(TestState);
            var state = new TestState();
            
            var stateProvider = new Mock<IInstanceStateProvider>();
            stateProvider
                .Setup(s => s.GetInstance(instanceId))
                .Returns(FormFlowInstance.Create(stateProvider.Object, key, instanceId, stateType, state, properties: new Dictionary<object, object>()));

            var instanceResolver = new FormFlowInstanceLoader(
                stateProvider.Object,
                NullLogger<FormFlowInstanceLoader>.Instance);

            var httpContext = new DefaultHttpContext();
            httpContext.Request.QueryString = new QueryString($"?ffiid={instanceId}");

            var routeData = new RouteData(new RouteValueDictionary()
            {
                { "ffiid", instanceId }
            });

            var actionDescriptor = new ActionDescriptor();
            actionDescriptor.SetProperty(new FormFlowDescriptor(key, stateType, IdGenerationSource.RandomId));

            var actionContext = new ActionContext(httpContext, routeData, actionDescriptor);

            // Act
            var result = instanceResolver.Resolve(actionContext);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(key, result.Key);
            Assert.Equal(instanceId, result.InstanceId);
            Assert.Equal(stateType, result.StateType);

            var feature = httpContext.Features.Get<FormFlowInstanceFeature>();
            Assert.NotNull(feature);
            Assert.Equal(key, feature.Instance.Key);
            Assert.Equal(instanceId, feature.Instance.InstanceId);
            Assert.Equal(stateType, feature.Instance.StateType);
        }

        private class TestState { }

        private class AnotherTestState { }
    }
}
