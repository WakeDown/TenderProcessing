using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpeCalcDataAccessLayer.Enums;

namespace SpeCalcDataAccessLayer.Models
{
    //юзер а ролью оператор
    public class Operator : UserBase
    {
        public string SubDivision { get; set; }

        public string Chief { get; set; }

        public string ChiefShortName { get; set; }
    }
}
