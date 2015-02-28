using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TenderProcessingDataAccessLayer.Models
{
    //Класс - расчет к позиции
    public class CalculateSpecificationPosition
    {
        public int Id { get; set; }

        public int IdSpecificationPosition { get; set; }

        public int IdTenderClaim { get; set; }

        public string CatalogNumber { get; set; }

        public string Name { get; set; }

        public string Replace { get; set; }

        public double PriceCurrency { get; set; }

        public double SumCurrency { get; set; }

        public int Currency { get; set; }

        public double PriceRub { get; set; }

        public double SumRub { get; set; }

        public string Provider { get; set; }

        public ProtectFact ProtectFact { get; set; }

        public string ProtectCondition { get; set; }

        public string Comment { get; set; }

        public string Author { get; set; }
    }
}
