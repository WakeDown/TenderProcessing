using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TenderProcessing.Models;

namespace TenderProcessing.Objects
{
    public class DbList
    {
        public static SelectList GetManagerSelectionList()
        {
            return new SelectList(Employee.GetManagerSelectionList(), "AdSid", "DisplayName");
        }

        public static SelectList GetProductManagerSelectionList()
        {
            return new SelectList(Employee.GetProductManagerSelectionList(), "AdSid", "DisplayName");
        }
    }
}