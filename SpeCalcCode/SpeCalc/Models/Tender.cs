using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DocumentFormat.OpenXml.Office2010.Excel;
using Newtonsoft.Json;
using Stuff.Objects;

namespace SpeCalc.Models
{
    public class Tender : DbModel
    {
        public Employee Manager { get; set; }
        public TenderState State { get; set; }
        public int PositionCount { get; set; }
        public int CalcCount { get; set; }

        public Tender() { }

        private void FillSelf(Tender model)
        {
            Manager = model.Manager;
            State = model.State;
            PositionCount = model.PositionCount;
            CalcCount = model.CalcCount;
        }

        public static IEnumerable<Tender> GetManagerReport(TenderManagerPositionsReportFilter filter)
        {
            //DateTime dateStart, DateTime dateEnd
            Uri uri = new Uri(String.Format("{0}/Tender/GetManagerReport?dateStart={1:dd.MM.yyyy}&dateEnd={2:dd.MM.yyyy}", OdataServiceUri, filter.DateStart.Date, filter.DateEnd.Date));
            string jsonString = GetJson(uri);
            var model = JsonConvert.DeserializeObject<IEnumerable<Tender>>(jsonString);
            return model;
        }
    }
}