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
    
    public partial class ProjectPositions
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ProjectPositions()
        {
            this.ProjectPositionCalculations = new HashSet<ProjectPositionCalculations>();
            this.ProjectPositionStateHistory = new HashSet<ProjectPositionStateHistory>();
            this.ProjectPositionStateHistory1 = new HashSet<ProjectPositionStateHistory>();
        }
    
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string RowNum { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public int QuantityUnitId { get; set; }
        public string CalculatorSid { get; set; }
        public string CalculatorName { get; set; }
        public System.DateTime CreateDate { get; set; }
        public string CreatorSid { get; set; }
        public string CreatorName { get; set; }
        public bool Enabled { get; set; }
        public Nullable<System.DateTime> DeleteDate { get; set; }
        public string DeleterSid { get; set; }
        public string DeleterName { get; set; }
        public int StateId { get; set; }
        public Nullable<System.DateTime> StateChangeDate { get; set; }
        public string StateChangerSid { get; set; }
        public string StateChangerName { get; set; }
        public string Vendor { get; set; }
        public string CatalogNumber { get; set; }
        public Nullable<System.DateTime> LastChangeDate { get; set; }
        public string LastChangerSid { get; set; }
        public string LastChangerName { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProjectPositionCalculations> ProjectPositionCalculations { get; set; }
        public virtual ProjectPositionQuantityUnits ProjectPositionQuantityUnits { get; set; }
        public virtual ProjectPositionStates ProjectPositionStates { get; set; }
        public virtual Projects Projects { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProjectPositionStateHistory> ProjectPositionStateHistory { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProjectPositionStateHistory> ProjectPositionStateHistory1 { get; set; }
    }
}
