using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Web;
using TenderProcessingDataAccessLayer;
using TenderProcessingDataAccessLayer.Enums;
using TenderProcessingDataAccessLayer.Models;

namespace TenderProcessing.Helpers
{
    //класс для работы с юзерами из ActiveDirectory
    public static class UserHelper
    {
        private static List<UserRole> _roles;

        static UserHelper()
        {
            var db = new DbEngine();
            _roles = db.LoadRoles();
        }

        //получение юзера из идентичности потока
        public static UserBase GetUser(IIdentity identity)
        {
            UserBase user = null;
            var userName = identity.Name;
            user = GetUserByName(userName);
            //var wi = (WindowsIdentity)identity;
            //if (wi.User != null)
            //{
            //    user = new UserBase();
            //    var domain = new PrincipalContext(ContextType.Domain);
            //    var id = wi.User.Value;
            //    user.Id = id;
            //    var login = wi.Name.Remove(0, wi.Name.IndexOf("\\", StringComparison.CurrentCulture) + 1);
            //    var userPrincipal = UserPrincipal.FindByIdentity(domain, login);
            //    if (userPrincipal != null)
            //    {
            //        var mail = userPrincipal.EmailAddress;
            //        var name = userPrincipal.DisplayName;
            //        user.Email = mail;
            //        user.Name = name;
            //        user.ShortName = GetShortName(user.Name);
            //        user.Roles = new List<Role>();
            //        var wp = new WindowsPrincipal(wi);
            //        foreach (var role in _roles)
            //        {
            //            var grpSid = new SecurityIdentifier(role.Sid);
            //            if (wp.IsInRole(grpSid))
            //            {
            //                user.Roles.Add(role.Role);
            //            }
            //        }
            //    }
            //}
            return user;
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
            return new List<ProductManager>()
            {
               new ProductManager(){Id = "dfsadfs", Name = "Гена", Roles = new List<Role>() { Role.Enter, Role.ProductManager}},
               new ProductManager(){Id = "fdbfgbv", Name = "Вася", Roles = new List<Role>() { Role.Enter, Role.ProductManager}},
               new ProductManager(){Id = "dfsdfvfdhadfs", Name = "Петр", Roles = new List<Role>() { Role.Enter, Role.ProductManager, Role.Manager}},
               new ProductManager(){Id = "dfsdwqedqwefefadfs", Name = "Олег", Roles = new List<Role>() { Role.Enter, Role.ProductManager, Role.Controller}},
               new ProductManager(){Id = "df45gfdgsadfs", Name = "Дима", Roles = new List<Role>() { Role.Enter, Role.ProductManager, Role.Operator}},
               new ProductManager(){Id = "dfsvdfgdfgdfbadfs", Name = "Alex", Roles = new List<Role>() { Role.Enter, Role.ProductManager}},
               new ProductManager(){Id = "khnhbfgbdf", Name = "Stan", Roles = new List<Role>() { Role.Enter, Role.ProductManager}}
            };
            //var list = new List<ProductManager>();
            //var domain = new PrincipalContext(ContextType.Domain);
            //var group = GroupPrincipal.FindByIdentity(domain, IdentityType.Name, _roles.First(x => x.Role == Role.ProductManager).Name);
            //if (group != null)
            //{
            //    var members = group.GetMembers(true);
            //    foreach (var principal in members)
            //    {
            //        var userPrincipal = UserPrincipal.FindByIdentity(domain, principal.Name);
            //        if (userPrincipal != null)
            //        {
            //            var email = userPrincipal.EmailAddress;
            //            var name = userPrincipal.DisplayName;
            //            var sid = userPrincipal.Sid.Value;
            //            var shortName = GetShortName(name);
            //            var user = new ProductManager()
            //            {
            //                Id = sid,
            //                Name = name,
            //                Email = email,
            //                ShortName = shortName,
            //                Roles = new List<Role>() { Role.ProductManager }
            //            };
            //            list.Add(user);
            //        }
            //    }
            //}
            //return list;
        }

        //получение менеджеров из ActiveDirectory
        public static List<Manager> GetManagers()
        {
            return new List<Manager>
            {
                new Manager() {Id = "asd", Name = "Олег Иванов", Roles = new List<Role>() { Role.Enter, Role.Manager}, SubDivision = "Barcelona", Chief = "Александров А.А."},
                new Manager() {Id = "rtre", Name = "Андрей Петров", Roles = new List<Role>() { Role.Enter, Role.Manager, Role.Operator}, SubDivision = "Borussia", Chief = "Широков Р.В."},
                new Manager() {Id = "fgdsf", Name = "Дмитрий Степанов", Roles = new List<Role>() { Role.Enter, Role.Manager, Role.TenderStatus}, SubDivision = "Zenit", Chief = "Файзулин В.Г."}
            };
            //var list = new List<Manager>();
            //var domain = new PrincipalContext(ContextType.Domain);
            //var group = GroupPrincipal.FindByIdentity(domain, IdentityType.Name, _roles.First(x => x.Role == Role.Manager).Name);
            //if (group != null)
            //{
            //    var members = group.GetMembers(true);
            //    foreach (var principal in members)
            //    {
            //        var userPrincipal = UserPrincipal.FindByIdentity(domain, principal.Name);
            //        if (userPrincipal != null)
            //        {
            //            var email = userPrincipal.EmailAddress;
            //            var name = userPrincipal.DisplayName;
            //            var sid = userPrincipal.Sid.Value;
            //            var shortName = GetShortName(name);
            //            var departament = GetProperty(userPrincipal, "department");
            //            var manager = GetProperty(userPrincipal, "manager");
            //            var managerShortName = string.Empty;
            //            if (!string.IsNullOrEmpty(manager))
            //            {
            //                var managerUser = UserPrincipal.FindByIdentity(domain, manager);
            //                if (managerUser != null)
            //                {
            //                    manager = managerUser.DisplayName;
            //                    managerShortName = GetShortName(manager);
            //                }
            //            }
            //            var user = new Manager()
            //            {
            //                Id = sid,
            //                Name = name,
            //                ShortName = shortName,
            //                Email = email,
            //                SubDivision = departament,
            //                Chief = manager,
            //                ChiefShortName = managerShortName,
            //                Roles = new List<Role>() { Role.Manager }
            //            };
            //            list.Add(user);
            //        }
            //    }
            //}
            //return list;
        }

        //получение юзеров получателей извещений о просрочке сроков сдачи
        public static List<UserBase> GetExpiredNoteUsers()
        {
            var list = new List<UserBase>();
            var domain = new PrincipalContext(ContextType.Domain);
            var group = GroupPrincipal.FindByIdentity(domain, IdentityType.Name, _roles.First(x => x.Role == Role.ExpiredNote).Name);
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
                        var user = new UserBase()
                        {
                            Id = sid,
                            Name = name,
                            Email = email,
                            ShortName = shortName,
                            Roles = new List<Role>() { Role.ExpiredNote }
                        };
                        list.Add(user);
                    }
                }
            }
            return list;
        }

        public static List<Operator> GetOperators()
        {
            return new List<Operator>()
            {
                new Operator(){ Id = "sdfsdfewrewrwe", Name = "L.Messi", Roles = new List<Role>() { Role.Enter, Role.Operator}},
                new Operator(){ Id = "yuygurere", Name = "L.Suarez", Roles = new List<Role>() { Role.Enter, Role.Operator, Role.Controller}}
            };
        }

        public static List<ControllerUser> GetControllerUsers()
        {
            return  new List<ControllerUser>()
            {
                new ControllerUser() { Id = "bngbtjradbdfgbffg", Name = "Тихонов Андрей", Roles = new List<Role>() { Role.Enter, Role.Controller}},
                new ControllerUser() { Id = "uyjtjuktsdfvwvfv", Name = "Аршавин Денис", Roles = new List<Role>() { Role.Enter, Role.Controller, Role.TenderStatus}},
            };
        }

        public static List<TenderStatusUser> GetTenderStatusUsers()
        {
            return new List<TenderStatusUser>()
            {
                new TenderStatusUser() { Id = "rtyutyujyujyuj", Name = "C. Ronaldo", Roles = new List<Role>() { Role.Enter, Role.TenderStatus, Role.ProductManager}},
                new TenderStatusUser() { Id = "iumsdfvsdfsdr", Name = "L. Modrich", Roles = new List<Role>() { Role.Enter, Role.TenderStatus}},
            };
        }

        public static UserBase GetUserByName(string name)
        {
            UserBase user = null;
            var managers = GetManagers();
            user = managers.FirstOrDefault(x => x.Name == name);
            if (user == null)
            {
                var products = GetProductManagers();
                user = products.FirstOrDefault(x => x.Name == name);
            }
            if (user == null)
            {
                var operators = GetOperators();
                user = operators.FirstOrDefault(x => x.Name == name);
            }
            if (user == null)
            {
                var controllerUsers = GetControllerUsers();
                user = controllerUsers.FirstOrDefault(x => x.Name == name);
            }
            if (user == null)
            {
                var tenderStatusUsers = GetTenderStatusUsers();
                user = tenderStatusUsers.FirstOrDefault(x => x.Name == name);
            }
            return user;
        }

        //получение юзера по id(sid)
        public static UserBase GetUserById(string id)
        {
            UserBase user = null;
            var managers = GetManagers();
            user = managers.FirstOrDefault(x => x.Id == id);
            if (user == null)
            {
                var products = GetProductManagers();
                user = products.FirstOrDefault(x => x.Id == id);
            }
            if (user == null)
            {
                var operators = GetOperators();
                user = operators.FirstOrDefault(x => x.Id == id);
            }
            if (user == null)
            {
                var controllerUsers = GetControllerUsers();
                user = controllerUsers.FirstOrDefault(x => x.Id == id);
            }
            if (user == null)
            {
                var tenderStatusUsers = GetTenderStatusUsers();
                user = tenderStatusUsers.FirstOrDefault(x => x.Id == id);
            }
            return user;
            //UserBase user = null;
            //var domain = new PrincipalContext(ContextType.Domain);
            //var userPrincipal = UserPrincipal.FindByIdentity(domain, IdentityType.Sid, id);
            //if (userPrincipal != null)
            //{
            //    var email = userPrincipal.EmailAddress;
            //    var name = userPrincipal.DisplayName;
            //    var sid = userPrincipal.Sid.Value;
            //    var shortName = GetShortName(name);
            //    user = new UserBase()
            //    {
            //        Id = sid,
            //        Name = name,
            //        ShortName = shortName,
            //        Email = email,
            //        Roles = new List<Role>() { Role.Enter }
            //    };
            //}
            //return user;
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