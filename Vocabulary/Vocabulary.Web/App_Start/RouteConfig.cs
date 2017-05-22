using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Vocabulary.Web
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default_lang",
                url: "{lang}/{controller}/{action}/{id}",
                defaults: new { area = "Main", controller = "Home", action = "Index", id = UrlParameter.Optional},
                constraints: new { lang = @"en|ru|uk"}
            );
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new {area = "Main", controller = "Home", action = "Index", id = UrlParameter.Optional, lang = "en" }
            );
            routes.MapRoute(
                "404-PageNotFound",
                "{*url}",
                new {controller = "Account", action = "PageNotFound"}
                );
        }
    }
}