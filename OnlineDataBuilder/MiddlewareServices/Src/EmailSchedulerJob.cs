using EMailService.Service;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModalLayer.Modal;
using NCrontab;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OnlineDataBuilder.MiddlewareServices.Src
{
    public class EmailSchedulerJob : IHostedService
    {
        private readonly ILogger<EmailSchedulerJob> _logger;
        private readonly IEMailManager _eMailManager;
        private readonly CrontabSchedule _cron;
        DateTime _nextCron;

        public EmailSchedulerJob(ILogger<EmailSchedulerJob> logger, IConfiguration configuration, IEMailManager eMailManager)
        {
            _logger = logger;
            _cron = CrontabSchedule.Parse(configuration.GetSection("CronSetting").Value, new CrontabSchedule.ParseOptions { IncludingSeconds = true });
            _nextCron = _cron.GetNextOccurrence(DateTime.Now);
            _eMailManager = eMailManager;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Run(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    int value = WaitForNextCronValue();
                    await Task.Delay(value, cancellationToken);

                    SendMail();
                    
                    _logger.LogInformation("Working cron service");
                    _nextCron = _cron.GetNextOccurrence(DateTime.Now);
                }
            });

            return Task.CompletedTask;
        }

        private void SendMail()
        {
            EmailSenderModal emailSenderModal = new EmailSenderModal
            {
                BCC = null,
                CC = null,
                FileDetails = null,
                From = "info@bottomhalf.in",
                Subject = "Testing",
                Title = "Test",
                To = new List<string> { "istiyaq.4game@gmail.com" },
                UserName = null
            };

            _eMailManager.SendMail(emailSenderModal);
        }

        private int WaitForNextCronValue() => Math.Max(0, (int)_nextCron.Subtract(DateTime.Now).TotalMilliseconds);

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}
