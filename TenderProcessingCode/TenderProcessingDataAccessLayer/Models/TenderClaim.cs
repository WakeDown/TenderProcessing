using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TenderProcessingDataAccessLayer.Enums;

namespace TenderProcessingDataAccessLayer.Models
{
    public class TenderClaim
    {
        public int Id { get; set; }

        public string TenderNumber { get; set; }

        public DateTime TenderStart { get; set; }

        public DateTime ClaimDeadline { get; set; }

        public DateTime KPDeadline { get; set; }

        public string TenderStartString { get; set; }

        public string ClaimDeadlineString { get; set; }

        public string KPDeadlineString { get; set; }

        public string Comment { get; set; }

        public string Customer { get; set; }

        public string CustomerInn { get; set; }

        public double Sum { get; set; }

        public int DealType { get; set; }

        public string TenderUrl { get; set; }

        public int ClaimStatus { get; set; }

        public Manager Manager { get; set; }

        public int TenderStatus { get; set; }

        public DateTime RecordDate { get; set; }

        public string RecordDateString { get; set; }

        public string Author { get; set; }

        public bool Deleted { get; set; }

        public List<ProductManager> ProductManagers { get; set; }

        public List<SpecificationPosition> Positions { get; set; } 
    }
}
