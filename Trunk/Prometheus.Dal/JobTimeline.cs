//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Prometheus.Dal
{
    using System;
    using System.Collections.Generic;
    
    public partial class JobTimeline
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public JobTimeline()
        {
            this.Block = new HashSet<Block>();
            this.BusinessFile = new HashSet<BusinessFile>();
            this.JobHistory = new HashSet<JobHistory>();
            this.Transaction = new HashSet<Transaction>();
        }
    
        public long Id { get; set; }
        public long ScheduleId { get; set; }
        public int JobStatusId { get; set; }
        public System.DateTime StartTime { get; set; }
        public string HangfireId { get; set; }
        public Nullable<long> NumberOfTransactions { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Block> Block { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<BusinessFile> BusinessFile { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<JobHistory> JobHistory { get; set; }
        public virtual JobStatus JobStatus { get; set; }
        public virtual Schedule Schedule { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Transaction> Transaction { get; set; }
    }
}
