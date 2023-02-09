using ModalLayer.Modal.Leaves;
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
        public int TotalMinutes { get; set; } // total effective minutes
        public bool IsHoliday { get; set; }
        public int PresentDayStatus { get; set; } = (int)DayStatus.Empty;
        public int IsTimeAttendacneApproved { get; set; } = 0;
        public bool IsOnLeave { get; set; }
        public long LeaveId { set; get; }
        public int AttendenceStatus { get; set; }
        public string LogOn { get; set; } // HH:MM
        public string LogOff { get; set; } // HH:MM
        public int GrossMinutes { get; set; } // total gross minutes per day
        public int SessionType { get; set; } // 1 = full day, 2 = first half and 3 = second half
        public int LunchBreanInMinutes { get; set; } // 1 = full day, 2 = first half and 3 = second half
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
        public int CompanyId { get; set; }
        public bool IsWeekend { get; set; }
        public string Emails { get; set; }
        public List<string> EmailList { get; set; }
        public string EmployeeName { set; get; }
        public string Email { set; get; }
        public string Mobile { set; get; }
        public long ReportingManagerId { set; get; }
        public string ManagerName { set; get; }
    }

    public class AttendanceWithClientDetail
    {
        public List<AttendenceDetail> AttendacneDetails { set; get; }
        public Employee EmployeeDetail { set; get; }
    }

    public class AttendanceDetailBuildModal
    {
        public int attendanceSubmissionLimit { get; set; }
        public Attendance attendance { set; get; }
        public int SessionType { set; get; } = 1;
        public DateTime firstDate { set; get; }
        public DateTime presentDate { set; get; }
        public List<Calendar> calendars { set; get; }
        public ShiftDetail shiftDetail { set; get; }
        public LeaveDetail leaveDetail{ set; get; }
        public List<CompalintOrRequest> compalintOrRequests { set; get; }
    }
}
