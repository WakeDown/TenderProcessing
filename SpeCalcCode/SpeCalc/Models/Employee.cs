using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DocumentFormat.OpenXml.Presentation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SpeCalc.Helpers;
using SpeCalcDataAccessLayer.Enums;
using SpeCalcDataAccessLayer.Models;
using Stuff.Objects;

namespace SpeCalc.Models
{
    public class Employee : DbModel
    {

        public string AdSid { get; set; }
        public string FullName { get; set; }
        public string DisplayName { get; set; }

        public static IEnumerable<Employee> GetManagerSelectionList()
        {
            var managers = new List<Employee>();

            foreach (UserBase manager in UserHelper.GetManagers())
            {
                if (!String.IsNullOrEmpty(manager.ShortName))
                managers.Add(new Employee(){AdSid = manager.Id, DisplayName = manager.ShortName});
            }

            managers = managers.OrderBy(m => m.DisplayName).ToList();

            return managers;
        }
        /// <summary>
        /// Возвращает подчиненных владельца id как список продактов
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static List<ProductManager> GetSubordinateProductManagers(string id)
        {
            var sidList = GetSubordinates(id);
            var subordinateList = new List<ProductManager>();

            foreach (var sid in sidList)
            {
                if (!string.IsNullOrEmpty(sid))
                {
                    var subordinate = UserHelper.GetUserById(sid);
                    var productManagerSubordinate = new ProductManager()
                    {
                        Id = subordinate.Id,
                        Name = subordinate.Name,
                        ShortName = subordinate.ShortName,
                        Roles = new List<Role>() { Role.ProductManager }
                    };
                    subordinateList.Add(productManagerSubordinate);
                }

            }
            subordinateList = subordinateList.OrderBy(m => m.ShortName).ToList();
            return subordinateList;
        }
        /// <summary>
        /// Возвращает подчиненных владельца id как список менеджеров
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static List<Manager> GetSubordinateManagers(string id)
        {
            var sidList = GetSubordinates(id);
            var subordinateList = new List<Manager>();
            var chief = UserHelper.GetUserById(id);
            foreach (var sid in sidList)
            {
                if (!string.IsNullOrEmpty(sid))
                {
                   var subordinate = UserHelper.GetUserById(sid);
               var managerSubordinate = new Manager()
               {
                   Id = subordinate.Id,
                   Name = subordinate.Name,
                   ShortName = subordinate.ShortName,
                   Email = subordinate.Email,
                   //SubDivision = subordinate,
                   Chief = chief.Name,
                   ChiefShortName = chief.ShortName,
                   Roles = new List<Role>() { Role.Manager }
               }; 
                subordinateList.Add(managerSubordinate); 
                }
               
            }
            subordinateList = subordinateList.OrderBy(m => m.ShortName).ToList();
            return subordinateList;
        }
        /// <summary>
        /// Возвращает список sid`ов подчиненных владельца id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static List<string> GetSubordinates(string id)
        {
            Uri uri = new Uri(String.Format("{0}/Employee/GetSubordinatesSimple?sid={1}", OdataServiceUri, id));
            string jsonString = GetJson(uri);
            var dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonString);
            var sidList = new List<string>() {id};
            foreach (var pair in dictionary)
            {
                sidList.Add(pair.Key);
            }
            return sidList;
        }

        public static IEnumerable<Employee> GetProductManagerSelectionList()
        {
            var managers = new List<Employee>();

            foreach (UserBase manager in UserHelper.GetProductManagers())
            {
                if (!String.IsNullOrEmpty(manager.ShortName))
                managers.Add(new Employee() { AdSid = manager.Id, DisplayName = manager.ShortName });
            }

            managers = managers.OrderBy(m => m.DisplayName).ToList();

            return managers;
        }

        public static IEnumerable<Employee> GetOperatorSelectionList()
        {
            var operators = new List<Employee>();

            foreach (UserBase manager in UserHelper.GetOperators())
            {
                if (!String.IsNullOrEmpty(manager.ShortName))
                    operators.Add(new Employee() { AdSid = manager.Id, DisplayName = manager.ShortName });
            }

            operators = operators.OrderBy(m => m.DisplayName).ToList();

            return operators;
        }
    }
}