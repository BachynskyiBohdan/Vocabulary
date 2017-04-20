using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Vocabulary.Domain.Abstract;
using Vocabulary.Domain.Entities;
using Vocabulary.Web.Models.Account;
using WebMatrix.WebData;

namespace Vocabulary.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserRepository _userRepository;

        public AccountController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public ActionResult SignIn()
        {
            var user = new LoginViewModel();
            return View(user);
        }
        [HttpPost]
        public ActionResult SignIn(LoginViewModel login)
        {
            var dbUser = _userRepository.Users.
                FirstOrDefault(u => login.UserName == u.UserTag || login.UserName == u.Email);

            if (dbUser == null)
            {
                ModelState.AddModelError("", "User with such username isn't exist.");
            }
            else if ((dbUser.UserTag != login.UserName && dbUser.Email != login.UserName) || dbUser.Password != login.Password)
            {
                ModelState.AddModelError("", "Username or password is incorrect.");
            }
            if (ModelState.IsValid)
            {
                WebSecurity.Login(dbUser.UserTag, dbUser.Password);
                Session["UserId"] = dbUser.Id;
                Session["Username"] = dbUser.UserTag;
                if (Roles.IsUserInRole(dbUser.UserTag, "Admin"))
                {
                    Session.Timeout = 20;
                    return RedirectToAction("MainPage", "Admin");
                }
                return RedirectToAction("Index", "Home");
            }
            TempData.Add("Error", "Username or password is incorrect");
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
            var user = new RegistrationViewModel();
            return View(user);
        }
        [HttpPost]
        public ActionResult Registration(RegistrationViewModel registration)
        {
            if (_userRepository.Users.FirstOrDefault(u => u.UserTag == registration.UserName) != null)
            {
                ModelState.AddModelError("", "Username is already exist.");
            }
            if (_userRepository.Users.FirstOrDefault(u => u.Email == registration.Email) != null)
            {
                ModelState.AddModelError("", "Email is already exist.");
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
                    return View("Error", (object)"Что-то пошло не так");
                }
                //var login = new LoginViewModel() {Password = registration.Password, UserName = registration.UserName};
                return RedirectToAction("SignIn", "Account", registration);
            }
            return View(registration);
        }



        #region Helpers
        private User InitializeUser(RegistrationViewModel registrationViewModel)
        {
            var user = new User();
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
