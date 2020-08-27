using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Encodings.Web;
using FormFlow.Metadata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebUtilities;

namespace FormFlow
{
    public readonly struct InstanceId : IEquatable<InstanceId>
    {
        private readonly string _id;

        internal InstanceId(string instanceId, RouteValueDictionary routeValues)
        {
            _id = instanceId ?? throw new ArgumentNullException(nameof(instanceId));
            RouteValues = routeValues ?? throw new ArgumentNullException(nameof(routeValues));
        }

        public IReadOnlyDictionary<string, object> RouteValues { get; }

        public static InstanceId Generate(
            ActionContext actionContext,
            FormFlowDescriptor flowDescriptor)
        {
            if (actionContext == null)
            {
                throw new ArgumentNullException(nameof(actionContext));
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
                return GenerateForRouteValues();
            }
            else
            {
                throw new NotSupportedException($"Unknown IdGenerationSource: '{flowDescriptor.IdGenerationSource}'.");
            }

            InstanceId GenerateForRandomId()
            {
                // Always generate a new ID, even if incoming routeValues have an existing one
                var id = Guid.NewGuid().ToString();

                return new InstanceId(
                    id,
                    new RouteValueDictionary()
                    {
                        { Constants.InstanceIdQueryParameterName, id }
                    });
            }

            InstanceId GenerateForRouteValues()
            {
                if (!TryResolve(actionContext, flowDescriptor, out var instanceId))
                {
                    var routeParameterNameList = string.Join(", ", flowDescriptor.IdRouteParameterNames);
                    throw new InvalidOperationException(
                        $"One or more route parameters are missing. Flow requires: {routeParameterNameList}.");
                }

                return instanceId;
            }
        }

        public static bool TryResolve(
            ActionContext actionContext,
            FormFlowDescriptor flowDescriptor,
            out InstanceId instanceId)
        {
            if (actionContext == null)
            {
                throw new ArgumentNullException(nameof(actionContext));
            }

            if (flowDescriptor == null)
            {
                throw new ArgumentNullException(nameof(flowDescriptor));
            }

            if (flowDescriptor.IdGenerationSource == IdGenerationSource.RandomId)
            {
                return TryCreateForRandomId(out instanceId);
            }
            else if (flowDescriptor.IdGenerationSource == IdGenerationSource.RouteValues)
            {
                return TryCreateForRouteValues(out instanceId);
            }
            else
            {
                throw new NotSupportedException($"Unknown IdGenerationSource: '{flowDescriptor.IdGenerationSource}'.");
            }

            bool TryCreateForRandomId(out InstanceId instanceId)
            {
                var id = actionContext.HttpContext.Request.Query[Constants.InstanceIdQueryParameterName].ToString();

                if (string.IsNullOrEmpty(id))
                {
                    instanceId = default;
                    return false;
                }

                instanceId = new InstanceId(
                    id,
                    new RouteValueDictionary()
                    {
                        { Constants.InstanceIdQueryParameterName, id }
                    });

                return true;
            }

            bool TryCreateForRouteValues(out InstanceId instanceId)
            {
                var urlEncoder = UrlEncoder.Default;

                var routeValues = actionContext.RouteData.Values;

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

                instanceId = new InstanceId(id, instanceRouteValues);
                return true;
            }
        }

        public bool Equals([AllowNull] InstanceId other) => _id == other._id;

        public override bool Equals(object obj) => obj is InstanceId x && x.Equals(this);

        public override int GetHashCode() => _id.GetHashCode();

        public override string ToString() => _id;

        public static bool operator ==(InstanceId left, InstanceId right) => left.Equals(right);

        public static bool operator !=(InstanceId left, InstanceId right) => !(left == right);

        public static implicit operator string(InstanceId instanceId) => instanceId.ToString();
    }
}
