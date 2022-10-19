using BottomhalfCore.DatabaseLayer.Common.Code;
using BottomhalfCore.Services.Code;
using BottomhalfCore.Services.Interface;
using ModalLayer.Modal;
using Newtonsoft.Json;
using ServiceLayer.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceLayer.Code
{
    public class TimesheetRequestService : ITimesheetRequestService
    {
        private readonly IDb _db;
        private readonly ITimezoneConverter _timezoneConverter;
        private readonly CurrentSession _currentSession;

        public TimesheetRequestService(IDb db, ITimezoneConverter timezoneConverter, CurrentSession currentSession)
        {
            _db = db;
            _timezoneConverter = timezoneConverter;
            _currentSession = currentSession;
        }

        public dynamic FetchPendingRequestService(long employeeId, int requestTypeId)
        {
            if (employeeId < 0)
                throw new HiringBellException("Invalid employee id.");

            string procedure = "sp_approval_requests_get";
            if (_currentSession.CurrentUserDetail.RoleId == (int)UserType.Admin)
                procedure = "sp_approval_requests_get_by_role";

            DateTime now = _timezoneConverter.ToSpecificTimezoneDateTime(_currentSession.TimeZone);
            var resultSet = _db.FetchDataSet(procedure, new
            {
                ManagerId = employeeId,
                StatusId = requestTypeId,
                ForYear = now.Year,
                ForMonth = now.Month
            });

            if (resultSet != null && resultSet.Tables.Count != 2)
                throw new HiringBellException("Fail to get approval request data for current user.");

            DataTable attendanceTable = null;
            if (resultSet.Tables[1].Rows.Count > 0)
                attendanceTable = resultSet.Tables[1];

            int i = 0;
            List<LeaveRequestNotification> approvalRequest = Converter.ToList<LeaveRequestNotification>(resultSet.Tables[0]);
            Parallel.For(i, approvalRequest.Count, x =>
            {
                approvalRequest.ElementAt(i).FromDate = _timezoneConverter.UpdateToUTCTimeZoneOnly(approvalRequest.ElementAt(i).FromDate);
                approvalRequest.ElementAt(i).ToDate = _timezoneConverter.UpdateToUTCTimeZoneOnly(approvalRequest.ElementAt(i).ToDate);
            });

            return new { ApprovalRequest = approvalRequest, AttendaceTable = attendanceTable };
        }

        public List<DailyTimesheetDetail> RejectTimesheetService(List<DailyTimesheetDetail> dailyTimesheetDetails)
        {
            return UpdateTimesheetRequest(dailyTimesheetDetails, ItemStatus.Rejected);
        }

        public List<DailyTimesheetDetail> ApprovalTimesheetService(List<DailyTimesheetDetail> dailyTimesheetDetails)
        {
            return UpdateTimesheetRequest(dailyTimesheetDetails, ItemStatus.Approved);
        }

        public List<DailyTimesheetDetail> UpdateTimesheetRequest(List<DailyTimesheetDetail> dailyTimesheetDetails, ItemStatus itemStatus)
        {
            var firstItem = dailyTimesheetDetails.FirstOrDefault();
            if (firstItem.TimesheetId <= 0)
                throw new HiringBellException("Invalid attendance day selected");

            var timesheet = _db.Get<TimesheetDetail>("sp_employee_timesheet_getby_id", new
            {
                TimesheetId = firstItem.TimesheetId
            });

            var allTimesheet = JsonConvert.DeserializeObject<List<DailyTimesheetDetail>>(timesheet.TimesheetMonthJson);
            foreach (var dailyTimesheet in dailyTimesheetDetails)
            {
                var currentTimesheet= allTimesheet.Find(x => x.PresentDate == dailyTimesheet.PresentDate);
                if (currentTimesheet != null)
                    currentTimesheet.TimesheetStatus = itemStatus;
            }
            var timesheetMonthJson = JsonConvert.SerializeObject(allTimesheet);
            timesheet.TimesheetMonthJson = timesheetMonthJson;
            // this call is used for only upadate AttendanceDetail json object
            var Result = _db.Execute<TimesheetDetail>("sp_attendance_update_request", new
            {
                timesheet.TimesheetId,
                timesheet.EmployeeId,
                timesheet.ClientId,
                timesheet.UserTypeId,
                timesheet.TimesheetMonthJson,
                timesheet.TotalDays,
                timesheet.DaysAbsent,
                timesheet.ExpectedBurnedMinutes,
                timesheet.ActualBurnedMinutes,
                timesheet.TotalWeekDays,
                timesheet.TotalWorkingDays,
                timesheet.TotalHolidays,
                timesheet.MonthTimesheetApprovalState,
                timesheet.ForYear,
                timesheet.ForMonth,
                AminId = _currentSession.CurrentUserDetail.UserId
            }, true);;
            if (string.IsNullOrEmpty(Result))
                throw new HiringBellException("Unable to update attendance status");

            var template = _db.Get<EmailTemplate>("sp_email_template_get", new { EmailTemplateId = 4 });
            // PrepareSendEmailNotification(currentAttendance, template);
            return this.FetchPendingRequestService(_currentSession.CurrentUserDetail.UserId, 0);
        }

        public List<DailyTimesheetDetail> ReAssigneTimesheetService(List<DailyTimesheetDetail> dailyTimesheetDetails)
        {
            return null;
        }
    }
}
