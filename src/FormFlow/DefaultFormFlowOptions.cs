using Microsoft.AspNetCore.Mvc;

namespace FormFlow
{
    public static class DefaultFormFlowOptions
    {
        public static MissingInstanceHandler MissingInstanceHandler { get; } =
            (flowDescriptor, httpContext) => new NotFoundResult();
    }
}
