using System;

namespace ModalLayer.Modal.Leaves
{
    public class CompleteLeaveDetail
    {
        public long EmployeeId { set; get; }
        public string EmployeeName { get; set; }
        public long ProjectId { set; get; }
        public long AssignTo { get; set; }
        public int LeaveTypeId { get; set; }
        public string LeaveTypeName { get; set; }
        public decimal NumOfDays { get; set; }
        public int Session { get; set; }
        public DateTime LeaveFromDay { get; set; }
        public DateTime LeaveToDay { get; set; }
        public int LeaveStatus { set; get; }
        public long RespondedBy { set; get; }
        public DateTime UpdatedOn { set; get; }
        public string Reason { get; set; }
        public string FeedBack { get; set; }
        public int LeavePlanId { get; set; }
        public DateTime RequestedOn { get; set; }
    }
}
