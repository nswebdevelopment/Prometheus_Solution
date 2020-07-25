using Prometheus.BL.Interfaces;
using Prometheus.Dal.Entities;
using Serilog;
using Prometheus.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Prometheus.Common;
using Prometheus.Common.Enums;
using Prometheus.Dal;

namespace Prometheus.BL.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IPrometheusEntities _entity;
        private readonly ILogger _logger;
        public DashboardService(IPrometheusEntities entity, ILogger logger)
        {
            _entity = entity;
            _logger = logger;
        }

        public IResponse<DashboardModel> GetDashboardData(long userProfileId)
        {
            var response = new Response<DashboardModel>();

            try
            {
                var jobTimelines = _entity.Schedule.Where(s => s.UserProfileId == userProfileId)?.SelectMany(s => s.JobTimeline).ToList();

                if (jobTimelines.Count != 0)
                {
                    var currentMonth = DateTime.UtcNow.Month;
                    var currentYear = DateTime.UtcNow.Year;

                    response.Value = new DashboardModel
                    {
                        PendingJobsCount = jobTimelines.Exists(jt => jt.JobStatusId == (int)Common.Enums.JobStatus.Pending) ? jobTimelines.Where(jt => jt.JobStatusId == (int)Common.Enums.JobStatus.Pending).Count() : default(int),
                        ExecutingJobsCount = jobTimelines.Exists(jt => jt.JobStatusId == (int)Common.Enums.JobStatus.Executing) ? jobTimelines.Where(jt => jt.JobStatusId == (int)Common.Enums.JobStatus.Executing).Count() : default(int),
                        DoneJobsCount = jobTimelines.Exists(jt => jt.JobStatusId == (int)Common.Enums.JobStatus.Done && jt.StartTime.Month == currentMonth && jt.StartTime.Year == currentYear) ? jobTimelines.Where(jt => jt.JobStatusId == (int)Common.Enums.JobStatus.Done).Count() : default(int),
                        TransactionsCount = jobTimelines.Where(jt => jt.StartTime.Month == currentMonth && jt.StartTime.Year == currentYear)?.Select(jt => jt.NumberOfTransactions) != null ? (int)jobTimelines.Select(jt => jt.NumberOfTransactions).Sum() : default(int)
                    };
                }
                else
                {
                    response.Value = new DashboardModel
                    {
                        PendingJobsCount = default(int),
                        ExecutingJobsCount = default(int),
                        DoneJobsCount = default(int),
                        TransactionsCount = default(int)
                    };
                }

                response.Value.RecentJobs = jobTimelines.OrderByDescending(jt => jt.StartTime).Take(5).Select(j => new JobModel
                {
                    StartTime = DateTime.SpecifyKind(j.StartTime, DateTimeKind.Utc),
                    ScheduleTitle = j.Schedule.Title,
                    JobDefinitionName = j.Schedule.JobDefinition.Name,
                    JobStatus = (Common.Enums.JobStatus)j.JobStatusId,
                }).ToList();

                var chartData = GetChartData(userProfileId, jobTimelines);

                response.Value.TransactionsChartData = chartData.Item1;
                response.Value.JobsChartData = chartData.Item2;

                response.Status = StatusEnum.Success;
            }
            catch (Exception ex)
            {
                response.Status = StatusEnum.Error;
                response.Message = Message.SomethingWentWrong;
                _logger.Information($"DashboardService.GetDashboardData(userProfileId: {userProfileId})");
                _logger.Error(ex.Message);
            }

            return response;
        }

        private Tuple<List<int>, List<int>> GetChartData(long userProfileId, List<JobTimeline> jobTimelines)
        {
            var month = DateTime.UtcNow.Month;
            var year = DateTime.UtcNow.Year;
            
            var transactions = new List<int>();
            var jobs = new List<int>();

            for (var i = 0; i < 6; i++)
            {
                if (month == 0)
                {
                    month = 12;
                    year--;
                }

                var transactionsNo = jobTimelines.Where(jt => jt.StartTime.Month == month && jt.StartTime.Year == year)?.Sum(j => j.NumberOfTransactions) ?? 0;
                var jobsNo = jobTimelines.Where(jt => jt.StartTime.Month == month && jt.StartTime.Year == year && jt.JobStatusId == (int)Common.Enums.JobStatus.Done).Count();

                transactions.Add((int)transactionsNo);
                jobs.Add(jobsNo);

                month--;
            }

            transactions.Reverse();
            jobs.Reverse();
            
            return Tuple.Create(transactions, jobs);
        }
    }
}
