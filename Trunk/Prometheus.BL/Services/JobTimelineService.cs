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

namespace Prometheus.BL.Services
{
    public class JobTimelineService : IJobTimelineService
    {
        private readonly IPrometheusEntities _entity;
        private readonly ILogger _logger;

        public JobTimelineService(IPrometheusEntities entity, ILogger logger)
        {
            _entity = entity;
            _logger = logger;
        }
        public IResponse<List<JobModel>> GetJobTimeline(long userProfileId)
        {
            var response = new Response<List<JobModel>>();

            try
            {
                var jobs = new List<JobTimeline>();
                var schedules = _entity.Schedule.Where(j => j.UserProfileId == userProfileId).ToList();

                foreach (var schedule in schedules)
                {
                    jobs.AddRange(schedule.JobTimeline.ToList());
                }
                
                response.Value = jobs.Select(j => new JobModel
                {
                    JobId = j.Id,
                    StartTime = DateTime.SpecifyKind(j.StartTime, DateTimeKind.Utc),
                    ScheduleTitle = j.Schedule.Title,
                    JobDefinitionName = j.Schedule.JobDefinition.Name,
                    JobStatus = (Common.Enums.JobStatus)j.JobStatusId,
                    TransactionCount = j.NumberOfTransactions,
                    BusinessFile = j.BusinessFile.Any()
                }).OrderByDescending(j => j.StartTime).ToList();
                
                response.Status = StatusEnum.Success;
            }
            catch (Exception ex)
            {
                response.Status = StatusEnum.Error;
                response.Message = Message.SomethingWentWrong;
                _logger.Information($"JobTimelineService.GetJobTimeline(userProfileId: {userProfileId})");
                _logger.Error(ex.Message);
            }

            return response;
        }


        public IResponse<NoValue> WriteNumberOfTransactions(long jobId, int numberOfTransactions)
        {
            var response = new Response<NoValue>();
            try
            {
                var jobTimeline = _entity.JobTimeline.Find(jobId);
                jobTimeline.NumberOfTransactions = jobTimeline.NumberOfTransactions != null ? jobTimeline.NumberOfTransactions + numberOfTransactions : numberOfTransactions;
                _entity.SaveChanges();
            }
            catch(Exception ex)
            {
                _logger.Information($"JobTimelineService.WriteNumberOfTransactions(jobId: {jobId}, numberOfTransactions: {numberOfTransactions})");
                _logger.Error(ex.Message);
            }
            
            return response;
        } 
    }
}
