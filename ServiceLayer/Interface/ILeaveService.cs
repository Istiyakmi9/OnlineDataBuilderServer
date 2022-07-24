using ModalLayer.Modal.Leaves;
using System.Collections.Generic;

namespace ServiceLayer.Interface
{
    public interface ILeaveService
    {
        List<LeavePlan> GetLeavePlansService();
        List<LeavePlanType> AddLeavePlanTypeService(LeavePlanType leavePlanType);
        List<LeavePlan> AddLeavePlansService(LeavePlan leavePlanType);
        LeavePlan LeavePlanUpdateTypes(int leavePlanId, List<LeavePlanType> leavePlanTypes);
        List<LeavePlanType> UpdateLeavePlanTypeService(int leavePlanTypeId, LeavePlanType leavePlanType);
        string AddUpdateLeaveQuotaService(LeaveDetail leaveDetail);
        LeavePlanConfiguration GetLeaveTypeDetailByIdService(int leavePlanTypeId);
        List<LeavePlanType> GetLeaveTypeFilterService();
    }
}
