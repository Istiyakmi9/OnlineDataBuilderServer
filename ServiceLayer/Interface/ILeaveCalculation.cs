using ModalLayer.Modal.Leaves;
using System;
using System.Threading.Tasks;

namespace ServiceLayer.Interface
{
    public interface ILeaveCalculation
    {
        Task<LeaveCalculationModal> GetBalancedLeave(long EmployeeId, DateTime FromDate, DateTime ToDate);
        Task RunAccrualCycle(bool runTillMonthOfPresentYear = false);
        Task<LeaveCalculationModal> CheckAndApplyForLeave(LeaveRequestModal leaveRequestModal);
        Task<LeaveCalculationModal> GetLeaveDetailService(long EmployeeId);
        Task RunAccrualCycleByEmployee(long EmployeeId);
    }
}
