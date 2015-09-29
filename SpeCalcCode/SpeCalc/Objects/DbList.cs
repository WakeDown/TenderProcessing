using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using SpeCalc.Helpers;
using SpeCalc.Models;

namespace SpeCalc.Objects
{
    public class DbList
    {
        public static SelectList GetManagerSelectionList()
        {
            var managerSelectionList = new SelectList(UserHelper.GetUserSelectionList(AdGroup.SpeCalcManager), "Key", "Value");
            return managerSelectionList;
        }

        public static SelectList GetProductManagerSelectionList()
        {
            var productManagerSelectionList = new SelectList(UserHelper.GetUserSelectionList(AdGroup.SpeCalcProduct), "Key", "Value");
            return productManagerSelectionList;
        }

        public static SelectList GetQueStateCheckedList()
        {
            return new SelectList(QueState.GetList(), "Id", "Name");
        }

        public static SelectList GetManagerAndOperatorSelectionList()
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
                if (!list.ContainsKey(o.Key))list.Add(o.Key, o.Value);
            }

            //list.AddRange(oper);
            //list = list.OrderBy(x => x.Value).ToList();

            var managerSelectionList = new SelectList(list.OrderBy(x => x.Value).ToList(), "Key", "Value");
            return managerSelectionList;
        }
    }
}