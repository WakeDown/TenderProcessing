using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using SpeCalc.Models;

namespace SpeCalc.Objects
{
    public class DbList
    {
        public static SelectList GetManagerSelectionList()
        {
            var managerSelectionList = new SelectList(Employee.GetManagerSelectionList(), "AdSid", "DisplayName");
            return managerSelectionList;
        }

        public static SelectList GetProductManagerSelectionList()
        {
            var productManagerSelectionList = new SelectList(Employee.GetProductManagerSelectionList(), "AdSid", "DisplayName");
            return productManagerSelectionList;
        }

        public static SelectList GetQueStateCheckedList()
        {
            return new SelectList(QueState.GetList(), "Id", "Name");
        }
    }
}