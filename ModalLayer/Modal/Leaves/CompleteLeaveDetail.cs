using System;
using System.Collections.Generic;

namespace ModalLayer.Modal.Leaves
{
    public class CompleteLeaveDetail
    {
        public string RecordId { set; get; }
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
        public List<RequestChainModal> RequestChain { set; get; }
        public DateTime RequestedOn { get; set; }
    }

    public class RequestChainModal
    {
        public long ExecuterId { set; get; }
        public int Status { set; get; }
        public DateTime ReactedOn { set; get; }
        public int Level { get; set; } 
        
        // level indicate approved, rejected etc by first, second etc person according to depth definde in the work flow
        public string FeedBack { get; set; }
    }
}
