using System.Web.Mvc;
using System.Web.Security;
using Vocabulary.Domain.Abstract;
using Vocabulary.Web.Controllers;

namespace Vocabulary.Web.Areas.Main.Controllers
{
    public class HomeController : BaseController
    {
        private readonly IUserRepository _userRepository;

        public HomeController(ILanguageRepository languageRepository)
            : base(languageRepository) {}


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
            return RedirectToAction("BeginingPage", new {area = "Main"});
        }

        public ActionResult BeginingPage()
        {
            return View();
        }
    }
}
