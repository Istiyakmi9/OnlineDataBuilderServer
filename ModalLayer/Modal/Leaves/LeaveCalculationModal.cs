using ModalLayer.Modal.Accounts;
using System;
using System.Collections.Generic;

namespace ModalLayer.Modal.Leaves
{
    public class LeaveCalculationModal
    {
        public DateTime fromDate { set; get; }
        public DateTime toDate { set; get; }
        public DateTime utcFromDate { set; get; }
        public DateTime utcToDate { set; get; }
        public DateTime utcPresentDate { set; get; }
        public DateTime presentDate { set; get; }
        public DateTime probationEndDate { set; get; }
        public DateTime noticePeriodEndDate { set; get; }
        public Employee employee { set; get; }
        public LeavePlan leavePlan { set; get; }
        public List<LeavePlan> leavePlans { set; get; }
        public LeavePlanConfiguration leavePlanConfiguration { set; get; }
        public LeaveRequestDetail leaveRequestDetail { set; get; }
        public CompanySetting companySetting { set; get; }
        public List<LeavePlanType> leavePlanTypes { set; get; }
        public int employeeType { set; get; }
        public bool isApplyingForHalfDay { set; get; }
        public long AssigneId { get; set; }
        public bool IsEmailNotificationPasued { get; set; }
        public bool IsLeaveAutoApproval { get; set; }
        public decimal numberOfLeaveApplyring { set; get; }
        public CompleteLeaveDetail lastApprovedLeaveDetail { set; get; }
    }
}
