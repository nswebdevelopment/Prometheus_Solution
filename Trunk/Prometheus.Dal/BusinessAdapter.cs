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
    
    public partial class BusinessAdapter
    {
        public long Id { get; set; }
        public string Filename { get; set; }
        public long AdapterId { get; set; }
    
        public virtual Adapter Adapter { get; set; }
    }
}
