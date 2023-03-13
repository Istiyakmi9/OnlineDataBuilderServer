using Microsoft.Extensions.DependencyInjection;
using ServiceLayer.Interface;
using System;
using System.Threading.Tasks;

namespace OnlineDataBuilder.HostedService.Services
{
    public class AttendanceApprovalLevelJob
    {
        public async static Task UpgradeRequestLevel(IServiceProvider _serviceProvider)
        {
            using (IServiceScope scope = _serviceProvider.CreateScope())
            {
                IAttendanceService _attendanceService = scope.ServiceProvider.GetRequiredService<IAttendanceService>();
                // await _leaveCalculation.StartAccrualCycle();
                await Task.CompletedTask;
            }
        }
    }
}
