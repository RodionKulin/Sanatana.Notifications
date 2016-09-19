using SignaloBot.Demo.Client.App_Resources;
using SignaloBot.Demo.Client.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SignaloBot.Demo.Client.Controllers
{
    public class HomeController : Controller
    {
        //поля
        private SignaloBotManager _signaloBot;


        //инициализация
        public HomeController()
        {
            _signaloBot = new SignaloBotManager();
        }            



        //методы
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Send()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Send(string param = null)
        {
            bool result = _signaloBot.EnqueueSignal();

            ViewBag.ResultMessage = result
                ? string.Format(ViewContent.Home_Send_ResultSuccessful, DateTime.Now.ToLongTimeString())
                : string.Format(ViewContent.Common_ServiceError, DateTime.Now.ToLongTimeString());
            
            return View();
        }

    }
}