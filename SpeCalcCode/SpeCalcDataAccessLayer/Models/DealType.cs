using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SpeCalcDataAccessLayer.Models
{
    //тпи сделки
    public class DealType : ServerDirectBase
    {
        public static SelectList GetList()
        { 
            return new SelectList(new DbEngine().LoadDealTypes(),"Id", "Value");
        }
    }

}
