using System;

namespace ModalLayer.Modal.Leaves
{
    public class LeaveCalculationModal
    {
        public DateTime fromDate { set; get; }
        public DateTime toDate { set; get; }
        public Employee employee { set; get; }
        public LeaveRequestDetail leaveRequestDetail { set; get; }
        public int employeeType { set; get; }
        public decimal totalNumOfLeaveApplied { set; get; }
        public CompleteLeaveDetail lastApprovedLeaveDetail { set; get; }
    }
}
