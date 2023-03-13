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
                ILeaveCalculation _leaveCalculation = scope.ServiceProvider.GetRequiredService<ILeaveCalculation>();
                await _leaveCalculation.StartAccrualCycle();
            }
        }
    }
}
