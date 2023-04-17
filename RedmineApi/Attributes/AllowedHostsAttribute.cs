using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using RedmineApi.Data;

namespace RedmineApi.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class AllowedHostsAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            //  Get host from the request and check if it's in the enumeration of allowed hosts
            string host = context.HttpContext.Request.Host.Host;
            var configs = context.HttpContext.RequestServices.GetRequiredService<RedmineApiConfigs>();
            bool isAllowed = configs.AllowedHosts.Contains(host, StringComparison.OrdinalIgnoreCase);
            if (!isAllowed)
            {
                //  Request came from an authorized host, return bad request
                context.Result = new BadRequestObjectResult("Host is not allowed");
            }
        }
    }
}
