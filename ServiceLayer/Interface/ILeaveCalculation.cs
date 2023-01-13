using ModalLayer.Modal.Leaves;
using System;
using System.Threading.Tasks;

namespace ServiceLayer.Interface
{
    public interface ILeaveCalculation
    {
        Task<LeaveCalculationModal> GetBalancedLeave(long EmployeeId, DateTime FromDate, DateTime ToDate);
        Task<LeaveCalculationModal> CheckAndApplyForLeave(LeaveRequestModal leaveRequestModal);
    }
}
