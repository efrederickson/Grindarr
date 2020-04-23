using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Threading.Tasks;

namespace Grindarr.Web.Api.Authorization
{
    public class ApiKeyMiddleware
    {
        private readonly PathString _securedPathPrefix;
        private readonly RequestDelegate _next;
        public ApiKeyMiddleware(RequestDelegate next, PathString securedPathPrefix)
        {
            _next = next;
            _securedPathPrefix = securedPathPrefix;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments(_securedPathPrefix))
            {
                string apiKey = null;
                if (context.Request.Headers.Keys.Contains("ApiKey", StringComparer.InvariantCultureIgnoreCase))
                    apiKey = context.Request.Headers["ApiKey"].FirstOrDefault();
                else if (context.Request.Query.ContainsKey("apikey"))
                    apiKey = context.Request.Query["apikey"];
                
                if (string.IsNullOrEmpty(apiKey) && ApiKeyWrapper.EnforceApiKey == true)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    await context.Response.WriteAsync("Missing API Key");
                } 
                else
                {
                    await ValidateApiKey(context, _next, apiKey);
                }
            }
            else
            {
                await _next(context);
            }
        }
        private static async Task ValidateApiKey(HttpContext context, RequestDelegate next, string apiKey)
        {
            var valid = ApiKeyWrapper.ValidateApiKey(apiKey);
            if (!valid)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                await context.Response.WriteAsync("Invalid API Key");
            }
            else
                await next.Invoke(context);
        }
    }
}
