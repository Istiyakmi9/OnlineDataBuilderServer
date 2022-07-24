using ModalLayer.Modal.Leaves;
using System.Collections.Generic;

namespace ServiceLayer.Interface
{
    public interface ILeaveService
    {
        List<LeavePlanType> GetLeavePlansService();
        string AddLeavePlanTypeService(LeavePlanType leavePlanType);
        List<LeavePlan> AddLeavePlansService(LeavePlan leavePlanType);
        string UpdateLeavePlanTypeService(int leavePlanTypeId, LeavePlanType leavePlanType);
        string AddUpdateLeaveQuotaService(LeaveDetail leaveDetail);
        List<LeavePlanType> GetLeaveTypeDetailByPlan(int leavePlanId);
        LeavePlanConfiguration GetLeaveTypeDetailByIdService(int leavePlanTypeId);
    }
}
