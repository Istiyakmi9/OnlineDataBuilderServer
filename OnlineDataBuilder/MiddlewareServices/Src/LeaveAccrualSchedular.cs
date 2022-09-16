using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NCrontab;
using ServiceLayer.Interface;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OnlineDataBuilder.MiddlewareServices.Src
{
    public class LeaveAccrualSchedular : IHostedService
    {
        private readonly ILogger<LeaveAccrualSchedular> _logger;
        private readonly CrontabSchedule _cron;
        private readonly IServiceProvider _serviceProvider;
        DateTime _nextCron;

        public LeaveAccrualSchedular(ILogger<LeaveAccrualSchedular> logger, IConfiguration configuration, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _cron = CrontabSchedule.Parse(configuration.GetSection("LeaveSchedularCronSetting").Value, new CrontabSchedule.ParseOptions { IncludingSeconds = true });
            _nextCron = _cron.GetNextOccurrence(DateTime.Now);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Run(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    int value = WaitForNextCronValue();
                    await Task.Delay(value, cancellationToken);

                    RunAsync();

                    _logger.LogInformation("Leave accrual cycle run successfully");
                    _nextCron = _cron.GetNextOccurrence(DateTime.Now);
                }
            });

            return Task.CompletedTask;
        }

        private void RunAsync()
        {
            using (IServiceScope scope = _serviceProvider.CreateScope())
            {
                ILeaveCalculation _leaveCalculation = scope.ServiceProvider.GetRequiredService<ILeaveCalculation>();
                _leaveCalculation.RunLeaveCalculationCycle();
            }
        }

        private int WaitForNextCronValue() => Math.Max(0, (int)_nextCron.Subtract(DateTime.Now).TotalMilliseconds);

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}
