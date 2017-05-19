using System.Web.Mvc;

namespace Vocabulary.Web.Areas.User
{
    public class UserAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "User";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                name: "User_lang",
                url: "{lang}/User/{controller}/{action}/{id}",
                constraints: new {lang = @"en|ru|uk"},
                defaults: new { action = "Index", id = UrlParameter.Optional }
            );

            context.MapRoute(
                "User_default",
                "User/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional, lang = "en" }
            );
        }
    }
}
