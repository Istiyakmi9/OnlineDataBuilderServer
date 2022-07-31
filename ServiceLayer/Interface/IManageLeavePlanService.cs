using ModalLayer.Modal.Leaves;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceLayer.Interface
{
    public interface IManageLeavePlanService
    {
        LeavePlanConfiguration UpdateLeaveAccrual(int leavePlanTypeId, LeaveAccrual leaveAccrual);
        LeavePlanConfiguration UpdateLeaveDetail(int leavePlanTypeId, LeaveDetail leaveDetail);
        LeavePlanConfiguration UpdateLeaveAccrualService(int leavePlanTypeId, LeaveAccrual leaveAccrual);
        LeavePlanConfiguration UpdateApplyForLeaveService(int leavePlanTypeId, LeaveApplyDetail leaveApplyDetail);
        LeavePlanConfiguration UpdateLeaveRestrictionService(int leavePlanTypeId, LeavePlanRestriction leavePlanRestriction);
        LeavePlanConfiguration UpdateHolidayNWeekOffPlanService(int leavePlanTypeId, LeaveHolidaysAndWeekoff leaveHolidaysAndWeekoff);        
        LeavePlanConfiguration UpdateYearEndProcessingService(int leavePlanTypeId, LeaveEndYearProcessing leaveEndYearProcessing);
        LeavePlanConfiguration UpdateLeaveApprovalService(int leavePlanTypeId, LeaveApproval leaveApproval);        
        LeavePlanConfiguration GetLeaveConfigurationDetail(int leavePlanTypeId);
        string AddUpdateEmpLeavePlanService(int leavePlanId, List<EmpLeavePlanMapping> empLeavePlanMapping);
        List<EmpLeavePlanMapping> GetEmpMappingByLeavePlanIdService(int leavePlanId);
    }
}
