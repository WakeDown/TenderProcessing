using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using DataProvider.Helpers;

namespace SpeCalcDataAccessLayer.Models
{
    public class DeliveryTime : ServerDirectBase
    {
        public int Id { get; set; }
        public string Name { get; set; }
        //public string SysName { get; set; }

        public DeliveryTime()
        {

        }

        public DeliveryTime(DataRow row)
        {
            Id = Db.DbHelper.GetValueIntOrDefault(row, "Id");
            Name = Db.DbHelper.GetValueString(row, "Value");
            //SysName = Db.DbHelper.GetValueString(row, "SysName");
        }
        [OutputCache(Duration = 3600)]
        public static IEnumerable<DeliveryTime> GetList()
        {
            var dt = Db.SpeCalc.ExecuteQueryStoredProcedure("DeliveryTimeGetList");
            var list = new List<DeliveryTime>();
            foreach (DataRow row in dt.Rows)
            {
                var es = new DeliveryTime(row);
                list.Add(es);
            }
            return (list);
        }
        [OutputCache(Duration = 3600)]
        public static SelectList GetSelectionList()
        {
            return new SelectList(GetList(), "Id", "Name");
        }
    }
}
