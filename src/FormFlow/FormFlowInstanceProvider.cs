using System;
using Microsoft.AspNetCore.Http;

namespace FormFlow
{
    public class FormFlowInstanceProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public FormFlowInstanceProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public FormFlowInstance GetInstance()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            return httpContext.Features.Get<FormFlowInstanceFeature>()?.Instance;
        }

        public FormFlowInstance<T> GetInstance<T>()
        {
            return (FormFlowInstance<T>)GetInstance();
        }
    }
}
