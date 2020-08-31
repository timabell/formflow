using FormFlow.Metadata;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Xunit;

namespace FormFlow.Tests
{
    public class FormFlowInstanceIdTests
    {
        [Fact]
        public void TryResolve_RandomIdGenerationSourceMissingQueryParameter_ReturnsFalse()
        {
            // Arrange
            var flowDescriptor = new FormFlowDescriptor(
                key: "key",
                stateType: typeof(MyState),
                idGenerationSource: IdGenerationSource.RandomId);

            var httpContext = new DefaultHttpContext();

            var routeData = new RouteData();

            // Act
            var created = FormFlowInstanceId.TryResolve(flowDescriptor, httpContext.Request, routeData, out var instanceId);

            // Assert
            Assert.False(created);
        }

        [Fact]
        public void TryResolve_RandomIdGenerationSourceContainsQueryParameter_ReturnsTrue()
        {
            // Arrange
            var flowDescriptor = new FormFlowDescriptor(
                key: "key",
                stateType: typeof(MyState),
                idGenerationSource: IdGenerationSource.RandomId);

            var httpContext = new DefaultHttpContext();
            httpContext.Request.QueryString = new QueryString("?ffiid=some-id");

            var routeData = new RouteData();

            // Act
            var created = FormFlowInstanceId.TryResolve(flowDescriptor, httpContext.Request, routeData, out var instanceId);

            // Assert
            Assert.True(created);
            Assert.Equal("some-id", instanceId.ToString());
        }

        [Fact]
        public void TryResolve_RouteValuesGenerationSourceMissingRouteParameter_ReturnsFalse()
        {
            // Arrange
            var flowDescriptor = new FormFlowDescriptor(
                key: "key",
                stateType: typeof(MyState),
                idGenerationSource: IdGenerationSource.RouteValues,
                idRouteParameterNames: new[] { "id1", "id2" });

            var httpContext = new DefaultHttpContext();

            var routeData = new RouteData(new RouteValueDictionary()
            {
                { "id1", "foo" }
            });

            // Act
            var created = FormFlowInstanceId.TryResolve(flowDescriptor, httpContext.Request, routeData, out var instanceId);

            // Assert
            Assert.False(created);
        }

        [Fact]
        public void TryResolve_RouteValuesGenerationSourceContainsAllRouteParameters_ReturnsTrue()
        {
            // Arrange
            var flowDescriptor = new FormFlowDescriptor(
                key: "key",
                stateType: typeof(MyState),
                idGenerationSource: IdGenerationSource.RouteValues,
                idRouteParameterNames: new[] { "id1", "id2" });

            var httpContext = new DefaultHttpContext();

            var routeData = new RouteData(new RouteValueDictionary()
            {
                { "id1", "foo" },
                { "id2", "bar" }
            });

            // Act
            var created = FormFlowInstanceId.TryResolve(flowDescriptor, httpContext.Request, routeData, out var instanceId);

            // Assert
            Assert.True(created);
            Assert.Equal("key?id1=foo&id2=bar", instanceId.ToString());
        }

        private class MyState { }
    }
}
