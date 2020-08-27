using System;
using Microsoft.AspNetCore.Http;

namespace FormFlow
{
    public class InstanceProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public InstanceProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public Instance GetInstance()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            return httpContext.Features.Get<FormFlowInstanceFeature>()?.Instance;
        }

        public Instance<T> GetInstance<T>()
        {
            return (Instance<T>)GetInstance();
        }
    }
}
