using System;
using System.Collections.Generic;

namespace ModalLayer.Modal
{
    public class TimesheetDetail
    {
        public long TimesheetId { set; get; } = 0;
        public long EmployeeId { get; set; }
        public long ClientId { get; set; }
        public int UserTypeId { get; set; }
        public string TimesheetMonthJson { get; set; }
        public int TotalDays { get; set; }
        public int DaysAbsent { get; set; }
        public int ExpectedBurnedMinutes { get; set; }
        public int ActualBurnedMinutes { get; set; }
        public int TotalWeekDays { get; set; }
        public int TotalWorkingDays { get; set; }
        public int TotalHolidays { get; set; }
        public int MonthTimesheetApprovalState { get; set; }
        public int ForYear { get; set; }
        public int ForMonth { get; set; }
        public DateTime TimesheetFromDate { get; set; }
        public DateTime TimesheetToDate { get; set; }
        public DateTime SubmittedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public long SubmittedBy { get; set; }
        public long? UpdatedBy { get; set; }
    }

    public class DailyTimesheetDetail
    {
        public long TimesheetId { set; get; }
        public long EmployeeId { get; set; }
        public long ClientId { get; set; }
        public int UserTypeId { get; set; }
        public decimal TotalMinutes { get; set; }
        public bool IsHoliday { get; set; }
        public bool IsWeekEnd { get; set; }
        public ItemStatus TimesheetStatus { get; set; }
        public DateTime PresentDate { get; set; }
        public string UserComments { get; set; }
        public string EmployeeName { set; get; }
        public string Email { set; get; }
        public string Mobile { set; get; }
        public long ReportingManagerId { set; get; }
        public string ManagerName { set; get; }
    }
}
