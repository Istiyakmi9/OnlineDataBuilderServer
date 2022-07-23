using ModalLayer.Modal.Leaves;
using System.Collections.Generic;

namespace ServiceLayer.Interface
{
    public interface ILeaveService
    {
        List<LeavePlan> GetLeavePlansService();
        string AddLeavePlansService(LeavePlan leavePlan);
        string UpdateLeavePlansService(int leavePlanId, LeavePlan leavePlan);
        string AddUpdateLeaveQuotaService(int leavePlanId, LeaveQuota leaveQuota);
    }
}
