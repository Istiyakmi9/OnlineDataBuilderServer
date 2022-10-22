using ModalLayer.Modal;
using System;
using System.Collections.Generic;

namespace ModalLayer
{
    public class EmployeeNotificationModel : EmailSenderModal
    {
        public string DeveloperName { set; get; }
        public string AttendanceRequestType { set; get; }
        public string LeaveType { set; get; }
        public string FromDate { set; get; }
        public string ToDate { set; get; }
        public string ManagerName { set; get; }
        public string Message { set; get; }
        public string CompanyName { set; get; }
        public string BodyContent { set; get; }
        public string ApprovalType { set; get; } // e.g. Approved, Rejected, Canceled etc.
    }
}
