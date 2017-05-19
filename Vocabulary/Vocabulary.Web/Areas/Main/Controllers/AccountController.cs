using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using Vocabulary.Domain.Abstract;
using Vocabulary.Web.App_LocalResources;
using Vocabulary.Web.Areas.Main.Models;
using Vocabulary.Web.Controllers;
using WebMatrix.WebData;

namespace Vocabulary.Web.Areas.Main.Controllers
{
    public class AccountController : BaseController
    {
        private readonly IUserRepository _userRepository;

        public AccountController(IUserRepository userRepository, ILanguageRepository languageRepository)
            : base(languageRepository)
        {
            _userRepository = userRepository;
        }

        public ActionResult SignIn()
        {
            if (WebSecurity.IsAuthenticated)
            {
                if (Request.UrlReferrer != null)
                    return Redirect(Request.UrlReferrer.ToString());
                if (Roles.IsUserInRole("Admin"))
                    return RedirectToAction("MainPage", "Phrases", new {area = "Admin"});
                return RedirectToAction("Index", "Glossary", new { area = "User" });
            }
                
            var user = new LoginViewModel();
            return View(user);
        }
        [HttpPost]
        public ActionResult SignIn(LoginViewModel login, string ReturnUrl = null)
        {
            var dbUser = _userRepository.Users.
                FirstOrDefault(u => login.UserName == u.UserTag || login.UserName == u.Email);

            if (dbUser == null)
            {
                ModelState.AddModelError("", GlobalRes.ExistUsernameError);
            }
            else if ((dbUser.UserTag != login.UserName && dbUser.Email != login.UserName) || dbUser.Password != login.Password)
            {
                ModelState.AddModelError("", GlobalRes.UsernameOrPasswordIncorrent);
            }
            if (ModelState.IsValid)
            {
                WebSecurity.Login(dbUser.UserTag, dbUser.Password);
                if (Roles.IsUserInRole(dbUser.UserTag, "Admin"))
                {
                    Session.Timeout = 180;
                    if (!string.IsNullOrEmpty(ReturnUrl))
                    {
                        return Redirect(ReturnUrl);
                    }
                    return RedirectToAction("MainPage", "Phrases", new {area = "Admin"});
                }

                if (!string.IsNullOrEmpty(ReturnUrl))
                {
                    return Redirect(ReturnUrl);
                }
                return RedirectToAction("Index", "Home");
            }
            TempData.Add("Error", GlobalRes.UsernameOrPasswordIncorrent);
            return View(login);
        }

        public ActionResult SignOut()
        {
            if (WebSecurity.IsAuthenticated)
                WebSecurity.Logout();

            return RedirectToAction("Index", "Home");
        }

        public ActionResult Registration()
        {
            if (WebSecurity.IsAuthenticated)
            {
                if (Request.UrlReferrer != null)
                    return Redirect(Request.UrlReferrer.ToString());
                if (Roles.IsUserInRole("Admin"))
                    return RedirectToAction("MainPage", "Phrases", new { area = "Admin" });
                return RedirectToAction("Index", "Glossary", new { area = "User" });
            }

            var user = new RegistrationViewModel();
            return View(user);
        }
        [HttpPost]
        public ActionResult Registration(RegistrationViewModel registration)
        {
            if (_userRepository.Users.FirstOrDefault(u => u.UserTag == registration.UserName) != null)
            {
                ModelState.AddModelError("", GlobalRes.UsernameAlreadyExist);
            }
            if (_userRepository.Users.FirstOrDefault(u => u.Email == registration.Email) != null)
            {
                ModelState.AddModelError("", GlobalRes.EmailAlreadyExist);
            }
            if (ModelState.IsValid)
            {
                var dbUser = InitializeUser(registration);
                if (_userRepository.Add(dbUser))
                {
                    WebSecurity.CreateAccount(registration.UserName, registration.Password);
                    Roles.AddUserToRole(registration.UserName, "User");
                }
                else
                {
                    return View("Error", (object)GlobalRes.SomethingWrong);
                }
                return RedirectToAction("SignIn", "Account", registration);
            }
            return View(registration);
        }

        public ActionResult PageNotFound()
        {
            return View("Error");
        }

        #region Helpers

        private Domain.Entities.User InitializeUser(RegistrationViewModel registrationViewModel)
        {
            var user = new Domain.Entities.User();
            user.UserTag = registrationViewModel.UserName;
            user.Email = registrationViewModel.Email;
            user.Password = registrationViewModel.Password;
            user.RoleId = 2; // 1 - Admin, 2 - User, 3 - PremiumUser
            user.FirstName = "";
            user.LastName = "";
            user.IsPremium = false;
            user.MaximumPhrases = 50;

            return user;
        }


        #endregion Helpers
    }
}
