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
    
    public partial class Schedule
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Schedule()
        {
            this.JobTimeline = new HashSet<JobTimeline>();
        }
    
        public long Id { get; set; }
        public string Title { get; set; }
        public System.DateTime StartDate { get; set; }
        public System.DateTime EndDate { get; set; }
        public long UserProfileId { get; set; }
        public long JobDefinitionId { get; set; }
        public string RecurrenceRule { get; set; }
        public System.DateTime CreatedAt { get; set; }
        public string Description { get; set; }
    
        public virtual JobDefinition JobDefinition { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<JobTimeline> JobTimeline { get; set; }
        public virtual UserProfile UserProfile { get; set; }
    }
}
