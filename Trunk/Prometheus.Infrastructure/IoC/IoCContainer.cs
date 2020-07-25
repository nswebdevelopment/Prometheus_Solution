using Microsoft.Extensions.DependencyInjection;
using Prometheus.ExchangesApi.Uow;
using Prometheus.BL.Interfaces;
using Prometheus.BL.Services;
using Prometheus.Dal;
using Prometheus.Dal.Entities;
using Prometheus.ExchangesApi.ExchangesData;
using Prometheus.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Prometheus.Media.SocialMedia;

namespace Prometheus.Infrastructure.IoC
{
    public class IoCContainer
    {
        public static void Configure(IServiceCollection services)
        {
            services.AddScoped<IExchangesData, ExchangesData>();

            services.AddScoped<IDataClient, DataClient>();

            services.AddScoped<IPrometheusEntities, PrometheusEntities>();

            services.AddScoped<IRssMedia, RssMedia>()
                    .AddScoped(x => new Lazy<IRssMedia>(() => x.GetRequiredService<IRssMedia>()));
            services.AddScoped<IFacebookClient, FacebookClient>();

            //services
            services.AddScoped<IScheduleService, ScheduleService>();
            services.AddScoped<IJobDefinitionService, JobDefinitionService>();
            services.AddScoped<IExchangesService, ExchangesService>();
            services.AddScoped<IUserProfileService, UserProfileService>();
            services.AddScoped<IEnterpriseAdapterService, EnterpriseAdapterService>();
            services.AddScoped<IFacebookService, FacebookService>();
            services.AddScoped<ISelectListService, SelectListService>();
            services.AddScoped<IAdapterService, AdapterService>();
            services.AddScoped<ICryptoAdapterService, CryptoAdapterService>();
            services.AddScoped<ITransactionService, TransactionService>();
            services.AddScoped<IBlockchainService, BlockchainService>();
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IJobTimelineService, JobTimelineService>();
            services.AddScoped<IJobHistoryService, JobHistoryService>();
            services.AddScoped<IJobService, JobService>();
            services.AddScoped<IDbAdapterService, DbAdapterService>();
            services.AddScoped<IBlockTransactionService, BlockTransactionService>();
            services.AddScoped<IBusinessAdapterService, BusinessAdapterService>();
            services.AddScoped<IDashboardService, DashboardService>();
            services.AddScoped<INotificationService, NotificationService>();
        }
    }
}
