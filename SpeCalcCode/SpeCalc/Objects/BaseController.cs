using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using SpeCalc.Helpers;
using SpeCalcDataAccessLayer.Objects;

namespace SpeCalc.Objects
{
    [WhitespaceFilter]
    public class BaseController:Controller
    {
        private static NetworkCredential nc = GetAdUserCredentials();
        public static NetworkCredential GetAdUserCredentials()
        {
            string accUserName = @"UN1T\adUnit_prog";
            string accUserPass = "1qazXSW@";

            string domain = "UN1T";//accUserName.Substring(0, accUserName.IndexOf("\\"));
            string name = "adUnit_prog";//accUserName.Substring(accUserName.IndexOf("\\") + 1);

            NetworkCredential nc = new NetworkCredential(name, accUserPass, domain);

            return nc;
        }
        protected AdUser CurUser;

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            DisplayCurUser();
            base.OnActionExecuting(filterContext);
        }

        [NonAction]
        public AdUser GetCurUser()
        {
            if (Session["CurUser"] != null)
            {
                return (AdUser)Session["CurUser"];
            }

            AdUser user = new AdUser();
            user.User = base.User;
            try
            {

                //////List<GroupPrincipal> result = new List<GroupPrincipal>();

                //////// establish domain context
                //////PrincipalContext yourDomain = new PrincipalContext(ContextType.Domain);

                //////// find your user
                //////UserPrincipal usr = UserPrincipal.FindByIdentity(yourDomain, userName);

                //////// if found - grab its groups
                //////if (user != null)
                //////{
                //////    PrincipalSearchResult<Principal> groups = usr.GetAuthorizationGroups();

                //////    // iterate over all groups
                //////    foreach (Principal p in groups)
                //////    {
                //////        // make sure to add only group principals
                //////        if (p is GroupPrincipal)
                //////        {
                //////            result.Add((GroupPrincipal)p);
                //////        }
                //////    }
                //////}

                //////return user;

                string fakeSid = null;
                string fakeLosgin = null;
                //fakeSid = "S-1-5-21-1970802976-3466419101-4042325969-3837";
                //fakeLosgin = "olga.skidan";

                using (WindowsImpersonationContextFacade impersonationContext
                    = new WindowsImpersonationContextFacade(
                        nc))
                {
                    var wi = (WindowsIdentity)base.User.Identity;
                    if (wi.User != null)
                    {
                        var domain = new PrincipalContext(ContextType.Domain);
                        string sid = fakeSid ?? wi.User.Value;
                        user.Sid = sid;
                        var login = fakeLosgin ?? wi.Name.Remove(0, wi.Name.IndexOf("\\", StringComparison.CurrentCulture) + 1);
                        user.Login = login;
                        var userPrincipal = UserPrincipal.FindByIdentity(domain, login);
                        if (userPrincipal != null)
                        {
                            var mail = userPrincipal.EmailAddress;
                            var name = userPrincipal.DisplayName;
                            user.Email = mail;
                            user.FullName = name;
                            //user.AdGroups = new List<AdGroup>();
                            //var wp = new WindowsPrincipal(wi);
                            //foreach (var role in AdUserGroup.GetList())
                            //{
                            //    var grpSid = new SecurityIdentifier(role.Sid);
                            //    if (wp.IsInRole(grpSid))
                            //    {
                            //        user.AdGroups.Add(role.Group);
                            //    }
                            //}
                            AdHelper.SetUserAdGroups(wi, ref user);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }

            Session["CurUser"] = user;

            return user;
        }

        protected AdUser DisplayCurUser()
        {
            CurUser = GetCurUser();
            if (CurUser == new AdUser()) RedirectToAction("AccessDenied", "Error");
            ViewBag.CurUser = CurUser;
            return CurUser;
        }
    }
}