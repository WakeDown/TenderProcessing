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
                    managers.Add(new Employee() { AdSid = manager.Id, DisplayName = manager.ShortName });
            }

            managers = managers.OrderBy(m => m.DisplayName).ToList();

            return managers;
        }
        /// <summary>
        /// Возвращает подчиненных владельца id как список продактов
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static List<ProductManager> GetSubordinateProductManagers(string id, List<KeyValuePair<string,string>> list = null)
        {
            var sidList = list ?? GetSubordinates(id);
            var subordinateList = new List<ProductManager>();

            foreach (var item in sidList)
            {
                if (!string.IsNullOrEmpty(item.Key))
                {
                    //var subordinate = UserHelper.GetUserById(sid);
                    var productManagerSubordinate = new ProductManager()
                    {
                        Id = item.Key,
                        //Name = subordinate.Name,
                        ShortName = item.Value,
                        //Roles = new List<Role>() { Role.ProductManager }
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
        public static List<Manager> GetSubordinateManagers(string id, List<KeyValuePair<string, string>> list = null)
        {
            var sidList = list ?? GetSubordinates(id);
            var subordinateList = new List<Manager>();
            //var chief = UserHelper.GetUserById(id);
            foreach (var item in sidList)
            {
                if (!string.IsNullOrEmpty(item.Key))
                {
                    //var subordinate = UserHelper.GetUserById(sid);
                    var managerSubordinate = new Manager()
                    {
                        Id = item.Key,
                        //Name = subordinate.Name,
                        ShortName = item.Value,
                        //Email = subordinate.Email,
                        ////SubDivision = subordinate,
                        //Chief = chief.Name,
                        //ChiefShortName = chief.ShortName,
                        //Roles = new List<Role>() { Role.Manager }
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
        public static IEnumerable<KeyValuePair<string, string>> GetSubordinates(string id)
        {
            Uri uri = new Uri(String.Format("{0}/Employee/GetSubordinatesSimple?sid={1}", OdataServiceUri, id));
            string jsonString = GetJson(uri);
            IEnumerable<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
            if (jsonString != null && jsonString != "{}") list = JsonConvert.DeserializeObject<IEnumerable<KeyValuePair<string, string>>>(jsonString);
            //var sidList = new List<string>() { id };
            //foreach (var pair in dictionary)
            //{
            //    sidList.Add(pair.Key);
            //}
            return list;
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

        public static bool UserIsSubordinate(IEnumerable<KeyValuePair<string, string>> subList, string userSid)
        {
            foreach (KeyValuePair<string, string> item in subList)
            {
                if (item.Key == userSid) return true;
            }
            return false;
        }

        //public static IEnumerable<KeyValuePair<string, string>> GetOperatorSelectionList()
        //{
        //    //var operators = new List<Employee>();

        //    //foreach (var oper in UserHelper.GetOperators())
        //    //{
        //    //    if (!String.IsNullOrEmpty(manager.ShortName))
        //    //        operators.Add(new Employee() { AdSid = manager.Id, DisplayName = manager.ShortName });
        //    //}

        //    //operators = operators.OrderBy(m => m.DisplayName).ToList();

        //    return UserHelper.GetOperators();
        //}
    }
}
