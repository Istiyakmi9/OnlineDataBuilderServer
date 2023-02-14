using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NCrontab;
using ServiceLayer.Interface;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace OnlineDataBuilder.HostedService
{
    public class DailyStartHourJob : IHostedService
    {
        private readonly ILogger<LeaveAccrualSchedular> _logger;
        private readonly CrontabSchedule _cron;
        private readonly IServiceProvider _serviceProvider;
        DateTime _nextCron;

        public DailyStartHourJob(ILogger<LeaveAccrualSchedular> logger, IConfiguration configuration, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _cron = CrontabSchedule.Parse(configuration.GetSection("DailyEarlyHourJob").Value,
                new CrontabSchedule.ParseOptions { IncludingSeconds = true });
            _nextCron = _cron.GetNextOccurrence(DateTime.Now);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            this.RunJob();
            return Task.CompletedTask;
        }

        private void RunJob()
        {
            Task.Run(async () =>
            {
                await RunDailyTimesheetCreationJob();
            });
        }

        private async Task RunDailyTimesheetCreationJob()
        {
            if (DateTime.UtcNow.DayOfWeek == DayOfWeek.Sunday)
            {
                var service = _serviceProvider.GetRequiredService<ITimesheetService>();
                await service.RunWeeklyTimesheetCreation(DateTime.UtcNow.AddDays(1));
            }

            await Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
        }
    }
}
