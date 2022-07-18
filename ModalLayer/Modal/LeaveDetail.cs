using System;
using System.Collections.Generic;
using System.Text;

namespace ModalLayer.Modal
{
    public class LeaveDetails
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
        public string LeaveDetail { get; set; }
        public long LeaveRequestId { set; get; }

    }
    public class Leave
    {
        public long LeaveRequestId { set; get; }
        public long EmployeeId { set; get; }
        public string LeaveDetail { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }
    public class CompleteLeaveDetail
    {
        public long EmployeeId { set; get; }
        public string EmployeeName { get; set; }
        public long ProjectId { set; get; }
        public long AssignTo { get; set; }
        public int LeaveType { get; set; }
        public string Session { get; set; }
        public DateTime LeaveFromDay { get; set; }
        public DateTime LeaveToDay { get; set; }
        public int LeaveStatus { set; get; }
        public int RespondedBy { set; get; }
        public DateTime UpdatedOn { set; get; }
        public string Reason { get; set; }
        public DateTime RequestedOn { get; set; }
    }
}
