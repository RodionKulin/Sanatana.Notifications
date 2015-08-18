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
        private SignaloBotQueries _queries;


        //инициализация
        public HomeController()
        {
            _signaloBot = new SignaloBotManager();
            _queries = new SignaloBotQueries();
        }            



        //методы
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Send()
        {
            ViewBag.SendQueue = _queries.SelectSignals();

            return View();
        }

        [HttpPost]
        public ActionResult Send(string param = null)
        {
            Exception exception;
            int subscribersCount;
            _signaloBot.SendGamesSignal(out exception, out subscribersCount);

            ViewBag.ResultMessage = exception == null
                ? string.Format(ViewContent.Home_Send_ResultSuccessful, subscribersCount)
                : exception.Message;

            ViewBag.SendQueue = _queries.SelectSignals();

            return View();
        }

    }
}