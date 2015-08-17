using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Web;
using DocumentFormat.OpenXml.Spreadsheet;
using SpeCalc.Objects;

namespace SpeCalc.Helpers
{
    public class AdHelper
    {
        private static NetworkCredential nc = Settings.GetAdUserCredentials();

        public static bool UserInGroup(IPrincipal user, params AdGroup[] groups)
        {
            using (WindowsImpersonationContextFacade impersonationContext
                = new WindowsImpersonationContextFacade(
                    nc))
            {
                var context = new PrincipalContext(ContextType.Domain);
                var userPrincipal = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName,
                    user.Identity.Name);

                if (userPrincipal.IsMemberOf(context, IdentityType.Sid, AdUserGroup.GetSidByAdGroup(AdGroup.SuperAdmin)))
                {
                    return true;
                } //Если юзер Суперадмин
                if (userPrincipal.IsMemberOf(context, IdentityType.Sid,
                    AdUserGroup.GetSidByAdGroup(AdGroup.SpeCalcKontroler)))
                {
                    return true;
                } //Если юзер Контролер

                foreach (var grp in groups)
                {
                    if (userPrincipal.IsMemberOf(context, IdentityType.Sid, AdUserGroup.GetSidByAdGroup(grp)))
                    {
                        return true;
                    }
                }


                return false;
            }
        }
    }
}