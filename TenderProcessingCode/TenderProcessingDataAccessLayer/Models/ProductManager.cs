using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TenderProcessingDataAccessLayer.Models
{
    //Класс - юзер с ролью снабженец
    public class ProductManager : UserBase
    {
        public int PositionsCount { get; set; }

        public int CalculatesCount { get; set; }
    }
}
