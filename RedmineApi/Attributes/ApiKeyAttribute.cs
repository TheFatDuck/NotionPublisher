using Lib.Common;
using Microsoft.AspNetCore.Mvc.Filters;
using RedmineApi.Services;
using System.Net;

namespace RedmineApi.Attributes
{
    public class ApiKeyAttribute : Attribute, IAsyncActionFilter
    {
        private Dictionary<string, DateTime> _apiKeys = new Dictionary<string, DateTime>();
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate nextAction)
        {
            var ipAddress = context.HttpContext.Connection.RemoteIpAddress;
            var host = context.HttpContext.Request.Host;
            if (IPAddress.IsLoopback(ipAddress) || host.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase))
            {
                await nextAction();
                return;
            }
            string npApiKey = context.HttpContext.Request.Headers[CommonConsts.NAME_REQ_HEADER_NP_API_KEY];
            if (!string.IsNullOrEmpty(npApiKey))
            {
                //  Get the api key from the memory
                if (_apiKeys.TryGetValue(npApiKey, out DateTime lastAccessed)) 
                {
                    if(lastAccessed.AddMinutes(5) > DateTime.Now)
                    {
                        //  Update the last accessed time
                        _apiKeys[npApiKey] = DateTime.Now;
                        await nextAction();
                        return;
                    }
                    else
                        _apiKeys.Remove(npApiKey);
                }
                //  Get the api key from the database
                DBManager dbManager = context.HttpContext.RequestServices.GetRequiredService<DBManager>();
                if (dbManager.SelectUser(npApiKey) != null)
                {
                    _apiKeys[npApiKey] = DateTime.Now;
                    await nextAction();
                    return;
                }
            }
            // Authentication failed, return a 401 Unauthorized response
            context.HttpContext.Response.StatusCode = 401;
            await context.HttpContext.Response.WriteAsync("Unauthorized");
        }
    }
}
