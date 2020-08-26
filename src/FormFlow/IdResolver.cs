using System;
using FormFlow.Metadata;
using Microsoft.AspNetCore.Mvc;

namespace FormFlow
{
    internal class IdResolver
    {
        private const string IdQueryParameterName = "ffiid";

        public string ResolveId(ActionContext actionContext, FormFlowActionDescriptor flowDescriptor)
        {
            if (actionContext == null)
            {
                throw new ArgumentNullException(nameof(actionContext));
            }

            if (flowDescriptor == null)
            {
                throw new ArgumentNullException(nameof(flowDescriptor));
            }

            var request = actionContext.HttpContext.Request;

            return request.Query[IdQueryParameterName].ToString();
        }
    }
}
