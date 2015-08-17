using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SpeCalc.Models
{
    public class HistoryQueState
    {
        public QueState State { get; set; }
        public Employee Creator { get; set; }
        public DateTime DateCreate { get; set; }
    }
}