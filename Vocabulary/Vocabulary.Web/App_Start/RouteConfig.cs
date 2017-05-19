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

            //routes.MapRoute(
            //    name: "Admin",
            //    url: "Admin/{action}/{phraseId}",
            //    defaults: new { controller = "Admin", action = "MainPage", phraseId = UrlParameter.Optional }
            //);

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new {area = "Main", controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
            routes.MapRoute(
                "404-PageNotFound",
                "{*url}",
                new {controller = "Account", action = "PageNotFound"}
                );
        }
    }
}