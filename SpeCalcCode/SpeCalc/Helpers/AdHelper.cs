using System;
using System.Collections.Generic;
using System.Configuration;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Principal;
using System.Web;
using System.Web.Caching;
using DocumentFormat.OpenXml.Spreadsheet;
using SpeCalc.Models;
using SpeCalc.Objects;
using SpeCalcDataAccessLayer.Objects;

namespace SpeCalc.Helpers
{
    public class AdHelper
    {
        public static NetworkCredential GetAdUserCredentials()
        {
            string accUserName = @"UN1T\adUnit_prog";
            string accUserPass = "1qazXSW@";

            string domain = "UN1T";//accUserName.Substring(0, accUserName.IndexOf("\\"));
            string name = "adUnit_prog";//accUserName.Substring(accUserName.IndexOf("\\") + 1);

            NetworkCredential nc = new NetworkCredential(name, accUserPass, domain);

            return nc;
        }
        private const string DomainPath = "LDAP://DC=UN1T,DC=GROUP";
        private static NetworkCredential nc = GetAdUserCredentials();

        public static IEnumerable<KeyValuePair<string, string>> GetSpecialistList(AdGroup grp)
        {
            var list = new Dictionary<string, string>();

            using (WindowsImpersonationContextFacade impersonationContext
                = new WindowsImpersonationContextFacade(
                    nc))
            {
                var domain = new PrincipalContext(ContextType.Domain);
                var group = GroupPrincipal.FindByIdentity(domain, IdentityType.Sid, AdUserGroup.GetSidByAdGroup(grp));
                if (group != null)
                {
                    var members = group.GetMembers(true);
                    foreach (var principal in members)
                    {
                        var userPrincipal = UserPrincipal.FindByIdentity(domain, principal.SamAccountName);
                        if (userPrincipal != null)
                        {
                            var name = MainHelper.ShortName(userPrincipal.DisplayName);
                            var sid = userPrincipal.Sid.Value;
                            list.Add(sid, name);
                        }
                    }
                }

                return list.OrderBy(x => x.Value);
            }
        }

        public static MailAddress[] GetRecipientsFromAdGroup(AdGroup group)
        {
            var list = new List<MailAddress>();
            using (WindowsImpersonationContextFacade impersonationContext
                = new WindowsImpersonationContextFacade(
                    nc))
            {
                string sid = AdUserGroup.GetSidByAdGroup(group);
                PrincipalContext ctx = new PrincipalContext(ContextType.Domain);
                GroupPrincipal grp = GroupPrincipal.FindByIdentity(ctx, IdentityType.Sid, sid);

                if (grp != null)
                {
                    foreach (Principal p in grp.GetMembers(true))
                    {
                        string email = new EmployeeSm(p.Sid.Value).Email;
                        if (String.IsNullOrEmpty(email)) continue;
                        list.Add(new MailAddress(email));
                    }
                    grp.Dispose();
                }

                ctx.Dispose();

                return list.ToArray();
            }
        }

        public static IEnumerable<KeyValuePair<string, string>> GetUserListByAdGroup(string grpSid)
        {
            var list = new Dictionary<string, string>();

            using (WindowsImpersonationContextFacade impersonationContext
                = new WindowsImpersonationContextFacade(
                    nc))
            {
                var domain = new PrincipalContext(ContextType.Domain);
                var group = GroupPrincipal.FindByIdentity(domain, IdentityType.Sid, grpSid);
                if (group != null)
                {
                    var members = group.GetMembers(true);
                    foreach (var principal in members)
                    {
                        var userPrincipal = UserPrincipal.FindByIdentity(domain, principal.SamAccountName);
                        if (userPrincipal != null)
                        {
                            var name = EmployeeSm.ShortName(userPrincipal.DisplayName);
                            var sid = userPrincipal.Sid.Value;
                            list.Add(sid, name);
                        }
                    }
                }
            }

            return list.OrderBy(x => x.Value);
        }

        


        public static IEnumerable<KeyValuePair<string, string>> GetUserListByAdGroup(AdGroup grp)
        {
            var list = new Dictionary<string, string>();

            using (WindowsImpersonationContextFacade impersonationContext
                = new WindowsImpersonationContextFacade(
                    nc))
            {
                var domain = new PrincipalContext(ContextType.Domain);
                var group = GroupPrincipal.FindByIdentity(domain, IdentityType.Sid, AdUserGroup.GetSidByAdGroup(grp));
                if (group != null)
                {
                    var members = group.GetMembers(true);
                    foreach (var principal in members)
                    {
                        var userPrincipal = UserPrincipal.FindByIdentity(domain, principal.SamAccountName);
                        if (userPrincipal != null)
                        {
                            var name = EmployeeSm.ShortName(userPrincipal.DisplayName);
                            var sid = userPrincipal.Sid.Value;
                            list.Add(sid, name);
                        }
                    }
                }
            }

            return list.OrderBy(x => x.Value);
        }
        public static EmployeeSm GetUserBySid(string sid)
        {
            var result = new EmployeeSm();

            using (WindowsImpersonationContextFacade impersonationContext
                = new WindowsImpersonationContextFacade(
                    nc))
            {
                var context = new PrincipalContext(ContextType.Domain);
                var userPrincipal = UserPrincipal.FindByIdentity(context, IdentityType.Sid, sid);

                if (userPrincipal != null)
                {
                    result.AdSid = sid;
                    result.FullName = userPrincipal.DisplayName;
                    result.DisplayName = EmployeeSm.ShortName(result.FullName);
                    result.DepartmentName = GetProperty(userPrincipal, "department");
                    //result.ChiefSid = GetProperty(userPrincipal, "manager");
                    var chief = GetProperty(userPrincipal, "manager");
                    if (chief.Contains("CN="))
                    {
                        if (chief.Contains(","))
                        {
                            chief = chief.Substring(chief.IndexOf("CN=") + 3, chief.IndexOf(",")-3);
                        }
                        else
                        {
                            chief = chief.Substring(chief.IndexOf("CN=") + 3);
                        }
                    }
                    result.ChiefName = chief;
                }
            }

            return result;
        }

        private static string GetProperty(Principal principal, String property)
        {
            var result = string.Empty;
            var directoryEntry = principal.GetUnderlyingObject() as DirectoryEntry;
            if (directoryEntry != null)
            {
                if (directoryEntry.Properties.Contains(property))
                    result = directoryEntry.Properties[property].Value.ToString();
                else
                    result = string.Empty;
            }
            return result;
        }

        public static void SetUserAdGroups(IIdentity identity, ref AdUser user)
        {


            //using (WindowsImpersonationContextFacade impersonationContext
            //    = new WindowsImpersonationContextFacade(
            //        nc))
            //{
            var wi = (WindowsIdentity)identity;
            var context = new PrincipalContext(ContextType.Domain);


            if (identity != null && wi.User != null && user != null)
            {

                user.AdGroups = new List<AdGroup>();

                var wp = new WindowsPrincipal(wi);
                var gr = wi.Groups;

                foreach (AdUserGroup grp in AdUserGroup.GetList())
                {
                    var grpSid = new SecurityIdentifier(grp.Sid);
                    if (wp.IsInRole(grpSid))
                    {
                        user.AdGroups.Add(grp.Group);
                    }
                }
            }
            //}

        }

        public static bool UserInGroup(IPrincipal user, params AdGroup[] groups)
        {
            using (WindowsImpersonationContextFacade impersonationContext
                = new WindowsImpersonationContextFacade(
                    nc))
            {
                string fakseLogin = null;

                if (ConfigurationManager.AppSettings["UserProxy"] == "True")
                {
                    fakseLogin = ConfigurationManager.AppSettings["UserProxyLogin"];
                }
                string login = fakseLogin ?? user.Identity.Name;
                var context = new PrincipalContext(ContextType.Domain);
                var userPrincipal = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, login);

                if (userPrincipal == null) return false;
                if (userPrincipal.IsMemberOf(context, IdentityType.Sid, AdUserGroup.GetSidByAdGroup(AdGroup.SuperAdmin))) { return true; }//Если юзер Суперадмин

                //using (PrincipalContext context = new PrincipalContext(ContextType.Domain))
                //using (UserPrincipal user = UserPrincipal.FindByIdentity(context, userName))
                //using (PrincipalSearchResult<Principal> groups = user.GetAuthorizationGroups())
                //{
                //    return groups.OfType<GroupPrincipal>().Any(g => g.Name.Equals(groupName, StringComparison.OrdinalIgnoreCase));
                //}

                foreach (var grp in groups)
                {
                    if (userPrincipal.IsMemberOf(context, IdentityType.Sid, AdUserGroup.GetSidByAdGroup(grp)))
                    {
                        return true;
                    }
                }
                return false;
                //return groups.Select(grp => GroupPrincipal.FindByIdentity(context, IdentityType.Sid, AdUserGroup.GetSidByAdGroup(grp))).Where(g => g != null).Any(g => g.GetMembers(true).Cast<UserPrincipal>().Any(usr => usr.SamAccountName == login));
            }
        }

        public static bool UserInGroup(string sid, params AdGroup[] groups)
        {
            using (WindowsImpersonationContextFacade impersonationContext
                = new WindowsImpersonationContextFacade(
                    nc))
            {
                var context = new PrincipalContext(ContextType.Domain);
                var userPrincipal = UserPrincipal.FindByIdentity(context, IdentityType.Sid, sid);

                if (userPrincipal == null) return false;
                ////if (userPrincipal.IsMemberOf(context, IdentityType.Sid, AdUserGroup.GetSidByAdGroup(AdGroup.SuperAdmin))) { return true; }//Если юзер Суперадмин

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

        public static void ExcludeUserFromAdGroup(string userSid, params AdGroup[] groups)
        {
            using (WindowsImpersonationContextFacade impersonationContext
               = new WindowsImpersonationContextFacade(
                   nc))
            {
                var context = new PrincipalContext(ContextType.Domain);
                var userPrincipal = UserPrincipal.FindByIdentity(context, IdentityType.Sid, userSid);
                if (userPrincipal == null) return;

                foreach (var grp in groups)
                {
                    //Если пользователь является членом группы то исключаем
                    if (userPrincipal.IsMemberOf(context, IdentityType.Sid, AdUserGroup.GetSidByAdGroup(grp)))
                    {
                        var group = GroupPrincipal.FindByIdentity(context, IdentityType.Sid, AdUserGroup.GetSidByAdGroup(grp));
                        if (group != null)
                        {
                            group.Members.Remove(userPrincipal);
                            group.Save();
                        }
                    }
                    else
                    {
                        continue;
                    }
                }
            }
        }

        public static void ExcludeUserFromAdGroup(string userSid, params string[] groupSidList)
        {
            using (WindowsImpersonationContextFacade impersonationContext
               = new WindowsImpersonationContextFacade(
                   nc))
            {
                var context = new PrincipalContext(ContextType.Domain);
                var userPrincipal = UserPrincipal.FindByIdentity(context, IdentityType.Sid, userSid);
                if (userPrincipal == null) return;

                foreach (var grpSid in groupSidList)
                {
                    //Если пользователь является членом группы то исключаем
                    if (userPrincipal.IsMemberOf(context, IdentityType.Sid, grpSid))
                    {
                        var group = GroupPrincipal.FindByIdentity(context, IdentityType.Sid, grpSid);
                        if (group != null)
                        {
                            group.Members.Remove(userPrincipal);
                            group.Save();
                        }
                    }
                    else
                    {
                        continue;
                    }
                }
            }
        }

        public static void IncludeUser2AdGroup(string userSid, params AdGroup[] groups)
        {
            using (WindowsImpersonationContextFacade impersonationContext
               = new WindowsImpersonationContextFacade(
                   nc))
            {
                var context = new PrincipalContext(ContextType.Domain);
                var userPrincipal = UserPrincipal.FindByIdentity(context, IdentityType.Sid, userSid);
                if (userPrincipal == null) return;

                foreach (var grp in groups)
                {
                    //Если пользователь не является членом группы то включаем
                    if (userPrincipal.IsMemberOf(context, IdentityType.Sid, AdUserGroup.GetSidByAdGroup(grp)))
                    {
                        continue;
                    }
                    else
                    {
                        var group = GroupPrincipal.FindByIdentity(context, IdentityType.Sid,
                            AdUserGroup.GetSidByAdGroup(grp));
                        if (group != null)
                        {
                            group.Members.Add(userPrincipal);
                            group.Save();
                        }
                    }
                }

            }
        }

        public static void IncludeUser2AdGroup(string userSid, params string[] groupSidList)
        {
            using (WindowsImpersonationContextFacade impersonationContext
               = new WindowsImpersonationContextFacade(
                   nc))
            {
                var context = new PrincipalContext(ContextType.Domain);
                var userPrincipal = UserPrincipal.FindByIdentity(context, IdentityType.Sid, userSid);
                if (userPrincipal == null) return;

                foreach (var grpSid in groupSidList)
                {
                    //Если пользователь не является членом группы то включаем
                    if (userPrincipal.IsMemberOf(context, IdentityType.Sid, grpSid))
                    {
                        continue;
                    }
                    else
                    {
                        var group = GroupPrincipal.FindByIdentity(context, IdentityType.Sid, grpSid);
                        if (group != null)
                        {
                            group.Members.Add(userPrincipal);
                            group.Save();
                        }
                    }
                }

            }
        }

        public static string CreateSimpleAdUser(string username, string password, string name, string description, string adPath = "OU=Users,OU=UNIT,DC=UN1T,DC=GROUP")
        {
            using (WindowsImpersonationContextFacade impersonationContext
               = new WindowsImpersonationContextFacade(
                   nc))
            {
                bool userIsExist = false;
                DirectoryEntry directoryEntry = new DirectoryEntry(DomainPath);

                using (directoryEntry)
                {
                    //Если пользователь существует
                    DirectorySearcher search = new DirectorySearcher(directoryEntry);
                    search.Filter = String.Format("(&(objectClass=user)(sAMAccountName={0}))", username);
                    SearchResult resultUser = search.FindOne();
                    userIsExist = resultUser != null && resultUser.Properties.Contains("sAMAccountName");
                }

                if (!userIsExist)
                {
                    //Создаем аккаунт в AD
                    using (
                        var pc = new PrincipalContext(ContextType.Domain, "UN1T", adPath))
                    {
                        using (var up = new UserPrincipal(pc))
                        {
                            up.SamAccountName = username;
                            up.UserPrincipalName = username + "@unitgroup.ru";

                            up.SetPassword(password);
                            up.Enabled = true;
                            up.PasswordNeverExpires = true;
                            up.UserCannotChangePassword = true;
                            up.Description = description;
                            //up.DistinguishedName = "DC=unitgroup.ru";
                            try
                            {
                                up.Save();
                            }
                            catch (PrincipalOperationException ex)
                            {

                            }
                            up.UnlockAccount();
                        }
                    }
                }

                directoryEntry = new DirectoryEntry(DomainPath);
                using (directoryEntry)
                {

                    //DirectoryEntry user = directoryEntry.Children.Add("CN=" + username, "user");
                    DirectorySearcher search = new DirectorySearcher(directoryEntry);
                    search.Filter = String.Format("(&(objectClass=user)(sAMAccountName={0}))", username);
                    search.PropertiesToLoad.Add("objectsid");
                    search.PropertiesToLoad.Add("samaccountname");
                    search.PropertiesToLoad.Add("userPrincipalName");
                    search.PropertiesToLoad.Add("mail");
                    search.PropertiesToLoad.Add("usergroup");
                    search.PropertiesToLoad.Add("displayname");
                    search.PropertiesToLoad.Add("givenName");
                    search.PropertiesToLoad.Add("sn");
                    search.PropertiesToLoad.Add("title");
                    search.PropertiesToLoad.Add("telephonenumber");
                    search.PropertiesToLoad.Add("homephone");
                    search.PropertiesToLoad.Add("mobile");
                    search.PropertiesToLoad.Add("manager");
                    search.PropertiesToLoad.Add("l");
                    search.PropertiesToLoad.Add("company");
                    search.PropertiesToLoad.Add("department");

                    SearchResult resultUser = search.FindOne();

                    if (resultUser == null) return String.Empty;

                    DirectoryEntry user = resultUser.GetDirectoryEntry();
                    //SetProp(ref user, ref resultUser, "mail", mail);
                    SetProp(ref user, ref resultUser, "displayname", name);
                    SetProp(ref user, ref resultUser, "givenName", username);
                    SetProp(ref user, ref resultUser, "sn", name);
                    SetProp(ref user, ref resultUser, "title", description);
                    //SetProp(ref user, ref resultUser, "telephonenumber", workNum);
                    //SetProp(ref user, ref resultUser, "mobile", mobilNum);
                    SetProp(ref user, ref resultUser, "l", description);
                    SetProp(ref user, ref resultUser, "company", name);
                    SetProp(ref user, ref resultUser, "department", "-");
                    //SetProp(ref user, ref resultUser, "manager", "");
                    //user.Properties["jpegPhoto"].Clear();
                    //SetProp(ref user, ref resultUser, "jpegPhoto", photo);
                    user.CommitChanges();

                    SecurityIdentifier sid = new SecurityIdentifier((byte[])resultUser.Properties["objectsid"][0],
                        0);

                    return sid.Value;

                }
                return String.Empty;
            }
        }

        public static void SetProp(ref DirectoryEntry user, ref SearchResult result, string name, object value)
        {
            if (value == null || String.IsNullOrEmpty(value.ToString()) || String.IsNullOrEmpty(name)) return;
            if (result.Properties.Contains(name))
            {
                user.Properties[name].Value = value;
            }
            else
            {
                user.Properties[name].Add(value);
            }
        }

        //////public static IEnumerable<KeyValuePair<string, string>> GetGroupListByAdOrg(AdOrg org)
        //////{
        //////    var list = new Dictionary<string, string>();

        //////    using (WindowsImpersonationContextFacade impersonationContext
        //////        = new WindowsImpersonationContextFacade(
        //////            nc))
        //////    {
        //////        var domain = new PrincipalContext(ContextType.Domain, "UN1T.GROUP", String.Format("{0}, DC=UN1T,DC=GROUP", AdOrganization.GetAdPathByAdOrg(org)));
        //////        GroupPrincipal groupList = new GroupPrincipal(domain, "*");
        //////        PrincipalSearcher ps = new PrincipalSearcher(groupList);

        //////        foreach (var grp in ps.FindAll())
        //////        {
        //////            list.Add(grp.Sid.Value, grp.Name);
        //////        }
        //////    }

        //////    return list;
        //////}

        ////public static string GenerateLoginByName(string surname, string name)
        ////{
        ////    using (WindowsImpersonationContextFacade impersonationContext
        ////        = new WindowsImpersonationContextFacade(
        ////            nc))
        ////    {
        ////        string login = String.Empty;
        ////        int maxLoginLength = 19; //-1 - потомучто будет точка
        ////        var trans = new Transliteration();
        ////        string surnameTranslit = trans.GetTranslit(surname);
        ////        string nameTranslit = trans.GetTranslit(name);
        ////        //Если длина транслита превышает максимальное значение
        ////        if (surnameTranslit.Length > maxLoginLength)
        ////        {
        ////            surnameTranslit = surnameTranslit.Substring(0, maxLoginLength);
        ////            nameTranslit = String.Empty;
        ////        }
        ////        else if (surnameTranslit.Length + nameTranslit.Length > maxLoginLength)
        ////        {
        ////            nameTranslit = nameTranslit.Substring(0, maxLoginLength - surnameTranslit.Length);
        ////        }

        ////        bool flag = false;
        ////        int i = 0;
        ////        int j = 1;
        ////        string nameAccount = nameTranslit;
        ////        string surnameAccount = surnameTranslit;
        ////        do
        ////        {
        ////            if (i >= 1 && i < nameTranslit.Length)
        ////            {
        ////                nameAccount = nameTranslit.Substring(0, i);
        ////            }
        ////            else if (i >= 1 && i >= nameTranslit.Length)
        ////            {
        ////                login = "ERROR";
        ////                break;
        ////                //nameAccount = String.Format("{1}{0}", nameTranslit, j++);
        ////            }

        ////            login = String.Format("{0}.{1}", nameAccount, surnameAccount).ToLower();

        ////            DirectoryEntry directoryEntry = new DirectoryEntry(DomainPath);
        ////            using (directoryEntry)
        ////            {
        ////                DirectorySearcher search = new DirectorySearcher(directoryEntry);
        ////                search.Filter = String.Format("(&(objectClass=user)(sAMAccountName={0}))", login);
        ////                SearchResult result = search.FindOne();
        ////                flag = result != null && result.Properties.Contains("sAMAccountName");
        ////            }
        ////            i++;
        ////        } while (flag);

        ////        return login;
        ////    }
        ////}

        public static string CreateAdGroup(string name, string adPath)
        {
            string grpSid = String.Empty;

            using (WindowsImpersonationContextFacade impersonationContext
              = new WindowsImpersonationContextFacade(
                  nc))
            {
                //DirectoryEntry directoryEntry = new DirectoryEntry(DomainPath);
                //DirectoryEntry ou = directoryEntry.Children.Find(adPath);
                //DirectoryEntry group = ou.Children.Add($"CN={name}", "group");
                //group.Properties["samAccountName"].Value = name;
                //group.CommitChanges();

                bool groupIsExist = false;
                DirectoryEntry directoryEntry = new DirectoryEntry(DomainPath);

                using (directoryEntry)
                {
                    //Если пользователь существует
                    DirectorySearcher search = new DirectorySearcher(directoryEntry);
                    search.Filter = String.Format("(&(objectClass=user)(sAMAccountName={0}))", name);
                    SearchResult resultGroup = search.FindOne();
                    groupIsExist = resultGroup != null && resultGroup.Properties.Contains("sAMAccountName");

                    if (!groupIsExist)
                    {
                        DirectoryEntry ou = directoryEntry.Children.Find(adPath);
                        DirectoryEntry group = ou.Children.Add($"CN={name}", "group");
                        group.Properties["samAccountName"].Value = name;
                        group.CommitChanges();
                        SecurityIdentifier sid = new SecurityIdentifier((byte[])group.Properties["objectsid"][0],
                            0);
                        grpSid = sid.Value;
                    }
                    else
                    {
                        SecurityIdentifier sid = new SecurityIdentifier((byte[])resultGroup.Properties["objectsid"][0],
                            0);
                        grpSid = sid.Value;
                    }
                }
            }

            return grpSid;
        }

        public static string GetADGroupNameBySid(string sid)
        {
            string name = null;
            using (WindowsImpersonationContextFacade impersonationContext
                = new WindowsImpersonationContextFacade(
                    nc))
            {

                //Если пользователь существует
                var context = new PrincipalContext(ContextType.Domain);
                var gp = GroupPrincipal.FindByIdentity(context, IdentityType.Sid, sid);
                name = gp.SamAccountName;

            }

            return name;
        }
        //////private static NetworkCredential nc = Settings.GetAdUserCredentials();

        //////public static bool UserInGroup(IPrincipal user, params AdGroup[] groups)
        //////{
        //////    using (WindowsImpersonationContextFacade impersonationContext
        //////        = new WindowsImpersonationContextFacade(
        //////            nc))
        //////    {
        //////        var context = new PrincipalContext(ContextType.Domain);
        //////        var userPrincipal = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName,
        //////            user.Identity.Name);

        //////        if (userPrincipal.IsMemberOf(context, IdentityType.Sid, AdUserGroup.GetSidByAdGroup(AdGroup.SuperAdmin)))
        //////        {
        //////            return true;
        //////        } //Если юзер Суперадмин
        //////        if (userPrincipal.IsMemberOf(context, IdentityType.Sid,
        //////            AdUserGroup.GetSidByAdGroup(AdGroup.SpeCalcKontroler)))
        //////        {
        //////            return true;
        //////        } //Если юзер Контролер

        //////        foreach (var grp in groups)
        //////        {
        //////            if (userPrincipal.IsMemberOf(context, IdentityType.Sid, AdUserGroup.GetSidByAdGroup(grp)))
        //////            {
        //////                return true;
        //////            }
        //////        }


        //////        return false;
        //////    }
        //////}
    }
}