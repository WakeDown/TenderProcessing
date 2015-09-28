using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SpeCalc.Helpers;
using SpeCalcDataAccessLayer.Models;

namespace SpeCalc.Models
{
    public class Employee
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