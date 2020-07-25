using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Prometheus.BL.Interfaces;
using Prometheus.Authorization.Data;
using Prometheus.Web.Services;
using Prometheus.Infrastructure.IoC;
using Prometheus.Authorization;
using Serilog;
using Microsoft.Extensions.Logging;
using Hangfire;
using Hangfire.SqlServer;
using Prometheus.Model;
using AutoMapper;
using Prometheus.Common.Hubs;

namespace Prometheus.Web
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                 .SetBasePath(env.ContentRootPath)
                 .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                 .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("Identity")));


            services.AddIdentity<ApplicationUser, IdentityRole<long>>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.Configure<IdentityOptions>(options =>
            {
                // Password settings
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 6;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;

                // User settings
                options.SignIn.RequireConfirmedEmail = true;
            });

            IoCContainer.Configure(services);

            services.AddSingleton<Serilog.ILogger>
                (x => new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.File(".\\Logs\\Prometheus.txt", rollingInterval: RollingInterval.Month)
                .CreateLogger());

            services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, AppClaimsPrincipalFactory>();
            services.AddTransient<IEmailSender, EmailSender>();
            services.AddScoped<IUserHandler, UserHandler>();

            //Hangfire
            services.AddHangfire(config => config.UseSqlServerStorage(Configuration.GetConnectionString("Hangfire")));

            services.AddAutoMapper(x => x.AddProfile(new MappingEntity()));

            services.AddMvc();

            services.AddOptions();
            services.Configure<AdapterSeed>(Configuration.GetSection("AdapterSeed"));
            services.Configure<RSSUrlSeed>(Configuration.GetSection("RSSUrlSeed"));

            services.AddSignalR();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, ITransactionService transactionService, INotificationService notificationService, IJobService jobService)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            loggerFactory.AddSerilog();

            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseHangfireDashboard();
            app.UseHangfireServer();

            app.UseSignalR(routes =>
            {
                routes.MapHub<NotificationHub>("/notificationHub");
            });
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });


            RecurringJob.AddOrUpdate("1", () => transactionService.CheckTransactionsStatus(), Cron.Hourly());

            RecurringJob.AddOrUpdate("2", () => jobService.JobCleaner(), Cron.Daily());

            notificationService.RegisterNotification();

        }
    }
}
