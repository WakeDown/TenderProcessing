using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Http;

namespace SpeCalc
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            
        }

        //protected void Application_BeginRequest(Object sender, EventArgs e)
        //{
        //    if (!Request.Url.ToString().Contains("ChromeOnly") && Request.UserAgent != null && !Request.UserAgent.Contains("Chrome") && !Request.UserAgent.Contains("CriOS"))
        //    {
        //        Response.RedirectToRoute("ChromeOnly", new { url = Request.Path.Replace("/", "|") });
        //    }
        //}
    }
}
