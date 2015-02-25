using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TenderProcessingDataAccessLayer.Models
{
    //класс - количество расчетов по позиции
    public class PositionCalculateCount
    {
        public int Id { get; set; }

        public int Count { get; set; }
    }
}
