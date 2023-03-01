using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NCrontab;
using ServiceLayer.Interface;
using System;
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

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                int value = WaitForNextCronValue();
                await Task.Delay(value, cancellationToken);

                await this.RunJobAsync();

                _logger.LogInformation("Daily cron jon ran successfully");
                _nextCron = _cron.GetNextOccurrence(DateTime.Now);
            }
        }

        private async Task RunJobAsync()
        {
            await RunDailyTimesheetCreationJob();

            await SendMail();

            await LeaveAccrualAsync();
        }

        #region WEEKLY TIMESHEET CREATION SCHEDULAR

        private async Task RunDailyTimesheetCreationJob()
        {
            if (DateTime.UtcNow.DayOfWeek == DayOfWeek.Saturday)
            {
                var service = _serviceProvider.GetRequiredService<ITimesheetService>();
                await service.RunWeeklyTimesheetCreation(DateTime.UtcNow.AddDays(1));
            }

            await Task.CompletedTask;
        }

        #endregion

        #region LEAVE ACCRUAL SCHEDULAR

        private async Task LeaveAccrualAsync()
        {
            using (IServiceScope scope = _serviceProvider.CreateScope())
            {
                ILeaveCalculation _leaveCalculation = scope.ServiceProvider.GetRequiredService<ILeaveCalculation>();
                await _leaveCalculation.StartAccrualCycle();
            }
        }

        #endregion

        #region PAYROLL SCHEDULAR

        #endregion

        #region DAILY EMAIL SCHEDULAR

        private async Task SendMail()
        {
            // _eMailManager.SendMail();
            await Task.CompletedTask;
        }

        #endregion

        #region ATTENDANCE APPROVAL LEVEL CHECK

        #endregion

        #region ATTENDANCE APPROVAL LEVEL CHECK

        #endregion

        #region ATTENDANCE APPROVAL LEVEL CHECK

        #endregion

        private int WaitForNextCronValue() => Math.Max(0, (int)_nextCron.Subtract(DateTime.Now).TotalMilliseconds);

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
        }
    }
}
