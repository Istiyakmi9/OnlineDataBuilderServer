using System;
using System.Collections.Generic;
using System.Text;

namespace ModalLayer.Modal
{
    public class ApprovalRequest
    {
        public long ApprovalRequestId { get; set; }
        public string UserName { get; set; }
        public string Message { get; set; }
        public long UserId { get; set; }
        public int UserTypeId { get; set; }
        public DateTime RequestedOn { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public long AssigneeId { get; set; }
        public long ProjectId { get; set; }
        public string ProjectName { get; set; }
        public int RequestStatusId { set; get; }
        public int RequestType { set; get; }
        public int LeaveType { set; get; }
        public long AttendanceId { set; get; }
        public int LeaveRequestId { set; get; }
    }
}
