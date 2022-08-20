using ModalLayer.Modal;
using ModalLayer.Modal.Leaves;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServiceLayer.Interface
{
    public interface ILeaveCalculation
    {
        Task<List<LeavePlanType>> GetBalancedLeave(long EmployeeId, DateTime FromDate, DateTime ToDate);
        Task<bool> CanApplyForHalfDay(long EmployeeId, DateTime FromDate, DateTime ToDate);
        Task<bool> AllowToSeeAndApply(long EmployeeId, DateTime FromDate, DateTime ToDate);
        Task<bool> CanApplyBackDatedLeave(long EmployeeId, DateTime FromDate, DateTime ToDate);
    }
}
