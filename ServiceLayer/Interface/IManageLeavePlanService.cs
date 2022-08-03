using ModalLayer.Modal.Leaves;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceLayer.Interface
{
    public interface IManageLeavePlanService
    {
        LeavePlanConfiguration UpdateLeaveAccrual(int leavePlanTypeId, LeaveAccrual leaveAccrual);
        LeavePlanConfiguration UpdateLeaveDetail(int leavePlanTypeId, int leavePlanId, LeaveDetail leaveDetail);
        LeavePlanConfiguration UpdateLeaveAccrualService(int leavePlanTypeId, int leavePlanId, LeaveAccrual leaveAccrual);
        LeavePlanConfiguration UpdateApplyForLeaveService(int leavePlanTypeId, int leavePlanId, LeaveApplyDetail leaveApplyDetail);
        LeavePlanConfiguration UpdateLeaveRestrictionService(int leavePlanTypeId, int leavePlanId, LeavePlanRestriction leavePlanRestriction);
        LeavePlanConfiguration UpdateHolidayNWeekOffPlanService(int leavePlanTypeId, int leavePlanId, LeaveHolidaysAndWeekoff leaveHolidaysAndWeekoff);        
        LeavePlanConfiguration UpdateYearEndProcessingService(int leavePlanTypeId, int leavePlanId, LeaveEndYearProcessing leaveEndYearProcessing);
        LeavePlanConfiguration UpdateLeaveApprovalService(int leavePlanTypeId, int leavePlanId, LeaveApproval leaveApproval);        
        LeavePlanConfiguration GetLeaveConfigurationDetail(int leavePlanTypeId);
        string AddUpdateEmpLeavePlanService(int leavePlanId, List<EmpLeavePlanMapping> empLeavePlanMapping);
        List<EmpLeavePlanMapping> GetEmpMappingByLeavePlanIdService(int leavePlanId);
    }
}
