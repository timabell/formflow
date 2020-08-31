using FormFlow.Metadata;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FormFlow
{
    public delegate IActionResult MissingInstanceHandler(FormFlowDescriptor flowDescriptor, HttpContext httpContext);
}
