using Common.Utility;
using MongoDB.Bson;
using SignaloBot.DAL;
using SignaloBot.Demo.Client.App_Resources;
using SignaloBot.Demo.Client.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SignaloBot.Demo.Client.Controllers
{
    public class UsersController : Controller
    {
        //поля
        protected SignaloBotManager _signalManager;



        //инициализация
        public UsersController()
        {
            _signalManager = new SignaloBotManager();
        }            



        //методы
        public async Task<ActionResult> List(int page = 1)
        {
            TotalResult<List<UserDeliveryTypeSettings<ObjectId>>> subscriberSettings =
                await _signalManager.GetAllSubscribers(page);

            return View(subscriberSettings);
        }

        public async Task<ActionResult> CheckEmail(string email)
        {
            QueryResult<bool> exists = await _signalManager.CheckEmailExists(email);
            return Json(new
            {
                exists = exists.Result,
                error = exists.HasExceptions
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
        public async Task<ActionResult> Add(NewUserVM user)
        {
            if (!ModelState.IsValid)
            {
                return View(user);
            }

            QueryResult<bool> exists = await _signalManager.CheckEmailExists(user.Email);
            if (exists.HasExceptions)
            {
                string message = string.Format(ViewContent.Common_DatabaseError, user.Email);
                ModelState.AddModelError("", message);
                return View(user);
            }
            if (exists.Result)
            {
                string message = string.Format(ViewContent.Users_Add_EmailExists, user.Email);
                ModelState.AddModelError("", message);
                return View(user);
            }

            bool addResult = await _signalManager.AddUser(user);
            if (!addResult)
            {
                string message = string.Format(ViewContent.Common_DatabaseError, user.Email);
                ModelState.AddModelError("", message);
                return View(user);
            }

            ViewBag.ResultMessage = ViewContent.Users_Add_AddSuccessful;
            return View(user);
        }

    }
}