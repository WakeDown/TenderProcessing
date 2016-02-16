using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeCalcDataAccessLayer.HelpModels
{
    public class MessageItem
    {
        public int Id { get; set; }
        public DateTime CreateDate { get; set; } 
        public string Message { get; set; }
        public string CreatorName { get; set; }
        public string CreatorSId { get; set; }
    }
}
