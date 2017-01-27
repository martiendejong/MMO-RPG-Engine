using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MMO_RPG_Engine.Controllers
{
    public class PocController : Controller
    {
        // GET: TestPage
        public ActionResult Index()
        {
            return View();
        }

        // GET: TestPage
        public ActionResult Map()
        {
            return View();
        }

        public ActionResult Hello()
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<GameHub>();
            context.Clients.All.hello();
            return View("Index");
        }
    }
}