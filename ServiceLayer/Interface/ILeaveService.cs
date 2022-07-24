using ModalLayer.Modal.Leaves;
using System.Collections.Generic;

namespace ServiceLayer.Interface
{
    public interface ILeaveService
    {
        List<LeavePlan> GetLeavePlansService();
        List<LeavePlan> AddLeavePlansService(LeavePlan leavePlan);
        List<LeavePlan> UpdateLeavePlansService(int leavePlanId, LeavePlan leavePlan);
        string AddUpdateLeaveQuotaService(int leavePlanId, LeaveQuota leaveQuota);
    }
}
