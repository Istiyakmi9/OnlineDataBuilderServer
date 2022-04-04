using System;

namespace ModalLayer.Modal
{
    public class Attendance
    {
        public long AttendanceId { set; get; }
        public long EmployeeId { set; get; }
        public int UserTypeId { set; get; }
        public string AttendanceDetail { set; get; }
        public bool FirstWeek { set; get; }
        public bool SecondWeek { set; get; }
        public bool ThirdWeek { set; get; }
        public bool ForthWeek { set; get; }
        public bool FifthWeek { set; get; }
        public bool SixthWeek { set; get; }
        public float TotalHoursBurend { set; get; }
        public float ExpectedHours { set; get; }
        public DateTime AttendanceForMonth { set; get; }
        public DateTime? SubmittedOn { set; get; }
        public DateTime? UpdatedOn { set; get; }
        public long SubmittedBy { set; get; }
        public long? UpdatedBy { set; get; }
    }
}
