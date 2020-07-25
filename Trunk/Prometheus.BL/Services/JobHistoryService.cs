using Prometheus.Authorization.Data;
using Prometheus.BL.Interfaces;
using Prometheus.Common;
using Prometheus.Common.Enums;
using Prometheus.Dal;
using Prometheus.Dal.Entities;
using Prometheus.Model.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Prometheus.Authorization;

namespace Prometheus.BL.Services
{
    public class JobHistoryService : IJobHistoryService
    {
        private readonly IPrometheusEntities _entity;
        private readonly ILogger _logger;
        private readonly IUserHandler _user;

        public JobHistoryService(IPrometheusEntities entity, ILogger logger, IUserHandler user)
        {
            _entity = entity;
            _logger = logger;
            _user = user;
        }

        public IResponse<List<JobModel>> GetJobHistory(long Id)
        {
            var response = new Response<List<JobModel>>();

            try
            {
                var jobsHistory = new List<JobHistory>();

                jobsHistory = _entity.JobHistory.Where(jh => jh.JobTimelineId == Id).ToList();

                response.Value = jobsHistory.Select(j => new JobModel
                {
                    JobHistoryId = j.Id,
                    Message = j.Message,
                    UpdateTime = DateTime.SpecifyKind(j.UpdatedAt, DateTimeKind.Utc),
                    JobStatus = (Common.Enums.JobStatus)j.JobStatusId,
                    JobId = j.JobTimeline.Id,
                    ScheduleTitle = j.JobTimeline.Schedule.Title,
                    JobDefinitionName = j.JobTimeline.Schedule.JobDefinition.Name

                }).OrderByDescending(j => j.UpdateTime).ToList();

                response.Status = StatusEnum.Success;
            }
            catch (Exception ex)
            {
                response.Status = StatusEnum.Error;
                response.Message = Message.SomethingWentWrong;
                _logger.Information($"JobHistoryService.GetJobHistory(Id: {Id})");
                _logger.Error(ex.Message);
            }

            return response;
        }

        public IResponse<List<JobModel>> GetJobHistoryForUser(long userProfileId)
        {
            var response = new Response<List<JobModel>>();

            try
            {
                var jobTimeline = new List<JobTimeline>();
                var jobsHistory = new List<JobHistory>();
                var schedules = _entity.Schedule.Where(s => s.UserProfileId == userProfileId).ToList();

                foreach (var schedule in schedules)
                {
                    jobTimeline.AddRange(schedule.JobTimeline.ToList());
                }

                foreach (var job in jobTimeline)
                {
                    jobsHistory.AddRange(job.JobHistory.ToList());
                }

                response.Value = jobsHistory.Select(j => new JobModel
                {
                    JobHistoryId = j.Id,
                    Message = j.Message,
                    UpdateTime = DateTime.SpecifyKind(j.UpdatedAt, DateTimeKind.Utc),
                    JobStatus = (Common.Enums.JobStatus)j.JobStatusId,
                    JobId = j.JobTimeline.Id,
                    ScheduleTitle = j.JobTimeline.Schedule.Title,
                    JobDefinitionName = j.JobTimeline.Schedule.JobDefinition.Name

                }).OrderByDescending(j => j.UpdateTime).ToList();


                response.Status = StatusEnum.Success;
            }
            catch (Exception ex)
            {
                response.Status = StatusEnum.Error;
                response.Message = Message.SomethingWentWrong;
                _logger.Information($"JobHistoryService.GetJobHistoryForUser(userProfileId: {userProfileId})");
                _logger.Error(ex.Message);
            }

            return response;
        }

        public IResponse<NoValue> ChangeJobStatus(Common.Enums.JobStatus jobStatus, long Id, int transactionCount = default(int), int blockCount = default(int), bool limitExceeded = default(bool))
        {
            var response = new Response<NoValue>();

            try
            {
                var jobTimeline = _entity.JobTimeline.Where(jt => jt.Id == Id).FirstOrDefault();
                var adapterTypeFromId = jobTimeline.Schedule.JobDefinition.Adapter.AdapterTypeItem.AdapterTypeId;
                var adapterTypeToId = jobTimeline.Schedule.JobDefinition.Adapter1.AdapterTypeItem.AdapterTypeId;
                var user = _user.UserManager.Users.Where(u => u.UserProfileId == jobTimeline.Schedule.UserProfileId).FirstOrDefault();
                var userId = _user.UserManager.GetUserIdAsync(user).Result;

                string message = String.Empty;
                if (jobStatus == Common.Enums.JobStatus.Executing)
                {
                    _entity.Notification.Add(new Notification
                    {
                        CreatedAt = DateTime.Now,
                        StatusId = (int)Statuses.Unnotified,
                        UserProfileId = jobTimeline.Schedule.UserProfileId,
                        UserId = userId,
                        Message = "Job execution started"
                    });
                    jobTimeline.JobStatusId = (int)jobStatus;
                    message = "Your job is being executed";
                }
                else if (jobStatus == Common.Enums.JobStatus.Done)
                {
                    jobTimeline.JobStatusId = (int)jobStatus;

                    _entity.Notification.Add(new Notification
                    {
                        CreatedAt = DateTime.Now,
                        StatusId = (int)Statuses.Unnotified,
                        UserProfileId = jobTimeline.Schedule.UserProfileId,
                        UserId = userId,
                        Message = "Your job is done"
                    });

                    if (adapterTypeFromId != (int)Common.Enums.AdapterType.Crypto)
                    {
                        message = $"Your job is done; {transactionCount} transaction(s) transferred";
                    }
                    else if (adapterTypeToId == (int)Common.Enums.AdapterType.Business)
                    {
                        message = $"Your job is done; {blockCount} block(s) found with total of {transactionCount} transaction(s)";
                    }
                    else
                    {
                        message = $"Your job is done; {blockCount} block(s) transferred with total of {transactionCount} transaction(s)";
                    }

                }
                else if (jobStatus == Common.Enums.JobStatus.Failed)
                {
                    _entity.Notification.Add(new Notification
                    {
                        CreatedAt = DateTime.Now,
                        StatusId = (int)Statuses.Unnotified,
                        UserProfileId = jobTimeline.Schedule.UserProfileId,
                        UserId = userId,
                        Message = "Your job failed"
                    });

                    if (limitExceeded == true)
                    {
                        jobStatus = Common.Enums.JobStatus.Done;
                        jobTimeline.JobStatusId = (int)jobStatus;
                        message = "Block limit exceeded. Your job failed.";
                    }
                    else
                    {
                        jobStatus = Common.Enums.JobStatus.Done;
                        jobTimeline.JobStatusId = (int)jobStatus;
                        message = "Something went wrong. Your job failed.";
                    }
                }

                jobTimeline.JobHistory.Add(new JobHistory
                {
                    JobTimelineId = jobTimeline.Id,
                    UpdatedAt = DateTime.UtcNow,
                    JobStatusId = (int)jobStatus,
                    Message = message
                });

                _entity.SaveChanges();
                response.Status = StatusEnum.Success;
            }
            catch (Exception ex)
            {
                response.Status = StatusEnum.Error;
                response.Message = ex.Message;
                _logger.Information($"JobHistoryService.ChangeJobStatus(jobStatus: {jobStatus}, Id: {Id}, transactionCount: {transactionCount})");
                _logger.Error(ex.Message);
            }
            return response;
        }
    }
}
