using System;

namespace ModalLayer.Modal
{
    public class Attendance : AttendanceDetailJson
    {
        public long AttendanceId { set; get; }
        public long EmployeeId { set; get; }
        public int UserTypeId { set; get; }
        public string AttendanceDetail { set; get; }
        public int TotalDays { set; get; }
        public int TotalWeekDays { set; get; }
        public int DaysPending { set; get; }
        public float TotalHoursBurend { set; get; }
        public float ExpectedHours { set; get; }
        public int ForYear { set; get; }
        public int ForMonth { set; get; }
        public string EmployeeName { set; get; }
        public string Email { set; get; }
        public string Mobile { set; get; }
        public long ReportingManagerId { set; get; }
        public string ManagerName { set; get; }
        public DateTime? SubmittedOn { set; get; }
        public DateTime? UpdatedOn { set; get; }
        public DateTime DOJ { set; get; }
        public int PendingRequestCount { set; get; }
        public long SubmittedBy { set; get; }
        public long? UpdatedBy { set; get; }
    }
}
