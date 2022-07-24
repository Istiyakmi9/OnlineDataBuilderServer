using System;
using System.Collections.Generic;
using System.Text;

namespace ModalLayer.Modal.Leaves
{
    public class LeavePlanRestriction
    {
        public int LeavePlanRestrictionId { get; set; }
        public int LeavePlanId { get; set; }
        public bool NewJoineeCanApplyLeave { get; set; }
        public int DaysAfterInProbation { get; set; }
        public int DaysAfterJoining { get; set; }
        public bool LimitDaysLeaveInProbation { get; set; }
        public bool IsConsecutiveLeaveLimit { get; set; }
        public int ConsecutiveDaysLimit { get; set; }
        public bool IsLeaveInNoticeExtendsNoticePeriod { get; set; }
        public int NoOfTimesNoticePeriodExtended { get; set; }
        public bool CanManageOverrideLeaveRestriction { get; set; }
        public int GapBetweenTwoConsicutiveLeaveDates { get; set; }
        public int LimitOfMaximumLeavesInCalendarMonth { get; set; }
        public int LimitOfMaximumLeavesInCalendarYear { get; set; }
        public int LimitOfMaximumLeavesInEntireTenure { get; set; }
        public bool IsLeaveRestrictionForEachMonth { get; set; }
        public int RestrictFromDayOfMonth { get; set; }
        public int RestrictToDayOfMonth { get; set; }
        public int CurrentLeaveCannotCombineWith { get; set; }
        public int CurrentLeaveCannotIfBalanceInPlan { get; set; }
    }
}
