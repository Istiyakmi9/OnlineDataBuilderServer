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
        private readonly IAttendanceRequestService _attendanceRequestService;

        public TimesheetRequestService(IDb db, ITimezoneConverter timezoneConverter, CurrentSession currentSession, IAttendanceRequestService attendanceRequestService)
        {
            _db = db;
            _timezoneConverter = timezoneConverter;
            _currentSession = currentSession;
            _attendanceRequestService = attendanceRequestService;
        }

        public dynamic RejectTimesheetService(List<DailyTimesheetDetail> dailyTimesheetDetails)
        {
            return UpdateTimesheetRequest(dailyTimesheetDetails, ItemStatus.Rejected);
        }

        public dynamic ApprovalTimesheetService(List<DailyTimesheetDetail> dailyTimesheetDetails)
        {
            return UpdateTimesheetRequest(dailyTimesheetDetails, ItemStatus.Approved);
        }

        public dynamic UpdateTimesheetRequest(List<DailyTimesheetDetail> dailyTimesheetDetails, ItemStatus itemStatus)
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
            timesheet.TimesheetMonthJson = JsonConvert.SerializeObject(allTimesheet);

            // this call is used for only upadate AttendanceDetail json object
            var Result = _db.Execute<TimesheetDetail>("sp_timesheet_insupd", new
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
                AdminId = _currentSession.CurrentUserDetail.UserId
            }, true);
            if (string.IsNullOrEmpty(Result))
                throw new HiringBellException("Unable to update attendance status");

            var template = _db.Get<EmailTemplate>("sp_email_template_get", new { EmailTemplateId = 4 });
            // PrepareSendEmailNotification(currentAttendance, template);
            return _attendanceRequestService.FetchPendingRequestService(firstItem.ReportingManagerId, 0);
        }

        public List<DailyTimesheetDetail> ReAssigneTimesheetService(List<DailyTimesheetDetail> dailyTimesheetDetails)
        {
            return null;
        }
    }
}
