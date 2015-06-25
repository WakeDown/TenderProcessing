using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TenderProcessing.Helpers;
using TenderProcessingDataAccessLayer.Models;

namespace TenderProcessing.Models
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
                managers.Add(new Employee() { AdSid = manager.Id, DisplayName = manager.ShortName });
            }

            managers = managers.OrderBy(m => m.DisplayName).ToList();

            return managers;
        }
    }
}