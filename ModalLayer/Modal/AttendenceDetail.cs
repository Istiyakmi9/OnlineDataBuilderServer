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
        public int TotalMinutes { get; set; }
        public bool IsHoliday { get; set; }
        public int PresentDayStatus { get; set; } = (int)DayStatus.WorkFromOffice;
        public bool IsTimeAttendacneApproved { get; set; } = false;
        public bool IsOnLeave { get; set; }
        public long LeaveId { set; get; }
        public int AttendenceStatus { get; set; }
        public DateTime SubmittedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public int ForYear { get; set; }
        public int ForMonth { get; set; }
        public DateTime? AttendenceFromDay { get; set; }
        public DateTime? AttendenceToDay { get; set; }
        public long SubmittedBy { get; set; }
        public long UpdatedBy { get; set; }
        public string UserComments { get; set; }
        public long EmployeeUid { get; set; }
        public long ClientId { get; set; }
        public int TotalDays { get; set; }
        public int DaysPending { get; set; }
        public bool IsActiveDay { get; set; }
        public bool IsOpen { get; set; }
        public List<TimeSheet> ClientTimeSheet { set; get; }
    }

    public class TimeSheet
    {
        public long ClientId { set; get; }
        public string Comments { set; get; }
        public DateTime SubmittedOn { set; get; }
    }

    public class AttendanceWithClientDetail
    {
        public List<AttendenceDetail> AttendacneDetails { set; get; }
        public Employee EmployeeDetail { set; get; }
    }
}
