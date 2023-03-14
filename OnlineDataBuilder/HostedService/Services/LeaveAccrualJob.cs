using Microsoft.Extensions.DependencyInjection;
using ServiceLayer.Interface;
using System;
using System.Threading.Tasks;

namespace OnlineDataBuilder.HostedService.Services
{
    public class LeaveAccrualJob
    {
        public async static Task LeaveAccrualAsync(IServiceProvider _serviceProvider)
        {
            using (IServiceScope scope = _serviceProvider.CreateScope())
            {
                int days = DateTime.DaysInMonth(DateTime.UtcNow.Year, DateTime.UtcNow.Month);
                if (DateTime.UtcNow.Day == days)
                {
                    ILeaveCalculation _leaveCalculation = scope.ServiceProvider.GetRequiredService<ILeaveCalculation>();
                    await _leaveCalculation.StartAccrualCycle();
                }
            }
        }
    }
}
