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
    
    public partial class AdapterMapping
    {
        public int Id { get; set; }
        public int From { get; set; }
        public int To { get; set; }
    
        public virtual AdapterTypeItem AdapterTypeItem { get; set; }
        public virtual AdapterTypeItem AdapterTypeItem1 { get; set; }
    }
}
