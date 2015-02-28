using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using TenderProcessing.Helpers;
using TenderProcessingDataAccessLayer.Models;

namespace TenderProcessing.Controllers
{
    public class AuthController : Controller
    {
        public ActionResult Index()
        {
            var users = new List<UserBase>();
            users.AddRange(UserHelper.GetManagers());
            users.AddRange(UserHelper.GetProductManagers());
            users.AddRange(UserHelper.GetOperators());
            users.AddRange(UserHelper.GetControllerUsers());
            users.AddRange(UserHelper.GetTenderStatusUsers());
            ViewBag.Users = users;
            return View();
        }

        [HttpPost]
        public ActionResult Index(string userName)
        {
            FormsAuthentication.SetAuthCookie(userName, true);
            return RedirectToAction("List", "Claim");
        }

        public ActionResult SignOut()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index");
        }

        public ActionResult ErrorPage(string message)
        {
            return View();
        }
	}
}