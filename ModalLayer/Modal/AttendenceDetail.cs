using System;
using System.Collections.Generic;

namespace ModalLayer.Modal
{
    public class AttendenceDetail
    {
        public long AttendanceId { set; get; } = 0;
        public long UserId { get; set; }
        public int UserTypeId { get; set; }
        public DateTime AttendanceDay { get; set; }
        public double BillingHours { get; set; }
        public double Hours { get; set; }
        public float IsHoliday { get; set; }
        public float IsWeekEnd { get; set; }
        public int AttendenceStatus { get; set; }
        public DateTime SubmittedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public DateTime? AttendenceForMonth { get; set; }
        public DateTime? AttendenceFromDay { get; set; }
        public DateTime? AttendenceToDay { get; set; }
        public long SubmittedBy { get; set; }
        public long UpdatedBy { get; set; }
        public string UserComments { get; set; }
        public long EmployeeUid { get; set; }
        public long ClientId { get; set; }
        public bool FirstWeek { get; set; }
        public bool SecondWeek { get; set; }
        public bool ThirdWeek { get; set; }
        public bool ForthWeek { get; set; }
        public bool FifthWeek { get; set; }
        public bool SixthWeek { get; set; }
    }

    public class AttendanceWithClientDetail
    {
        public List<AttendenceDetail> AttendacneDetails { set; get; }
        public AssignedClients Client { set; get; }
    }
}
