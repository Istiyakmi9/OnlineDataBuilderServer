using System;
using System.Collections.Generic;
using System.Text;

namespace ModalLayer.Modal.Leaves
{
    public class LeaveApplyDetail
    {
        public int LeaveApplyDetailId { get; set; }
        public int LeavePlanId { get; set; }
        public bool IsAllowForHalfDay { get; set; }
        public bool EmployeeCanSeeAndApplyCurrentPlanLeave { get; set; }
        public int RemaningCalendarDayInNotice { get; set; }
        public int RequiredCalendarDaysForLeaveApply { get; set; }
        public int RemaningWorkingDaysInNotice { get; set; }
        public int ApplyPriorBeforeLeaveDate { get; set; }
        public int BackDateLeaveApplyNotBeyondDays { get; set; }
        public int RestrictBackDateLeaveApplyAfter { get; set; }
        public bool CurrentLeaveRequiredComments { get; set; }
        public bool ProofRequiredIfDaysExceeds { get; set; }
        public int NoOfDaysExceeded { get; set; }
    }
}
