using System;
using System.Collections.Generic;
using System.Text;

namespace ModalLayer.Modal.Leaves
{
    public class LeaveEndYearProcessing
    {
        public int LeaveEndYearProcessingId { get; set; }
        public int LeavePlanId { get; set; }
        public bool IsLeaveBalanceExpiredOnEndOfYear { get; set; }
        public bool AllConvertedToPaid { get; set; }
        public bool AllLeavesCarryForwardToNextYear { get; set; }
        public bool PayFirstNCarryForwordRemaning { get; set; }
        public bool CarryForwordFirstNPayRemaning { get; set; }
        public bool PayNCarryForwardForFixedDays { get; set; }
        public bool PayNCarryForwardForPercent { get; set; }
        public int PayNCarryForwardIfDaysBalance { get; set; }
        public decimal PayPercent { get; set; }
        public decimal CarryForwardPercent { get; set; }
        public bool IsMaximumPayableRequired { get; set; }
        public decimal MaximumPayableDays { get; set; }
        public bool IsMaximumCarryForwardRequired { get; set; }
        public decimal MaximumCarryForwardDays { get; set; }
        public decimal RulesForLeaveBalanceIsMoreThan { get; set; }
        public decimal PaybleForDays { get; set; }
        public decimal CarryForwardForDays { get; set; }
        public bool DoestCarryForwardExpired { get; set; }
        public decimal ExpiredAfter { get; set; }
        public bool DoesNegativeLeaveHasImpact { get; set; }
        public bool DeductFromSalaryOnYearChange { get; set; }
        public bool ResetBalanceToZero { get; set; }
        public bool CarryForwardToNextYear { get; set; }
    }
}
