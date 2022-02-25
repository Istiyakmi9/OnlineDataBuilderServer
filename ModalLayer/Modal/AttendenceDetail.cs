using System;

namespace ModalLayer.Modal
{
    public class AttendenceDetail
    {
        public long AttendanceId { set; get; } = 0;
        public long UserId { get; set; }
        public int UserTypeId { get; set; }
        public DateTime AttendanceDay { get; set; }
        public double Hours { get; set; }
        public float IsHoliday { get; set; }
        public float IsWeekEnd { get; set; }
        public int AttendenceStatus { get; set; }
        public DateTime SubmittedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public DateTime? AttendenceFromDay { get; set; }
        public DateTime? AttendenceToDay { get; set; }
        public long SubmittedBy { get; set; }
        public long UpdatedBy { get; set; }
        public string UserComments { get; set; }
        public long EmployeeUid { get; set; }
    }
}
