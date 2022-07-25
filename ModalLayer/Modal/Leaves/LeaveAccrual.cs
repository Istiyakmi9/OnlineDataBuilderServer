using System;
using System.Collections.Generic;
using System.Text;

namespace ModalLayer.Modal.Leaves
{
    public class LeaveAccrual
    {
        public int LeaveAccrualId { get; set; }
        public int LeavePlanTypeId { get; set; }
        public bool CanApplyEntireLeave { get; set; }
        public bool IsLeaveAccruedPatternAvail { get; set; }
        public string LeaveDistributionSequence { get; set; }
        public int LeaveDistributionAppliedFrom { get; set; }
        public bool IsAllowLeavesForJoinigMonth { get; set; }
        public bool IsAllowLeavesProbationPeriod { get; set; }
        public int BreakMonthLeaveAllocationId { get; set; }
        public bool IsNoLeaveOnProbationPeriod { get; set; }
        public bool IsVaryOnProbationOrExprience { get; set; }
        public bool IsImpactedOnWorkDaysEveryMonth { get; set; }
        public int WeekOffAsAbsentIfAttendaceLessThen { get; set; }
        public int HolidayAsAbsentIfAttendaceLessThen { get; set; }
        public bool CanApplyForFutureDate { get; set; }
        public bool ExtraLeaveBeyondAccruedBalance { get; set; }
        public int NoOfDaysForExtraLeave { get; set; }
        public int AllowOnlyIfAccrueBalanceIsAlleast { get; set; }
        public int NotAllowIfAlreadyOnLeaveMoreThan { get; set; }
        public bool RoundOffLeaveBalance { get; set; }
        public bool ToNearestHalfDay { get; set; }
        public bool ToNearestFullDay { get; set; }
        public bool ToNextAvailableHalfDay { get; set; }
        public bool ToNextAvailableFullDay { get; set; }
        public bool ToPreviousHalfDay { get; set; }
        public bool DoesLeaveExpireAfterSomeTime { get; set; }
        public int AfterHowManyDays { get; set; }
    }

    public class Allocate_leave_break_for_month
    {
    public int BreakMonthLeaveAllocationId { get; set; }
    public int LeavePlanId { get; set; }
    public int LeavePlanDetailId { get; set; }
    public int FromDate { get; set; }
    public int ToDate { get; set; }
    public int AllocatedLeave { get; set; }
    }
}
