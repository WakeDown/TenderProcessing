using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TenderProcessingDataAccessLayer.Models
{
    //юзер с ролью менеджер
    public class Manager : UserBase
    {
        public string SubDivision { get; set; }

        public string Chief { get; set; }

        public string ChiefShortName { get; set; }
    }
}
