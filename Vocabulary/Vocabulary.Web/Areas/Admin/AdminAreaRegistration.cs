using System.Web.Mvc;

namespace Vocabulary.Web.Areas.Admin
{
    public class AdminAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Admin";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                name: "Admin_lang",
                url: "{lang}/Admin/{controller}/{action}/{id}",
                defaults: new { controller = "Phrases", action = "Index", id = UrlParameter.Optional },
                constraints: new {lang = @"en|ru|uk"}
            );

            context.MapRoute(
                name: "Admin_default",
                url: "Admin/{controller}/{action}/{id}",
                defaults: new { controller = "Phrases", action = "Index", id = UrlParameter.Optional, lang="en" }
            );
        }
    }
}
