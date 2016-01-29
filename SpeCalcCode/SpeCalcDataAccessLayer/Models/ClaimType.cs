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
    public class ClaimType
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string SysName { get; set; }

        public ClaimType()
        {

        }

        public ClaimType(DataRow row)
        {
            Id = Db.DbHelper.GetValueIntOrDefault(row, "id");
            Name = Db.DbHelper.GetValueString(row, "name");
            SysName = Db.DbHelper.GetValueString(row, "sys_name");
        }

        public static IEnumerable<ClaimType> GetList()
        {
            var dt = Db.SpeCalc.ExecuteQueryStoredProcedure("ClaimTypeGetList");
            var list = new List<ClaimType>();
            foreach (DataRow row in dt.Rows)
            {
                var es = new ClaimType(row);
                list.Add(es);
            }
            return (list);
        }

        public static SelectList GetSelectionList()
        {
            return new SelectList(GetList(), "Id", "Name");
        }
    }
}
