using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SpeCalcDataAccessLayer.Models
{
    //класс - настройки фильтра
    public class FilterTenderClaim
    {
        public int RowCount { get; set; }

        public int IdClaim { get; set; }

        public string TenderNumber { get; set; }

        public string Customer { get; set; }

        public List<int> ClaimStatus { get; set; }

        public int DealTypeId { get; set; }

        public string IdManager { get; set; }

        public string IdProductManager { get; set; }

        public string ManagerSubDivision { get; set; }

        public string TenderStartFrom { get; set; }

        public string TenderStartTo { get; set; }

        public string Author { get; set; }

        public bool? Overdie { get; set; }
    }
}