using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SpeCalc.Models
{
    public class QuestionFilter
    {
        public int? Id { get; set; }
        public Employee Manager { get; set; }
        public Employee Product { get; set; }
        public List<QueState> States { get; set; }
        public int? Top { get; set; }

        public QuestionFilter()
        {
            Product = new Employee();
            Manager = new Employee();
            States = QueState.GetList().ToList();
            Top = 30;
        }
    }
}