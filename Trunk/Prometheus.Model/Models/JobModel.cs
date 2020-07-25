using Prometheus.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prometheus.Model.Models
{
    public class JobModel
    {
        public long JobId { get; set; }
        public DateTime StartTime { get; set; }
        public string JobDefinitionName { get; set; }
        public string ScheduleTitle { get; set; }
        public JobStatus JobStatus { get; set; }
        public long? TransactionCount { get; set; }
        public bool BusinessFile { get; set; }

        //JobHistory Fields
        public long JobHistoryId { get; set; }
        public DateTime UpdateTime { get; set; }
        public string Message { get; set; }
    }
}
