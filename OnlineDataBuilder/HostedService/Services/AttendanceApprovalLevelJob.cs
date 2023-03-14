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
                ILeaveRequestService _leaveRequestService = scope.ServiceProvider.GetRequiredService<ILeaveRequestService>();
                await _leaveRequestService.LeaveLeaveManagerMigration();
            }
        }
    }
}
