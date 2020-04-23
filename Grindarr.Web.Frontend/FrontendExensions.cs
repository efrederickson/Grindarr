using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;

namespace Grindarr.Web.Frontend
{
    public static class FrontendExensions
    {
        public static IApplicationBuilder UseGrindarrFrontend(this IApplicationBuilder builder, bool redirectRoot = true)
        {
            if (redirectRoot)
            {
                var rootRedirect = new RewriteOptions();
                rootRedirect.AddRedirect("^$", "/frontend/downloads");
                builder.UseRewriter(rootRedirect);
            }

            return builder;
        }

        public static void AddGrindarrFrontend(this IServiceCollection svc)
        {
            svc.ConfigureOptions<FrontendConfigureOptions>();
        }
    }
}
