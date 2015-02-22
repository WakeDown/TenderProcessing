using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TenderProcessingDataAccessLayer.Enums;
using TenderProcessingDataAccessLayer.Models;

namespace TenderProcessing.Models
{
    public static class UserHelper
    {
        public static List<ProductManager> GetProductManagers()
        {
            return new List<ProductManager>()
            {
               new ProductManager(){Id = "dfsadfs", Name = "Гена", Roles = new List<Role>() { Role.ProductManager}},
               new ProductManager(){Id = "fdbfgbv", Name = "Вася", Roles = new List<Role>() { Role.ProductManager}},
               new ProductManager(){Id = "dfsdfvfdhadfs", Name = "Петр", Roles = new List<Role>() { Role.ProductManager, Role.Manager}},
               new ProductManager(){Id = "dfsdwqedqwefefadfs", Name = "Олег", Roles = new List<Role>() { Role.ProductManager, Role.Controller}},
               new ProductManager(){Id = "df45gfdgsadfs", Name = "Дима", Roles = new List<Role>() { Role.ProductManager, Role.Operator}},
               new ProductManager(){Id = "dfsvdfgdfgdfbadfs", Name = "Alex", Roles = new List<Role>() { Role.ProductManager}},
               new ProductManager(){Id = "khnhbfgbdf", Name = "Stan", Roles = new List<Role>() { Role.ProductManager}}
            };
        }

        public static ProductManager GetProductManagerFromActiveDirectoryByName(string name)
        {
            var managers = GetProductManagers();
            return managers.FirstOrDefault(x => x.Name == name);
        }

        public static ProductManager GetProductManagerFromActiveDirectoryById(string id)
        {
            var managers = GetProductManagers();
            return managers.FirstOrDefault(x => x.Id == id);
        }

        public static List<Manager> GetManagers()
        {
            return new List<Manager>
            {
                new Manager() {Id = "asd", Name = "Олег Иванов", Roles = new List<Role>() { Role.Manager}, SubDivision = "Barcelona", Chief = "Александров А.А."},
                new Manager() {Id = "rtre", Name = "Андрей Петров", Roles = new List<Role>() { Role.Manager, Role.Operator}, SubDivision = "Borussia", Chief = "Широков Р.В."},
                new Manager() {Id = "fgdsf", Name = "Дмитрий Степанов", Roles = new List<Role>() { Role.Manager, Role.TenderStatus}, SubDivision = "Zenit", Chief = "Файзулин В.Г."}
            };
        }

        public static Manager GetManagerFromActiveDirectoryById(string id)
        {
            var managers = GetManagers();
            return managers.FirstOrDefault(x => x.Id == id);
        }

        public static List<Operator> GetOperators()
        {
            return new List<Operator>()
            {
                new Operator(){ Id = "sdfsdfewrewrwe", Name = "L.Messi", Roles = new List<Role>() { Role.Operator}},
                new Operator(){ Id = "yuygurere", Name = "L.Suarez", Roles = new List<Role>() { Role.Operator, Role.Controller}}
            };
        }

        public static List<ControllerUser> GetControllerUsers()
        {
            return  new List<ControllerUser>()
            {
                new ControllerUser() { Id = "bngbtjradbdfgbffg", Name = "Тихонов Андрей", Roles = new List<Role>() { Role.Controller}},
                new ControllerUser() { Id = "uyjtjuktsdfvwvfv", Name = "Аршавин Денис", Roles = new List<Role>() { Role.Controller, Role.TenderStatus}},
            };
        }

        public static List<TenderStatusUser> GetTenderStatusUsers()
        {
            return new List<TenderStatusUser>()
            {
                new TenderStatusUser() { Id = "rtyutyujyujyuj", Name = "C. Ronaldo", Roles = new List<Role>() { Role.TenderStatus, Role.ProductManager}},
                new TenderStatusUser() { Id = "iumsdfvsdfsdr", Name = "L. Modrich", Roles = new List<Role>() { Role.TenderStatus}},
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

        public static bool IsUserAccess(UserBase user)
        {
            var result = false;
            if (user.Roles.Contains(Role.Controller) || user.Roles.Contains(Role.TenderStatus) ||
                user.Roles.Contains(Role.Manager) || user.Roles.Contains(Role.ProductManager) ||
                user.Roles.Contains(Role.Operator)) result = true;
            return result;
        }

        public static bool IsController(UserBase user)
        {
            var result = false;
            if (user.Roles.Contains(Role.Controller)) result = true;
            return result;
        }

        public static bool IsProductManager(UserBase user)
        {
            var result = false;
            if (user.Roles.Contains(Role.ProductManager)) result = true;
            return result;
        }

        public static bool IsOperator(UserBase user)
        {
            var result = false;
            if (user.Roles.Contains(Role.Operator)) result = true;
            return result;
        }

        public static bool IsManager(UserBase user)
        {
            var result = false;
            if (user.Roles.Contains(Role.Manager)) result = true;
            return result;
        }

        public static bool IsTenderStatus(UserBase user)
        {
            var result = false;
            if (user.Roles.Contains(Role.TenderStatus)) result = true;
            return result;
        }
    }
}