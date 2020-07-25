using Prometheus.Common;
using Prometheus.Model.Models.ScheduleModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prometheus.BL.Interfaces
{
    public interface IScheduleService
    {
        IResponse<NoValue> CreateSchedule(ScheduleModel model, IEnumerable<DateTime> dates);
        IResponse<IEnumerable<ScheduleModel>> GetSchedule(long userProfileId, DateTime startTime, DateTime endTime);
        IResponse<ScheduleModel> GetScheduleById(long scheduleId, long jobId);
        IResponse<NoValue> EditSchedule(ScheduleModel model, IEnumerable<DateTime> dates);
        IResponse<NoValue> DeleteSchedule(long id);
        IResponse<NoValue> DeleteJob(long id);
    }
}
