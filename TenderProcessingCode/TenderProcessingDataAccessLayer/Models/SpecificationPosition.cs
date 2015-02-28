using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TenderProcessingDataAccessLayer.Enums;

namespace TenderProcessingDataAccessLayer.Models
{
    //класс - позиция заявки
    public class SpecificationPosition
    {
        public int Id { get; set; }

        public int IdClaim { get; set; }

        public int RowNumber { get; set; }

        public string CatalogNumber { get; set; }

        public string Name { get; set; }

        public string Replace { get; set; }

        public PositionUnit Unit { get; set; }

        public int Value { get; set; }

        public ProductManager ProductManager { get; set; }

        public string Comment { get; set; }

        public double Price { get; set; }

        public double Sum { get; set; }

        public int State { get; set; }

        public string Author { get; set; }

        public int Currency { get; set; }

        public List<CalculateSpecificationPosition> Calculations { get; set; } 
    }
}
