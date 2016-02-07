//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SpeCalcDataAccessLayer
{
    using System;
    using System.Collections.Generic;
    
    public partial class ProjectWorkCalculations
    {
        public int Id { get; set; }
        public int WorkId { get; set; }
        public string Name { get; set; }
        public System.DateTime CreateDate { get; set; }
        public string CreatorSid { get; set; }
        public string CreatorName { get; set; }
        public bool Enabled { get; set; }
        public Nullable<System.DateTime> DeleteDate { get; set; }
        public string DeleterSid { get; set; }
        public string DeleterName { get; set; }
        public Nullable<int> StateId { get; set; }
        public Nullable<System.DateTime> StateChangeDate { get; set; }
        public string StateChangerSid { get; set; }
        public string StateChangerName { get; set; }
        public string ExecutorName { get; set; }
        public Nullable<decimal> Cost { get; set; }
        public Nullable<int> CurrencyId { get; set; }
        public Nullable<int> ExecutionTimeId { get; set; }
        public string ExecutionTime { get; set; }
        public string Comment { get; set; }
        public Nullable<System.DateTime> LastChangeDate { get; set; }
        public string LastChangerSid { get; set; }
        public string LastChangerName { get; set; }
    
        public virtual ProjectCurrencies ProjectCurrencies { get; set; }
        public virtual ProjectWorkExecutinsTimes ProjectWorkExecutinsTimes { get; set; }
        public virtual ProjectWorks ProjectWorks { get; set; }
    }
}
