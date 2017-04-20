using System.Web.Mvc;
using System.Web.Security;

namespace Vocabulary.Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                if (Roles.IsUserInRole("Admin"))
                    return RedirectToAction("MainPage", "Admin");
                return RedirectToAction("Index", "Glossary");
            }
            return RedirectToAction("BeginingPage");
        }

        public ActionResult BeginingPage()
        {
            return View();
        }
    }
}
