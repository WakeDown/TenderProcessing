using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TenderProcessingDataAccessLayer.Models
{
    //класс - истроия статуса заявки
    public class ClaimStatusHistory
    {
        public int Id { get; set; }

        public int IdClaim { get; set; }

        public DateTime Date { get; set; }

        public string DateString { get; set; }

        public string Comment { get; set; }

        public string IdUser { get; set; }

        public ClaimStatus Status { get; set; }
    }
}
