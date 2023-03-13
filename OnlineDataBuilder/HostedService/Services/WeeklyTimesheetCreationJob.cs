using Microsoft.Extensions.DependencyInjection;
using ServiceLayer.Interface;
using System;
using System.Threading.Tasks;

namespace OnlineDataBuilder.HostedService.Services
{
    public class WeeklyTimesheetCreationJob
    {
        public async static Task RunDailyTimesheetCreationJob(IServiceProvider _serviceProvider)
        {
            if (DateTime.UtcNow.DayOfWeek == DayOfWeek.Saturday)
            {
                var service = _serviceProvider.GetRequiredService<ITimesheetService>();
                await service.RunWeeklyTimesheetCreation(DateTime.UtcNow.AddDays(2));
            }

            await Task.CompletedTask;
        }
    }
}
