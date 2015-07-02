using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Security.Principal;
using System.Web;
using DocumentFormat.OpenXml.Spreadsheet;
using TenderProcessing.Objects;

namespace TenderProcessing.Helpers
{
    public class AdHelper
    {
        public static bool UserInGroup(IPrincipal user, params AdGroup[] groups)
        {
            var context = new PrincipalContext(ContextType.Domain);
            var userPrincipal = UserPrincipal.FindByIdentity(context,IdentityType.SamAccountName,user.Identity.Name);

            if (userPrincipal.IsMemberOf(context, IdentityType.Sid, AdUserGroup.GetSidByAdGroup(AdGroup.SuperAdmin))) { return true; }//Если юзер Суперадмин
            if (userPrincipal.IsMemberOf(context, IdentityType.Sid, AdUserGroup.GetSidByAdGroup(AdGroup.SpeCalcKontroler))) { return true; }//Если юзер Контролер

            foreach (var grp in groups)
            {
                if (userPrincipal.IsMemberOf(context, IdentityType.Sid, AdUserGroup.GetSidByAdGroup(grp))) { return true; }
            }
            

            return false;
        }
    }
}