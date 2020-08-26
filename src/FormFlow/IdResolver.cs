using System;
using FormFlow.Metadata;
using Microsoft.AspNetCore.Http;

namespace FormFlow
{
    internal class IdResolver
    {
        private const string IdQueryParameterName = "ffiid";

        public string ResolveId(HttpRequest request, FormFlowActionDescriptor flowDescriptor)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (flowDescriptor == null)
            {
                throw new ArgumentNullException(nameof(flowDescriptor));
            }

            return request.Query[IdQueryParameterName].ToString();
        }
    }
}
