using System.Collections.Generic;

namespace Prometheus.Model.Models
{
    public class DashboardModel
    {
        public int PendingJobsCount { get; set; }
        public int ExecutingJobsCount { get; set; }
        public int DoneJobsCount { get; set; }
        public int TransactionsCount { get; set; }
        
        public List<JobModel> RecentJobs { get; set; }

        public List<int> TransactionsChartData { get; set; }
        public List<int> JobsChartData { get; set; }

    }
}
