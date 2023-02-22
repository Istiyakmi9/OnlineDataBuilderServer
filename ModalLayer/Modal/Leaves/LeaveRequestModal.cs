using System;

namespace ModalLayer.Modal.Leaves
{
    public class LeaveRequestModal
    {
        public long EmployeeId { set; get; }
        public long AssigneId { set; get; }
        public string AssigneeEmail { set; get; }
        public int LeaveTypeId { set; get; }
        public string LeavePlanName { set; get; }
        public string Reason { set; get; }
        public string Session { set; get; }
        public DateTime LeaveFromDay { get; set; }
        public DateTime LeaveToDay { get; set; }
    }
}
