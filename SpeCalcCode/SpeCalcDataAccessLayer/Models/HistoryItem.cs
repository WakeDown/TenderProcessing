using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using DataProvider.Helpers;

namespace Stuff.Models
{
    public class HistoryItem
    {
        public int Id { get; set; }
        public int IdState { get; set; }
        public string StateName { get; set; }
        public string CreatorSid { get; set; }
        public string CreatorName { get; set; }
        public DateTime DateCreate { get; set; }
        public int? LinkId { get; set; }

        public string DateCreateStr => DateCreate.ToString("dd.MM.yyyy HH:mm");

        public string Descr { get; set; }

        public HistoryItem()
        {
        }
        public HistoryItem(DataRow row)
            : this()
        {
            FillSelf(row);
        }

        private void FillSelf(DataRow row)
        {
            Id = Db.DbHelper.GetValueIntOrDefault(row, "id");
            CreatorSid = Db.DbHelper.GetValueString(row, "creator_sid");
            CreatorName = Db.DbHelper.GetValueString(row, "creator_name");
            IdState = Db.DbHelper.GetValueIntOrDefault(row, "id_state");
            StateName = Db.DbHelper.GetValueString(row, "state_name");
            Descr = Db.DbHelper.GetValueString(row, "descr");
            DateCreate = Db.DbHelper.GetValueDateTimeOrDefault(row, "dattim1");
            LinkId = Db.DbHelper.GetValueIntOrNull(row, "link_id");
        }
    }
}