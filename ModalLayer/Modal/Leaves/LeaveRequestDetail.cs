using System;
using System.Collections.Generic;

namespace ModalLayer.Modal.Leaves
{
    public class LeaveRequestDetail
    {
        public long EmployeeId { set; get; }
        public int UserTypeId { get; set; }
        public long LeaveId { set; get; }
        public DateTime LeaveFromDay { get; set; }
        public DateTime LeaveToDay { get; set; }
        public string Session { get; set; }
        public string Reason { get; set; }
        public string Notify { get; set; }
        public long AssignTo { get; set; }
        public int LeaveType { get; set; }
        public int RequestType { get; set; }
        public int Year { get; set; }
        public string LeaveDetail { get; set; }
        public long LeaveRequestId { set; get; }
        public decimal PresentMonthLeaveAccrualed { set; get; }
        public decimal AvailableLeaves { set; get; }
        public decimal TotalLeaveApplied { set; get; }
        public decimal TotalApprovedLeave { set; get; }
        public string LeaveQuotaDetail { set; get; }
        public decimal TotalLeaveQuota { set; get; }
        public int RequestStatusId { get; set; }
        public List<EmployeeLeaveQuota> EmployeeLeaveQuotaDetail { set; get; }

    }
}
