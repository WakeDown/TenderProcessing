using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SpeCalc.Controllers
{
    public class ErrorController : Controller
    {
        //
        // GET: /Error/
        public ActionResult ChromeOnly(string url)
        {
            TempData["url"] = Request.Url.GetLeftPart(UriPartial.Authority) +  url.Replace("|", "/");
            return View();
        }
	}
}