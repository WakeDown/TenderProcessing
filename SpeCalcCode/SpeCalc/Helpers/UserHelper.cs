using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using SpeCalc.Objects;
using SpeCalcDataAccessLayer;
using SpeCalcDataAccessLayer.Enums;
using SpeCalcDataAccessLayer.Models;
using Stuff.Objects;

namespace SpeCalc.Helpers
{
    //класс для работы с юзерами из ActiveDirectory
    public class UserHelper:DbModel
    {
        private static NetworkCredential nc = Settings.GetAdUserCredentials();

        private static List<UserRole> _roles;

        static UserHelper()
        {
            var db = new DbEngine();
            _roles = db.LoadRoles();
        }

        //получение юзера из идентичности потока
        public static UserBase GetUser(IIdentity identity)
        {
            using (WindowsImpersonationContextFacade impersonationContext
                = new WindowsImpersonationContextFacade(
                    nc))
            {
                //UserBase user = null;
                //var userName = identity.Name;
                //user = GetUserByName(userName);
                //return user;
                UserBase user = null;
                var wi = (WindowsIdentity) identity;
                if (wi.User != null)
                {
                    user = new UserBase();
                    var domain = new PrincipalContext(ContextType.Domain);
                    var id = "S-1-5-21-1970802976-3466419101-4042325969-1774";//wi.User.Value;
                    user.Id = id;
                    var login = "anton.demakov";//wi.Name.Remove(0, wi.Name.IndexOf("\\", StringComparison.CurrentCulture) + 1);
                    var userPrincipal = UserPrincipal.FindByIdentity(domain, login);
                    if (userPrincipal != null)
                    {
                        var mail = userPrincipal.EmailAddress;
                        var name = userPrincipal.DisplayName;
                        user.Email = mail;
                        user.Name = name;
                        user.ShortName = GetShortName(user.Name);
                        user.Roles = new List<Role>();
                        var wp = new WindowsPrincipal(wi);
                        //foreach (var role in _roles)
                        //{
                        //    var grpSid = new SecurityIdentifier(role.Sid);
                        //    if (wp.IsInRole(grpSid))
                        //    {
                        //        user.Roles.Add(role.Role);
                        //    }
                        //}
                        user.Roles.Add(Role.Enter);
                        user.Roles.Add(Role.Manager);
                    }
                }
            
            return user;
            }
        }

        public static bool IsUserAccess(UserBase user)
        {
            var result = user.Roles.Contains(Role.Enter);
            return result;
        }

        public static bool IsController(UserBase user)
        {
            var result = user.Roles.Contains(Role.Controller);
            return result;
        }

        public static bool IsProductManager(UserBase user)
        {
            var result = user.Roles.Contains(Role.ProductManager);
            return result;
        }

        public static bool IsOperator(UserBase user)
        {
            var result = user.Roles.Contains(Role.Operator);
            return result;
        }

        public static bool IsManager(UserBase user)
        {
            
            var result = user.Roles.Contains(Role.Manager);
            return result;
        }

        public static bool IsTenderStatus(UserBase user)
        {
            var result = user.Roles.Contains(Role.TenderStatus);
            return result;
        }

        //получение снабженцев из ActiveDirectory
        public static List<ProductManager> GetProductManagers()
        {
            //var man = new List<ProductManager>()
            //{
            //   new ProductManager(){Id = "dfsadfs", Name = "Гена", Roles = new List<Role>() { Role.Enter, Role.ProductManager}},
            //   new ProductManager(){Id = "fdbfgbv", Name = "Вася", Roles = new List<Role>() { Role.Enter, Role.ProductManager}},
            //   new ProductManager(){Id = "dfsdfvfdhadfs", Name = "Петр", Roles = new List<Role>() { Role.Enter, Role.ProductManager, Role.Manager}},
            //   new ProductManager(){Id = "dfsdwqedqwefefadfs", Name = "Олег", Roles = new List<Role>() { Role.Enter, Role.ProductManager, Role.Controller}},
            //   new ProductManager(){Id = "df45gfdgsadfs", Name = "Дима", Roles = new List<Role>() { Role.Enter, Role.ProductManager, Role.Operator}},
            //   new ProductManager(){Id = "dfsvdfgdfgdfbadfs", Name = "Alex", Roles = new List<Role>() { Role.Enter, Role.ProductManager}},
            //   new ProductManager(){Id = "khnhbfgbdf", Name = "Stan", Roles = new List<Role>() { Role.Enter, Role.ProductManager}}
            //};
            //man.ForEach((x) => x.ShortName = x.Name);
            //return man;
            using (WindowsImpersonationContextFacade impersonationContext
                = new WindowsImpersonationContextFacade(
                    nc))
            {
                var list = new List<ProductManager>();
                var domain = new PrincipalContext(ContextType.Domain);
                var group = GroupPrincipal.FindByIdentity(domain, IdentityType.Name,
                    _roles.First(x => x.Role == Role.ProductManager).Name);
                if (group != null)
                {
                    var members = group.GetMembers(true);
                    foreach (var principal in members)
                    {
                        var userPrincipal = UserPrincipal.FindByIdentity(domain, principal.Name);
                        if (userPrincipal != null)
                        {
                            var email = userPrincipal.EmailAddress;
                            var name = userPrincipal.DisplayName;
                            var sid = userPrincipal.Sid.Value;
                            var shortName = GetShortName(name);
                            var user = new ProductManager()
                            {
                                Id = sid,
                                Name = name,
                                Email = email,
                                ShortName = shortName,
                                Roles = new List<Role>() {Role.ProductManager}
                            };
                            list.Add(user);
                        }
                    }
                }

                list = list.OrderBy(m => m.ShortName).ToList();
                return list;
            }
        }

        //получение менеджеров из ActiveDirectory
        public static List<Manager> GetManagers()
        {
            //var man = new List<Manager>
            //{
            //    new Manager() {Id = "asd", Name = "Олег Иванов", Roles = new List<Role>() { Role.Enter, Role.Manager}, SubDivision = "Barcelona", Chief = "Александров А.А."},
            //    new Manager() {Id = "rtre", Name = "Андрей Петров", Roles = new List<Role>() { Role.Enter, Role.Manager, Role.Operator}, SubDivision = "Borussia", Chief = "Широков Р.В."},
            //    new Manager() {Id = "fgdsf", Name = "Дмитрий Степанов", Roles = new List<Role>() { Role.Enter, Role.Manager, Role.TenderStatus}, SubDivision = "Zenit", Chief = "Файзулин В.Г."}
            //};
            //man.ForEach((x) =>
            //{
            //    x.ShortName = x.Name;
            //    x.ChiefShortName = x.Chief;
            //});
            //return man;
            using (WindowsImpersonationContextFacade impersonationContext
                = new WindowsImpersonationContextFacade(
                    nc))
            {
                var list = new List<Manager>();
                var domain = new PrincipalContext(ContextType.Domain);
                var group = GroupPrincipal.FindByIdentity(domain, IdentityType.Name,
                    _roles.First(x => x.Role == Role.Manager).Name);
                if (group != null)
                {
                    var members = group.GetMembers(true);
                    foreach (var principal in members)
                    {
                        var userPrincipal = UserPrincipal.FindByIdentity(domain, principal.Name);
                        if (userPrincipal != null)
                        {
                            var email = userPrincipal.EmailAddress;
                            var name = userPrincipal.DisplayName;
                            var sid = userPrincipal.Sid.Value;
                            var shortName = GetShortName(name);
                            var departament = GetProperty(userPrincipal, "department");
                            var manager = GetProperty(userPrincipal, "manager");
                            var managerShortName = string.Empty;
                            if (!string.IsNullOrEmpty(manager))
                            {
                                var managerUser = UserPrincipal.FindByIdentity(domain, manager);
                                if (managerUser != null)
                                {
                                    manager = managerUser.DisplayName;
                                    managerShortName = GetShortName(manager);
                                }
                            }
                            var user = new Manager()
                            {
                                Id = sid,
                                Name = name,
                                ShortName = shortName,
                                Email = email,
                                SubDivision = departament,
                                Chief = manager,
                                ChiefShortName = managerShortName,
                                Roles = new List<Role>() {Role.Manager}
                            };
                            list.Add(user);
                        }
                    }
                }
                list = list.OrderBy(m => m.ShortName).ToList();

                return list;
            }
        }
        public static List<Manager> GetManagersSelectionList()
        {
            var list = new List<Manager>();
            foreach (var item in GetUserSelectionList(AdGroup.SpeCalcManager))
            {
                list.Add(new Manager() { Id = item.Key, ShortName = item.Value });
            }
            return list;
        }
        public static List<ProductManager> GetProductManagersSelectionList()
        {
           
            var list = new List<ProductManager>();
            foreach (var item in GetUserSelectionList(AdGroup.SpeCalcProduct))
            {
                list.Add(new ProductManager() {Id=item.Key, ShortName = item.Value});
            }
            return list;
        }


        public static IEnumerable<KeyValuePair<string, string>> GetUserSelectionList(AdGroup group)
        {
            Uri uri = new Uri($"{OdataServiceUri}/Ad/GetUserListByAdGroup?group={group}");
            string jsonString = GetJson(uri);
            var model = JsonConvert.DeserializeObject<IEnumerable<KeyValuePair<string, string>>>(jsonString);
            return model;
        }

        public static IEnumerable<KeyValuePair<string, string>> GetAuthorsSelectionList()
        {
            var list = new Dictionary<string, string>();
            var man = UserHelper.GetUserSelectionList(AdGroup.SpeCalcManager);  //Employee.GetManagerSelectionList();

            foreach (var m in man)
            {
                if (!list.ContainsKey(m.Key))
                    list.Add(m.Key, m.Value);
            }

            var oper = UserHelper.GetUserSelectionList(AdGroup.SpeCalcOperator);
            foreach (var o in oper)
            {
                if (!list.ContainsKey(o.Key)) list.Add(o.Key, o.Value);
            }
            return list;
        } 

        //public static IEnumerable<KeyValuePair<string, string>> GetOperators()
        //{
        //    return GetUserSelectionList(AdGroup.SpeCalcOperator);
        //    //using (WindowsImpersonationContextFacade impersonationContext
        //    //    = new WindowsImpersonationContextFacade(
        //    //        nc))
        //    //{
        //    //    var list = new List<Operator>();
        //    //    var domain = new PrincipalContext(ContextType.Domain);
        //    //    var group = GroupPrincipal.FindByIdentity(domain, IdentityType.Name,
        //    //        _roles.First(x => x.Role == Role.Operator).Name);
        //    //    if (group != null)
        //    //    {
        //    //        var members = group.GetMembers(true);
        //    //        foreach (var principal in members)
        //    //        {
        //    //            var userPrincipal = UserPrincipal.FindByIdentity(domain, principal.Name);
        //    //            if (userPrincipal != null)
        //    //            {
        //    //                var email = userPrincipal.EmailAddress;
        //    //                var name = userPrincipal.DisplayName;
        //    //                var sid = userPrincipal.Sid.Value;
        //    //                var shortName = GetShortName(name);
        //    //                var departament = GetProperty(userPrincipal, "department");
        //    //                var manager = GetProperty(userPrincipal, "manager");
        //    //                var managerShortName = string.Empty;
        //    //                if (!string.IsNullOrEmpty(manager))
        //    //                {
        //    //                    var managerUser = UserPrincipal.FindByIdentity(domain, manager);
        //    //                    if (managerUser != null)
        //    //                    {
        //    //                        manager = managerUser.DisplayName;
        //    //                        managerShortName = GetShortName(manager);
        //    //                    }
        //    //                }
        //    //                var user = new Operator()
        //    //                {
        //    //                    Id = sid,
        //    //                    Name = name,
        //    //                    ShortName = shortName,
        //    //                    Email = email,
        //    //                    SubDivision = departament,
        //    //                    Chief = manager,
        //    //                    ChiefShortName = managerShortName,
        //    //                    Roles = new List<Role>() { Role.Manager }
        //    //                };
        //    //                list.Add(user);
        //    //            }
        //    //        }
        //    //    }
        //    //    list = list.OrderBy(m => m.ShortName).ToList();

        //    //    return list;
        //    //}
        //}

        //public static List<ControllerUser> GetControllerUsers()
        //{
        //    return  new List<ControllerUser>()
        //    {
        //        new ControllerUser() { Id = "bngbtjradbdfgbffg", Name = "Тихонов Андрей", Roles = new List<Role>() { Role.Enter, Role.Controller}},
        //        new ControllerUser() { Id = "uyjtjuktsdfvwvfv", Name = "Аршавин Денис", Roles = new List<Role>() { Role.Enter, Role.Controller, Role.TenderStatus}},
        //    };
        //}

        //public static List<TenderStatusUser> GetTenderStatusUsers()
        //{
        //    return new List<TenderStatusUser>()
        //    {
        //        new TenderStatusUser() { Id = "rtyutyujyujyuj", Name = "C. Ronaldo", Roles = new List<Role>() { Role.Enter, Role.TenderStatus, Role.ProductManager}},
        //        new TenderStatusUser() { Id = "iumsdfvsdfsdr", Name = "L. Modrich", Roles = new List<Role>() { Role.Enter, Role.TenderStatus}},
        //    };
        //}

        //public static UserBase GetUserByName(string name)
        //{
        //    UserBase user = null;
        //    var managers = GetManagers();
        //    user = managers.FirstOrDefault(x => x.Name == name);
        //    if (user == null)
        //    {
        //        var products = GetProductManagers();
        //        user = products.FirstOrDefault(x => x.Name == name);
        //    }
        //    if (user == null)
        //    {
        //        var operators = GetOperators();
        //        user = operators.FirstOrDefault(x => x.Name == name);
        //    }
        //    if (user == null)
        //    {
        //        var controllerUsers = GetControllerUsers();
        //        user = controllerUsers.FirstOrDefault(x => x.Name == name);
        //    }
        //    if (user == null)
        //    {
        //        var tenderStatusUsers = GetTenderStatusUsers();
        //        user = tenderStatusUsers.FirstOrDefault(x => x.Name == name);
        //    }
        //    return user;
        //}

        //получение юзера по id(sid)
        public static UserBase GetUserById(string id)
        {
            //UserBase user = null;
            //var managers = GetManagers();
            //user = managers.FirstOrDefault(x => x.Id == id);
            //if (user == null)
            //{
            //    var products = GetProductManagers();
            //    user = products.FirstOrDefault(x => x.Id == id);
            //}
            //if (user == null)
            //{
            //    var operators = GetOperators();
            //    user = operators.FirstOrDefault(x => x.Id == id);
            //}
            //if (user == null)
            //{
            //    var controllerUsers = GetControllerUsers();
            //    user = controllerUsers.FirstOrDefault(x => x.Id == id);
            //}
            //if (user == null)
            //{
            //    var tenderStatusUsers = GetTenderStatusUsers();
            //    user = tenderStatusUsers.FirstOrDefault(x => x.Id == id);
            //}
            //return user;
            using (WindowsImpersonationContextFacade impersonationContext
                = new WindowsImpersonationContextFacade(
                    nc))
            {
                UserBase user = null;
                var domain = new PrincipalContext(ContextType.Domain);
                var userPrincipal = UserPrincipal.FindByIdentity(domain, IdentityType.Sid, id);
                if (userPrincipal != null)
                {
                    var email = userPrincipal.EmailAddress;
                    var name = userPrincipal.DisplayName;
                    var sid = userPrincipal.Sid.Value;
                    var shortName = GetShortName(name);
                    var manager = GetProperty(userPrincipal, "manager");
                    user = new UserBase()
                    {
                        Id = sid,
                        Name = name,
                        ShortName = shortName,
                        Email = email,
                        ManagerName = manager,
                        Roles = new List<Role>() { Role.Enter }
                    };
                }
                return user;
            }
        }

        private static string GetProperty(Principal principal, String property)
        {
            //using (WindowsImpersonationContextFacade impersonationContext
            //    = new WindowsImpersonationContextFacade(
            //        nc))
            //{
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
            //}
        }

        private static string GetShortName(string name)
        {

            var shortName = new StringBuilder();
            var partNames = name.Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries);
            if (partNames.Count() > 2)
            {
                shortName.Append(partNames[0]);
                shortName.Append(" ");
                shortName.Append(partNames[1].Substring(0, 1));
                shortName.Append(".");
                shortName.Append(partNames[2].Substring(0, 1));
                shortName.Append(".");
            }
            return shortName.ToString();
        }
    }
}