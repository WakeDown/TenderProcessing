using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using DataProvider.Helpers;

namespace Stuff.Models
{
    public class StateItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string SysName { get; set; }
        public string Count { get; set; }
        public string BorderColor { get; set; }
        public string BackgroundColor { get; set; }
        public string ForegroundColor { get; set; }

        public string Descr { get; set; }

        public StateItem()
        {
        }
        public StateItem(DataRow row)
            : this()
        {
            FillSelf(row);
        }

        private void FillSelf(DataRow row)
        {
            Id = Db.DbHelper.GetValueIntOrDefault(row, "id");
            Name = Db.DbHelper.GetValueString(row, "name");
            SysName = Db.DbHelper.GetValueString(row, "sys_name");
            Count = Db.DbHelper.GetValueString(row, "count");
            BorderColor = Db.DbHelper.GetValueString(row, "border_color");
            BackgroundColor = Db.DbHelper.GetValueString(row, "background_color");
            ForegroundColor = Db.DbHelper.GetValueString(row, "foreground_color");
        }
    }
}