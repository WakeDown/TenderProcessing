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
    //класс - факт получения защиты
    public class ProtectFact : ServerDirectBase
    {
        public int Id { get; set; }
        public string Name { get; set; }
        //public string SysName { get; set; }

        public ProtectFact()
        {

        }

        public ProtectFact(DataRow row)
        {
            Id = Db.DbHelper.GetValueIntOrDefault(row, "Id");
            Name = Db.DbHelper.GetValueString(row, "Value");
            //SysName = Db.DbHelper.GetValueString(row, "SysName");
        }
        [OutputCache(Duration = 3600)]
        public static IEnumerable<ProtectFact> GetList()
        {
            var dt = Db.SpeCalc.ExecuteQueryStoredProcedure("ProtectFactGetList");
            var list = new List<ProtectFact>();
            foreach (DataRow row in dt.Rows)
            {
                var es = new ProtectFact(row);
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
