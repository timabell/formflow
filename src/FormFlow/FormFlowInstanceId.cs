using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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
            FormFlowDescriptor flowDescriptor,
            HttpRequest httpRequest,
            RouteData routeData)
        {
            if (flowDescriptor == null)
            {
                throw new ArgumentNullException(nameof(flowDescriptor));
            }

            if (httpRequest == null)
            {
                throw new ArgumentNullException(nameof(httpRequest));
            }

            if (routeData == null)
            {
                throw new ArgumentNullException(nameof(routeData));
            }

            if (flowDescriptor.IdGenerationSource == IdGenerationSource.RandomId)
            {
                return GenerateForRandomId();
            }
            else if (flowDescriptor.IdGenerationSource == IdGenerationSource.RouteValues)
            {
                return GenerateForRouteValues(flowDescriptor, httpRequest, routeData);
            }
            else
            {
                throw new NotSupportedException($"Unknown IdGenerationSource: '{flowDescriptor.IdGenerationSource}'.");
            }
        }

        public static FormFlowInstanceId GenerateForRandomId()
        {
            var id = Guid.NewGuid().ToString();

            return new FormFlowInstanceId(
                id,
                new RouteValueDictionary()
                {
                    { Constants.InstanceIdQueryParameterName, id }
                });
        }

        public static FormFlowInstanceId GenerateForRouteValues(
            string key,
            IDictionary<string, object> routeValues)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (routeValues == null)
            {
                throw new ArgumentNullException(nameof(routeValues));
            }

            if (routeValues.Count == 0)
            {
                throw new ArgumentException("At least one route value must be provided.", nameof(routeValues));
            }

            var id = GenerateIdForRouteValues(key, routeValues);

            return new FormFlowInstanceId(id, new RouteValueDictionary(routeValues));
        }

        public static FormFlowInstanceId GenerateForRouteValues(
            FormFlowDescriptor flowDescriptor,
            HttpRequest httpRequest,
            RouteData routeData)
        {
            if (flowDescriptor == null)
            {
                throw new ArgumentNullException(nameof(flowDescriptor));
            }

            if (httpRequest == null)
            {
                throw new ArgumentNullException(nameof(httpRequest));
            }

            if (routeData == null)
            {
                throw new ArgumentNullException(nameof(routeData));
            }

            if (flowDescriptor.IdGenerationSource != IdGenerationSource.RouteValues)
            {
                throw new ArgumentException(
                    $"{nameof(flowDescriptor)}'s {nameof(flowDescriptor.IdGenerationSource)} must be {IdGenerationSource.RouteValues}.");
            }

            if (!TryResolveForRouteValues(flowDescriptor, routeData, out var instanceId))
            {
                var routeParameterNameList = string.Join(", ", flowDescriptor.IdRouteParameterNames);
                throw new InvalidOperationException(
                    $"One or more route parameters are missing. Flow requires: {routeParameterNameList}.");
            }

            return instanceId;
        }

        public static bool TryResolve(
            FormFlowDescriptor flowDescriptor,
            HttpRequest httpRequest,
            RouteData routeData,
            out FormFlowInstanceId instanceId)
        {
            if (flowDescriptor == null)
            {
                throw new ArgumentNullException(nameof(flowDescriptor));
            }

            if (httpRequest == null)
            {
                throw new ArgumentNullException(nameof(httpRequest));
            }

            if (routeData == null)
            {
                throw new ArgumentNullException(nameof(routeData));
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

        public static bool TryResolveForRandomId(
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

        public static bool TryResolveForRouteValues(
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

            if (flowDescriptor.IdGenerationSource != IdGenerationSource.RouteValues)
            {
                throw new ArgumentException(
                    $"{nameof(flowDescriptor)}'s {nameof(flowDescriptor.IdGenerationSource)} must be {IdGenerationSource.RouteValues}.");
            }

            var routeValues = routeData.Values;

            var instanceRouteValues = new RouteValueDictionary();

            foreach (var routeParam in flowDescriptor.IdRouteParameterNames)
            {
                var routeValue = routeValues[routeParam]?.ToString();

                if (string.IsNullOrEmpty(routeValue))
                {
                    instanceId = default;
                    return false;
                }

                instanceRouteValues.Add(routeParam, routeValue);
            }

            var id = GenerateIdForRouteValues(flowDescriptor.Key, instanceRouteValues);

            instanceId = new FormFlowInstanceId(id, instanceRouteValues);
            return true;
        }

        public bool Equals([AllowNull] FormFlowInstanceId other) => _id == other._id;

        public override bool Equals(object obj) => obj is FormFlowInstanceId x && x.Equals(this);

        public override int GetHashCode() => _id.GetHashCode();

        public override string ToString() => _id;

        public static bool operator ==(FormFlowInstanceId left, FormFlowInstanceId right) => left.Equals(right);

        public static bool operator !=(FormFlowInstanceId left, FormFlowInstanceId right) => !(left == right);

        public static implicit operator string(FormFlowInstanceId instanceId) => instanceId.ToString();

        private static string GenerateIdForRouteValues(string key, IDictionary<string, object> routeValues)
        {
            var urlEncoder = UrlEncoder.Default;

            return routeValues.Aggregate(
                seed: urlEncoder.Encode(key),
                (l, r) => QueryHelpers.AddQueryString(l, r.Key, r.Value.ToString()));
        }
    }
}
