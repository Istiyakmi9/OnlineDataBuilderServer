using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModalLayer.Modal;
using NCrontab;
using OnlineDataBuilder.HostedService.Services;
using ServiceLayer.Interface;
using System;
using System.Configuration;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace OnlineDataBuilder.HostedService
{
    public class DailyStartHourJob : IHostedService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<DailyStartHourJob> _logger;
        private readonly CrontabSchedule _cron;
        private readonly IServiceProvider _serviceProvider;
        private readonly ApplicationConfiguration _applicationConfiguration;
        private readonly FileLocationDetail _fileLocationDetail;

        private int counter = 0;
        private int index = 1;
        DateTime _nextCron;

        public DailyStartHourJob(ILogger<DailyStartHourJob> logger, 
            IConfiguration configuration, 
            IServiceProvider serviceProvider,
            ApplicationConfiguration applicationConfiguration,
            FileLocationDetail fileLocationDetail)
        {
            _logger = logger;
            _configuration = configuration;
            _fileLocationDetail = fileLocationDetail;
            _applicationConfiguration = applicationConfiguration;
            _serviceProvider = serviceProvider;
            _cron = CrontabSchedule.Parse(configuration.GetSection("DailyEarlyHourJob").Value,
                new CrontabSchedule.ParseOptions { IncludingSeconds = true });
            _nextCron = _cron.GetNextOccurrence(DateTime.Now);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            EnableLoggin();

            while (!cancellationToken.IsCancellationRequested)
            {
                int value = WaitForNextCronValue();
                await Task.Delay(value, cancellationToken);
                _logger.LogInformation($"Daily cron jon started. Index = {index}   ...............");

                await this.RunJobAsync();

                _logger.LogInformation($"Daily cron jon ran successfully. Index = {index++}   .................");
                _nextCron = _cron.GetNextOccurrence(DateTime.Now);
            }
        }

        private async Task RunJobAsync()
        {
            var companySettings = await LeaveAccrualJob.LeaveAccrualAsync(_serviceProvider);
            
            await WeeklyTimesheetCreationJob.RunDailyTimesheetCreationJob(_serviceProvider);

            await NotificationEmailJob.SendNotificationEmail(_serviceProvider);

            await AttendanceApprovalLevelJob.UpgradeRequestLevel(_serviceProvider, companySettings);

            await PayrollCycleJob.RunPayrollAsync(_serviceProvider, counter--);
        }

        private void EnableLoggin()
        {
            _applicationConfiguration.LoggingFilePath = _configuration.GetSection("ExceptionLoggingPath").Value;
            var flag = _configuration.GetSection("Logging:LogTransaction").Value;
            if (flag.ToLower() == "true")
                _applicationConfiguration.IsLoggingEnabled = true;
            else
                _applicationConfiguration.IsLoggingEnabled = false;

            _applicationConfiguration.LoggingFilePath = Path.Combine(
                _fileLocationDetail.RootPath,
            _applicationConfiguration.LoggingFilePath);

            if (!Directory.Exists(_applicationConfiguration.LoggingFilePath))
            {
                Directory.CreateDirectory(_applicationConfiguration.LoggingFilePath);
            }
        }

        private int WaitForNextCronValue() => Math.Max(0, (int)_nextCron.Subtract(DateTime.Now).TotalMilliseconds);

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
        }
    }
}
