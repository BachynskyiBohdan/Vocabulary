using System.Web.Mvc;
using System.Web.Security;
using Vocabulary.Web.Controllers;

namespace Vocabulary.Web.Areas.Main.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                if (Roles.IsUserInRole("Admin"))
                {
                    return RedirectToAction("MainPage", "Phrases", new { area = "Admin" });
                }
                return RedirectToAction("Index", "Glossary", new { area = "User" });
            }
            return RedirectToAction("BeginingPage");
        }

        public ActionResult BeginingPage()
        {
            return View();
        }
    }
}
