using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SpeCalc.Models
{
    public class TenderManagerPositionsReportFilter
    {
        public DateTime DateStart { get; set; }
        public DateTime DateEnd { get; set; }

        public TenderManagerPositionsReportFilter() { }

        public TenderManagerPositionsReportFilter(DateTime? dateStart, DateTime? dateEnd)
        {
            DateStart = dateStart ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            DateEnd = dateEnd ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month));
        }
    }
}