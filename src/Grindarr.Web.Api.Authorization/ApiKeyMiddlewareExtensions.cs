using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grindarr.Web.Api.Authorization
{
    public static class ApiKeyMiddlewareExtensions
    {   public static IApplicationBuilder UseApiKeyMiddleware(this IApplicationBuilder builder, string securedPathPrefix)
        {
            return builder.UseMiddleware<ApiKeyMiddleware>(new PathString(securedPathPrefix));
        }
    }
}
