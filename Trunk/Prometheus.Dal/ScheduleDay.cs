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
    
    public partial class ScheduleDay
    {
        public long ScheduleId { get; set; }
        public short DayId { get; set; }
    
        public virtual Schedule Schedule { get; set; }
    }
}
