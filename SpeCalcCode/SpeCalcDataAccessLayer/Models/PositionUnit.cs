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
    public class PositionUnit
    {
        public int Id { get; set; }
        public string Name { get; set; }
        //public string SysName { get; set; }

        public PositionUnit()
        {

        }

        public PositionUnit(DataRow row)
        {
            Id = Db.DbHelper.GetValueIntOrDefault(row, "Id");
            Name = Db.DbHelper.GetValueString(row, "Name");
            //SysName = Db.DbHelper.GetValueString(row, "SysName");
        }

        public static IEnumerable<PositionUnit> GetList()
        {
            var dt = Db.SpeCalc.ExecuteQueryStoredProcedure("PositionUnitGetList");
            var list = new List<PositionUnit>();
            foreach (DataRow row in dt.Rows)
            {
                var es = new PositionUnit(row);
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
