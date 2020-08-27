using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace FormFlow
{
    public static class RedirectToActionResultExtensions
    {
        public static RedirectToActionResult WithFormFlowInstanceId(
            this RedirectToActionResult result,
            Instance instance)
        {
            return WithFormFlowInstanceId(result, instance.InstanceId);
        }

        public static RedirectToActionResult WithFormFlowInstanceId(
            this RedirectToActionResult result,
            InstanceId instanceId)
        {
            result.RouteValues ??= new RouteValueDictionary();

            foreach (var kvp in instanceId.RouteValues)
            {
                result.RouteValues.Add(kvp.Key, kvp.Value);
            }

            return result;
        }
    }
}
