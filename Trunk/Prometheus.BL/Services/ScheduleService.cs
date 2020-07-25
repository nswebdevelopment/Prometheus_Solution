using Prometheus.BL.Interfaces;
using Prometheus.Common;
using Prometheus.Dal;
using Prometheus.Dal.Entities;
using Prometheus.Model.Models.ScheduleModel;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using Hangfire;

namespace Prometheus.BL.Services
{
    public class ScheduleService : IScheduleService
    {
        private readonly IPrometheusEntities _entity;
        private readonly ILogger _logger;
        private readonly IJobService _jobService;

        public ScheduleService(IPrometheusEntities entity, ILogger logger, IJobService jobService)
        {
            _entity = entity;
            _logger = logger;
            _jobService = jobService;
        }

        public IResponse<NoValue> CreateSchedule(ScheduleModel model, IEnumerable<DateTime> dates)
        {
            var result = new Response<NoValue>();
            var jobs = new List<JobTimeline>();

            try
            {
                int counter = 0;

                if (model.StartDate < DateTime.Now)
                {
                    result.Status = Common.Enums.StatusEnum.Error;
                    result.Message = "Jobs cannot be created in the past";
                    return result;
                }

                var schedule = new Schedule
                {
                    Title = model.Title,
                    StartDate = model.StartDate.ToUniversalTime(),
                    EndDate = model.EndDate,
                    UserProfileId = model.UserProfileId,
                    JobDefinitionId = model.JobDefinitionId,
                    RecurrenceRule = model.RecurrenceRule,
                    CreatedAt = DateTime.UtcNow,
                    Description = model.Description
                };

                _entity.Schedule.Add(schedule);

                if (dates != null)
                {
                    var jobexecutiondates = dates.Select(d => new JobTimeline
                    {
                        ScheduleId = schedule.Id,
                        JobStatusId = (int)Common.Enums.JobStatus.Pending,
                        StartTime = d.ToUniversalTime()
                    }).ToList();

                    jobs = jobexecutiondates;
                    _entity.JobTimeline.AddRange(jobexecutiondates);

                    counter = jobexecutiondates.Count;

                    foreach (var jobExecutionDate in jobexecutiondates)
                    {
                        jobExecutionDate.JobHistory.Add(new JobHistory
                        {
                            JobTimelineId = jobExecutionDate.Id,
                            UpdatedAt = DateTime.UtcNow,
                            JobStatusId = (int)Common.Enums.JobStatus.Pending,
                            Message = "Your job is waiting for execution"
                        });
                    }
                }
                else
                {
                    var jobTimeline = _entity.JobTimeline.Add(new JobTimeline
                    {
                        ScheduleId = schedule.Id,
                        JobStatusId = (int)Common.Enums.JobStatus.Pending,
                        StartTime = schedule.StartDate.ToUniversalTime()
                    });

                    _entity.JobHistory.Add(new JobHistory
                    {
                        JobTimelineId = jobTimeline.Id,
                        UpdatedAt = DateTime.UtcNow,
                        JobStatusId = (int)Common.Enums.JobStatus.Pending,
                        Message = "Your job is waiting for execution"
                    });
                    jobs.Add(jobTimeline);
                    counter = 1;
                }

                _entity.SaveChanges();
                result.Message = $"{counter} job(s) scheduled successfully.";
                result.Status = Common.Enums.StatusEnum.Success;

                //sync jobs with Hangfire (needs to be refactored)
                foreach (var job in jobs)
                {
                    var idHangfire = BackgroundJob.Schedule(() => _jobService.ExecuteJob(job.Id), job.StartTime);
                    job.HangfireId = idHangfire;
                }

                _entity.SaveChanges();
            }
            catch (Exception ex)
            {
                result.Status = Common.Enums.StatusEnum.Error;
                result.Message = Message.SomethingWentWrong;
                _logger.Information($"ScheduleService.CreateSchedule(model: {model}, dates: {dates})");
                _logger.Error(ex.Message);
            }

            return result;
        }

        public IResponse<IEnumerable<ScheduleModel>> GetSchedule(long userProfileId, DateTime startTime, DateTime endTime)
        {
            var result = new Response<IEnumerable<ScheduleModel>>();

            try
            {
                var jobs = new List<ScheduleModel>();
                var jobTimelines = new List<JobTimeline>();
                var schedules = _entity.Schedule.Where(sch => sch.UserProfileId == userProfileId).ToList();

                foreach (var schedule in schedules)
                {
                    jobTimelines.AddRange(schedule.JobTimeline.ToList());
                }

                result.Value = jobTimelines.Where(jt => jt.StartTime >= startTime && jt.StartTime <= endTime).Select(jt => new ScheduleModel
                {
                    Id = jt.ScheduleId,
                    Title = jt.Schedule.Title,
                    StartDate = jt.StartTime.ToLocalTime(),
                    JobId = jt.Id
                }).ToList();

                result.Status = Common.Enums.StatusEnum.Success;
            }
            catch (Exception ex)
            {
                result.Status = Common.Enums.StatusEnum.Error;
                result.Message = Message.SomethingWentWrong;
                _logger.Information($"ScheduleService.GetSchedule(userProfileId: {userProfileId}, startTime: {startTime}, endTime: {endTime})");
                _logger.Error(ex.Message);
            }

            return result;
        }


        public IResponse<ScheduleModel> GetScheduleById(long scheduleId, long jobId)
        {
            var result = new Response<ScheduleModel>();

            try
            {
                var schedule = _entity.Schedule.Find(scheduleId);
                var scheduleModel = new ScheduleModel
                {
                    Id = schedule.Id,
                    Title = schedule.Title,
                    StartDate = schedule.StartDate.ToLocalTime(),
                    EndDate = schedule.EndDate,
                    RecurrenceRule = schedule.RecurrenceRule,
                    User = schedule.UserProfile.FirstName + " " + schedule.UserProfile.LastName,
                    Description = schedule.Description,
                    JobDefinitionId = schedule.JobDefinitionId,
                    JobId = jobId,

                    //check if at least one job has been started
                    JobsExecuted = schedule.JobTimeline.Any(jt => jt.JobStatusId != 2)
                };
                result.Value = scheduleModel;
                result.Status = Common.Enums.StatusEnum.Success;
            }
            catch (Exception ex)
            {
                result.Status = Common.Enums.StatusEnum.Error;
                result.Message = Message.SomethingWentWrong;
                _logger.Information($"ScheduleService.GetScheduleById(scheduleId: {scheduleId})");
                _logger.Error(ex.Message);
            }

            return result;
        }

        public IResponse<NoValue> EditSchedule(ScheduleModel model, IEnumerable<DateTime> dates)
        {
            var result = new Response<NoValue>();

            try
            {
                var schedule = _entity.Schedule.Find(model.Id);

                //check if at least one job has been started
                var jobsExecuted = schedule.JobTimeline.Any(jt => jt.JobStatusId != 2);

                if (jobsExecuted)
                {
                    result.Status = Common.Enums.StatusEnum.Error;
                    result.Message = "Schedule cannot be updated if at least one job has been started.";
                    return result;
                }

                foreach (var job in schedule.JobTimeline.ToList())
                {
                    //delete from Hangfire
                    var deletedHangfireJob = BackgroundJob.Delete(job.HangfireId);

                    _entity.JobHistory.RemoveRange(job.JobHistory);
                }
                _entity.JobTimeline.RemoveRange(schedule.JobTimeline);

                schedule.Title = model.Title;
                schedule.StartDate = model.StartDate.ToUniversalTime();
                schedule.EndDate = model.EndDate;
                schedule.UserProfileId = model.UserProfileId;
                schedule.JobDefinitionId = model.JobDefinitionId;
                schedule.RecurrenceRule = model.RecurrenceRule;
                schedule.Description = model.Description;
                schedule.CreatedAt = DateTime.UtcNow;

                if (dates != null)
                {
                    var jobexecutiondates = dates.Select(d => new JobTimeline
                    {
                        ScheduleId = schedule.Id,
                        JobStatusId = (int)Common.Enums.JobStatus.Pending,
                        StartTime = d.ToUniversalTime()
                    }).ToList();

                    _entity.JobTimeline.AddRange(jobexecutiondates);

                    foreach (var jobExecutionDate in jobexecutiondates)
                    {
                        jobExecutionDate.JobHistory.Add(new JobHistory
                        {
                            JobTimelineId = jobExecutionDate.Id,
                            UpdatedAt = DateTime.UtcNow,
                            JobStatusId = (int)Common.Enums.JobStatus.Pending,
                            Message = "Your job is waiting for execution"
                        });
                    }
                }
                else
                {
                    var job = new JobTimeline
                    {
                        ScheduleId = schedule.Id,
                        JobStatusId = (int)Common.Enums.JobStatus.Pending,
                        StartTime = schedule.StartDate.ToUniversalTime()
                    };
                    schedule.JobTimeline.Add(job);

                    _entity.JobHistory.Add(new JobHistory
                    {
                        JobTimelineId = job.Id,
                        UpdatedAt = DateTime.UtcNow,
                        JobStatusId = (int)Common.Enums.JobStatus.Pending,
                        Message = "Your job is waiting for execution"
                    });
                }

                _entity.SaveChanges();
                result.Message = "Schedule has been updated successfully.";
                result.Status = Common.Enums.StatusEnum.Success;

                //sync jobs with Hangfire (needs to be refactored)
                var jobs = _entity.JobTimeline.Where(jt => jt.ScheduleId == schedule.Id).ToList();

                foreach (var job in jobs)
                {
                    var idHangfire = BackgroundJob.Schedule(() => _jobService.ExecuteJob(job.Id), job.StartTime);
                    job.HangfireId = idHangfire;
                }

                _entity.SaveChanges();
            }
            catch (Exception ex)
            {
                result.Status = Common.Enums.StatusEnum.Error;
                result.Message = Message.SomethingWentWrong;
                _logger.Information($"ScheduleService.EditSchedule(model: {model}, dates: {dates})");
                _logger.Error(ex.Message);
            }

            return result;
        }

        public IResponse<NoValue> DeleteSchedule(long id)
        {
            var result = new Response<NoValue>();

            try
            {
                var schedule = _entity.Schedule.Find(id);
                var jobTimeline = schedule.JobTimeline.ToList();
                var jobTimelineToDelete = new List<JobTimeline>();

                foreach (var job in jobTimeline)
                {
                    if (job.StartTime > DateTime.UtcNow && job.JobStatusId == (int)Common.Enums.JobStatus.Pending)
                    {
                        //delete from Hangfire
                        var deleteHangfireJob = BackgroundJob.Delete(job.HangfireId);
                        _entity.JobHistory.RemoveRange(job.JobHistory);
                        jobTimelineToDelete.Add(job);
                    }
                }

                _entity.JobTimeline.RemoveRange(jobTimelineToDelete);

                if (schedule.JobTimeline.Count == 0)
                {
                    _entity.Schedule.Remove(schedule);
                }
                else if (schedule.JobTimeline.Count == 1)
                {
                    schedule.RecurrenceRule = null;
                }
                else
                {
                    int newCount = schedule.JobTimeline.Count;
                    int index = schedule.RecurrenceRule.LastIndexOf("=");
                    string recurrenceString = schedule.RecurrenceRule.Substring(0, index + 1);
                    recurrenceString = recurrenceString + newCount.ToString();
                    schedule.RecurrenceRule = recurrenceString;
                }

                _entity.SaveChanges();
                result.Message = "Job(s) deleted successfully. Note: If you have jobs that have already been started, they won't be deleted.";
                result.Status = Common.Enums.StatusEnum.Success;
            }
            catch (Exception ex)
            {
                result.Status = Common.Enums.StatusEnum.Error;
                result.Message = Message.SomethingWentWrong;

                _logger.Information($"ScheduleService.DeleteSchedule(id: {id})");
                _logger.Error(ex.Message);
            }

            return result;
        }


        public IResponse<NoValue> DeleteJob(long id)
        {
            var result = new Response<NoValue>();

            try
            {
                var jobTimeline = _entity.JobTimeline.Find(id);

                if (jobTimeline.JobStatusId == 2)
                {
                    var jobHistory = jobTimeline.JobHistory.ToList();
                    var schedule = jobTimeline.Schedule;

                    //delete from Hangfire
                    var deleteHangfireJob = BackgroundJob.Delete(jobTimeline.HangfireId);
                    _entity.JobHistory.RemoveRange(jobHistory);
                    _entity.JobTimeline.Remove(jobTimeline);

                    if (schedule.JobTimeline.Count == 0)
                    {
                        _entity.Schedule.Remove(schedule);
                    }
                    else if (schedule.JobTimeline.Count == 1)
                    {
                        schedule.RecurrenceRule = null;
                    }
                    else
                    {
                        int newCount = schedule.JobTimeline.Count;
                        int index = schedule.RecurrenceRule.LastIndexOf("=");
                        string recurrenceString = schedule.RecurrenceRule.Substring(0, index + 1);
                        recurrenceString = recurrenceString + newCount.ToString();
                        schedule.RecurrenceRule = recurrenceString;
                    }

                    _entity.SaveChanges();
                    result.Message = "Job deleted successfully.";
                    result.Status = Common.Enums.StatusEnum.Success;
                }
                else
                {
                    result.Status = Common.Enums.StatusEnum.Error;
                    result.Message = "Job that has already been started cannot be deleted.";
                }

            }
            catch (Exception ex)
            {
                result.Status = Common.Enums.StatusEnum.Error;
                result.Message = Message.SomethingWentWrong;

                _logger.Information($"ScheduleService.DeleteSchedule(id: {id})");
                _logger.Error(ex.Message);
            }

            return result;
        }
    }
}
