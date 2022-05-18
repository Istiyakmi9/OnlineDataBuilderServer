using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NCrontab;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OnlineDataBuilder.MiddlewareServices.Src
{
    public class SchedulerJob : IHostedService
    {
        private readonly ILogger<SchedulerJob> _logger;
        private readonly CrontabSchedule _cron;
        DateTime _nextCron;

        public SchedulerJob(ILogger<SchedulerJob> logger, IConfiguration configuration)
        {
            _logger = logger;
            _cron = CrontabSchedule.Parse(configuration.GetSection("CronSetting").Value, new CrontabSchedule.ParseOptions { IncludingSeconds = true });
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

                    _logger.LogInformation("Working cron service");

                    _nextCron = _cron.GetNextOccurrence(DateTime.Now);
                }
            });

            return Task.CompletedTask;
        }

        private int WaitForNextCronValue() => Math.Max(0, (int)_nextCron.Subtract(DateTime.Now).TotalMilliseconds);

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}
