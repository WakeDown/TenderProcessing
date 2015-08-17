using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using Stuff.Objects;

namespace SpeCalc.Models
{
    public class QueState:DbModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string SysName { get; set; }
        public int OrderNum { get; set; }
        public bool Checked { get; set; }

        public QueState() { }

        public static IEnumerable<QueState> GetList()
        {
            Uri uri = new Uri(String.Format("{0}/QueState/GetList", OdataServiceUri));
            string jsonString = GetJson(uri);
            var model = JsonConvert.DeserializeObject<IEnumerable<QueState>>(jsonString);
            return model;
        }

        private void FillSelf(QueState model)
        {
            Id = model.Id;
            Name = model.Name;
            SysName = model.SysName;
            OrderNum = model.OrderNum;
        }
    }
}