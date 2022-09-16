using ModalLayer.Modal.Leaves;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServiceLayer.Interface
{
    public interface ILeaveCalculation
    {
        Task<LeaveCalculationModal> GetBalancedLeave(long EmployeeId, DateTime FromDate, DateTime ToDate);
        Task CheckAndApplyForLeave(long EmployeeId, int LeaveTypeId, DateTime FromDate, DateTime ToDate);
        Task<bool> CanApplyForHalfDay(long EmployeeId);
        void RunLeaveCalculationCycle();
    }
}
