using System;
using System.Web.Mvc;

namespace ClearChat.Controllers
{
    public class HomeController : Controller
    {
        [Authorize]
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";
            return View("Index");
        }

        public ActionResult Logout()
        {
            Response.Cookies["FedAuth"].Expires = DateTime.Now.AddDays(-1);
            Response.Cookies["FedAuth1"].Expires = DateTime.Now.AddDays(-1);
            return RedirectToAction("Index", "Home");
        }
    }
}
