using BottomhalfCore.DatabaseLayer.Common.Code;
using BottomhalfCore.Services.Interface;
using EMailService.Service;
using ModalLayer.Modal;
using Newtonsoft.Json;
using ServiceLayer.Code.SendEmail;
using ServiceLayer.Interface;
using System.Collections.Generic;
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
        private readonly IEMailManager _eMailManager;
        private readonly IEmailService _emailService;
        private readonly ICommonService _commonService;
        private readonly ApprovalEmailService _approvalEmailService;

        public TimesheetRequestService(IDb db,
            ITimezoneConverter timezoneConverter,
            ApprovalEmailService approvalEmailService,
            CurrentSession currentSession,
            IEmailService emailService,
            IAttendanceRequestService attendanceRequestService,
            ICommonService commonService,
            IEMailManager eMailManager)
        {
            _db = db;
            _timezoneConverter = timezoneConverter;
            _currentSession = currentSession;
            _attendanceRequestService = attendanceRequestService;
            _eMailManager = eMailManager;
            _commonService = commonService;
            _emailService = emailService;
            _approvalEmailService = approvalEmailService;
        }

        public async Task<RequestModel> RejectTimesheetService(List<DailyTimesheetDetail> dailyTimesheetDetails, int filterId = ApplicationConstants.Only)
        {
            if (dailyTimesheetDetails == null)
                throw new HiringBellException("Invalid operation. Please contact to admin.");

            await UpdateTimesheetRequest(dailyTimesheetDetails, ItemStatus.Rejected);
            return _attendanceRequestService.GetRequestPageData(_currentSession.CurrentUserDetail.UserId, filterId);
        }

        public async Task<RequestModel> ApprovalTimesheetService(List<DailyTimesheetDetail> dailyTimesheetDetails, int filterId = ApplicationConstants.Only)
        {
            if (dailyTimesheetDetails == null)
                throw new HiringBellException("Invalid operation. Please contact to admin.");

            await UpdateTimesheetRequest(dailyTimesheetDetails, ItemStatus.Approved);
            return _attendanceRequestService.GetRequestPageData(_currentSession.CurrentUserDetail.UserId, filterId);
        }

        public async Task<RequestModel> UpdateTimesheetRequest(List<DailyTimesheetDetail> dailyTimesheetDetails, ItemStatus itemStatus)
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
                var currentTimesheet = allTimesheet.Find(x => x.PresentDate == dailyTimesheet.PresentDate);
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

            await _approvalEmailService.TimesheetApprovalStatusSendEmail(firstItem, dailyTimesheetDetails, itemStatus);
            return _attendanceRequestService.FetchPendingRequestService(firstItem.ReportingManagerId);
        }

        public List<DailyTimesheetDetail> ReAssigneTimesheetService(List<DailyTimesheetDetail> dailyTimesheetDetails, int filterId = ApplicationConstants.Only)
        {
            return null;
        }
    }
}
