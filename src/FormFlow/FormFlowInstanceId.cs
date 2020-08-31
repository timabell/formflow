using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Encodings.Web;
using FormFlow.Metadata;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebUtilities;

namespace FormFlow
{
    public readonly struct FormFlowInstanceId : IEquatable<FormFlowInstanceId>
    {
        private readonly string _id;

        internal FormFlowInstanceId(string instanceId, RouteValueDictionary routeValues)
        {
            _id = instanceId ?? throw new ArgumentNullException(nameof(instanceId));
            RouteValues = routeValues ?? throw new ArgumentNullException(nameof(routeValues));
        }

        public IReadOnlyDictionary<string, object> RouteValues { get; }

        public static FormFlowInstanceId Generate(
            HttpRequest httpRequest,
            RouteData routeData,
            FormFlowDescriptor flowDescriptor)
        {
            if (httpRequest == null)
            {
                throw new ArgumentNullException(nameof(httpRequest));
            }

            if (routeData == null)
            {
                throw new ArgumentNullException(nameof(routeData));
            }

            if (flowDescriptor == null)
            {
                throw new ArgumentNullException(nameof(flowDescriptor));
            }

            if (flowDescriptor.IdGenerationSource == IdGenerationSource.RandomId)
            {
                return GenerateForRandomId();
            }
            else if (flowDescriptor.IdGenerationSource == IdGenerationSource.RouteValues)
            {
                return GenerateForRouteValues(httpRequest, routeData, flowDescriptor);
            }
            else
            {
                throw new NotSupportedException($"Unknown IdGenerationSource: '{flowDescriptor.IdGenerationSource}'.");
            }
        }

        public static bool TryResolve(
            HttpRequest httpRequest,
            RouteData routeData,
            FormFlowDescriptor flowDescriptor,
            out FormFlowInstanceId instanceId)
        {
            if (httpRequest == null)
            {
                throw new ArgumentNullException(nameof(httpRequest));
            }

            if (routeData == null)
            {
                throw new ArgumentNullException(nameof(routeData));
            }

            if (flowDescriptor == null)
            {
                throw new ArgumentNullException(nameof(flowDescriptor));
            }

            if (flowDescriptor.IdGenerationSource == IdGenerationSource.RandomId)
            {
                return TryResolveForRandomId(httpRequest, out instanceId);
            }
            else if (flowDescriptor.IdGenerationSource == IdGenerationSource.RouteValues)
            {
                return TryResolveForRouteValues(flowDescriptor, routeData, out instanceId);
            }
            else
            {
                throw new NotSupportedException($"Unknown IdGenerationSource: '{flowDescriptor.IdGenerationSource}'.");
            }
        }

        public bool Equals([AllowNull] FormFlowInstanceId other) => _id == other._id;

        public override bool Equals(object obj) => obj is FormFlowInstanceId x && x.Equals(this);

        public override int GetHashCode() => _id.GetHashCode();

        public override string ToString() => _id;

        public static bool operator ==(FormFlowInstanceId left, FormFlowInstanceId right) => left.Equals(right);

        public static bool operator !=(FormFlowInstanceId left, FormFlowInstanceId right) => !(left == right);

        public static implicit operator string(FormFlowInstanceId instanceId) => instanceId.ToString();

        internal static FormFlowInstanceId GenerateForRandomId()
        {
            // Always generate a new ID, even if incoming routeValues have an existing one
            var id = Guid.NewGuid().ToString();

            return new FormFlowInstanceId(
                id,
                new RouteValueDictionary()
                {
                        { Constants.InstanceIdQueryParameterName, id }
                });
        }

        internal static FormFlowInstanceId GenerateForRouteValues(
            HttpRequest httpRequest,
            RouteData routeData,
            FormFlowDescriptor flowDescriptor)
        {
            if (!TryResolve(httpRequest, routeData, flowDescriptor, out var instanceId))
            {
                var routeParameterNameList = string.Join(", ", flowDescriptor.IdRouteParameterNames);
                throw new InvalidOperationException(
                    $"One or more route parameters are missing. Flow requires: {routeParameterNameList}.");
            }

            return instanceId;
        }

        internal static bool TryResolveForRandomId(
            HttpRequest httpRequest,
            out FormFlowInstanceId instanceId)
        {
            if (httpRequest == null)
            {
                throw new ArgumentNullException(nameof(httpRequest));
            }

            var id = httpRequest.HttpContext.Request.Query[Constants.InstanceIdQueryParameterName].ToString();

            if (string.IsNullOrEmpty(id))
            {
                instanceId = default;
                return false;
            }

            instanceId = new FormFlowInstanceId(
                id,
                new RouteValueDictionary()
                {
                    { Constants.InstanceIdQueryParameterName, id }
                });

            return true;
        }

        internal static bool TryResolveForRouteValues(
            FormFlowDescriptor flowDescriptor,
            RouteData routeData,
            out FormFlowInstanceId instanceId)
        {
            if (flowDescriptor == null)
            {
                throw new ArgumentNullException(nameof(flowDescriptor));
            }

            if (routeData == null)
            {
                throw new ArgumentNullException(nameof(routeData));
            }

            var urlEncoder = UrlEncoder.Default;

            var routeValues = routeData.Values;

            var id = urlEncoder.Encode(flowDescriptor.Key);
            var instanceRouteValues = new RouteValueDictionary();

            foreach (var routeParam in flowDescriptor.IdRouteParameterNames)
            {
                var routeValue = routeValues[routeParam]?.ToString();

                if (string.IsNullOrEmpty(routeValue))
                {
                    instanceId = default;
                    return false;
                }

                id = QueryHelpers.AddQueryString(id, routeParam, routeValue);
                instanceRouteValues.Add(routeParam, routeValue);
            }

            instanceId = new FormFlowInstanceId(id, instanceRouteValues);
            return true;
        }
    }
}
