using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prometheus.BL.Interfaces
{
    public interface IJobService
    {
        Task ExecuteJob(long jobId);
        Task JobCleaner();
    }
}
