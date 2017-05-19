using System.Web.Mvc;

namespace Vocabulary.Web.Areas.Main
{
    public class MainAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Main";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                name: "Main_lang",
                url: "{lang}/Main/{controller}/{action}/{id}",
                constraints: new {lang = @"en|uk|ru"},
                defaults: new { action = "Index", id = UrlParameter.Optional },
                namespaces: new []{"Vocabulary.Web.Areas.Main.Controllers"}
            );

            context.MapRoute(
                name: "Main_default",
                url: "Main/{controller}/{action}/{id}",
                defaults: new { action = "Index", id = UrlParameter.Optional, lang = "en"},
                namespaces: new[] { "Vocabulary.Web.Areas.Main.Controllers" }
            );
        }
    }
}
