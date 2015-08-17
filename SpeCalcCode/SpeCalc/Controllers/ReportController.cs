using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SpeCalc.Models;

namespace SpeCalc.Controllers
{
    public class ReportController : Controller
    {

        
        public ActionResult ManagerTenderPositions(DateTime? dateStart, DateTime? dateEnd)
        {
            //if (!dateStart.HasValue || !dateEnd.HasValue) return View("Error");

            //var list = Tender.GetManagerReport(dateStart.Value, dateEnd.Value);

            var filter = new TenderManagerPositionsReportFilter(dateStart, dateEnd);

            return View(filter);
        }
    }
}