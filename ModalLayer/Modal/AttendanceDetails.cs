using System;

namespace ModalLayer.Modal
{
    public class AttendanceDetails
    {
        public long EmployeeUid {set;get;}
        public string EmployeeName {set;get;}
        public string Email {set;get;}
        public string Mobile {set;get;}
        public long ReportingManagerId {set;get;}
        public string ManagerName { set;get;}
        public int TotalMinutes {set;get;}
        public int UserTypeId {set;get;}
        public int PresentDayStatus {set;get;}
        public long AttendanceId {set;get;}
        public string UserComments {set;get;}
        public DateTime AttendanceDay {set;get;}
        public int AttendenceStatus { set; get; }
    }
}
