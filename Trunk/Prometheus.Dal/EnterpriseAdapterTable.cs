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
    
    public partial class EnterpriseAdapterTable
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public EnterpriseAdapterTable()
        {
            this.EnterpriseAdapterTableColumn = new HashSet<EnterpriseAdapterTableColumn>();
        }
    
        public long Id { get; set; }
        public string TableName { get; set; }
        public long EnterpriseAdapterId { get; set; }
    
        public virtual EnterpriseAdapter EnterpriseAdapter { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<EnterpriseAdapterTableColumn> EnterpriseAdapterTableColumn { get; set; }
    }
}
