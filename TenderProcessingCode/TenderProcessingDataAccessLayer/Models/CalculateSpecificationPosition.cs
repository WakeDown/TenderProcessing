using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TenderProcessingDataAccessLayer.Models
{
    public class CalculateSpecificationPosition
    {
        public int Id { get; set; }

        public int IdSpecificationPosition { get; set; }

        public int IdTenderClaim { get; set; }

        public string CatalogNumber { get; set; }

        public string Name { get; set; }

        public string Replace { get; set; }

        public double PriceUsd { get; set; }

        public double SumUsd { get; set; }

        public double PriceRub { get; set; }

        public double SumRub { get; set; }

        public string Provider { get; set; }

        public ProtectFact ProtectFact { get; set; }

        public string ProtectCondition { get; set; }

        public string Comment { get; set; }
    }
}
