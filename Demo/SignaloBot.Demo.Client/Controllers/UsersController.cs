using SignaloBot.DAL.Entities.Core;
using SignaloBot.Demo.Client.App_Resources;
using SignaloBot.Demo.Client.Model;
using SignaloBot.Demo.Client.Model.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SignaloBot.Demo.Client.Controllers
{
    public class UsersController : Controller
    {
        //поля
        protected SignaloBotManager _signaloBot;



        //инициализация
        public UsersController()
        {
            _signaloBot = new SignaloBotManager();
        }            



        //методы
        public ActionResult List(int page = 1)
        {
            Exception exception;
            int total;

            List<UserDeliveryTypeSettings> subscriberSettings =
                _signaloBot.GetAllSubscribers(page, out total, out exception);

            return View(subscriberSettings);
        }

        public ActionResult CheckEmail(string email)
        {
            bool exists = _signaloBot.CheckEmailExists(email);
            return Json(new
            {
                exists = exists
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Add()
        {
            NewUserVM user = new NewUserVM()
            {
                IsEmailEnabled = true
            };
            return View(user);
        }

        [HttpPost]
        public ActionResult Add(NewUserVM user)
        {
            if (!ModelState.IsValid)
            {
                return View(user);
            }

            bool exists = _signaloBot.CheckEmailExists(user.Email);
            if (exists)
            {
                string message = string.Format(ViewContent.Users_Add_EmailExists, user.Email);
                ModelState.AddModelError("", message);
                return View(user);
            }

            bool result = _signaloBot.AddUser(user);
            if (result)
            {
                ViewBag.ResultMessage = ViewContent.Users_Add_AddSuccessful;
            }

            return View(user);
        }

    }
}