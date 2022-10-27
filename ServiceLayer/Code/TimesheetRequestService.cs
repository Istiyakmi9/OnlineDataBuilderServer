using BottomhalfCore.DatabaseLayer.Common.Code;
using BottomhalfCore.Services.Interface;
using EMailService.Service;
using ModalLayer;
using ModalLayer.Modal;
using Newtonsoft.Json;
using ServiceLayer.Interface;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public TimesheetRequestService(IDb db,
            ITimezoneConverter timezoneConverter,
            CurrentSession currentSession,
            IAttendanceRequestService attendanceRequestService,
            IEMailManager eMailManager)
        {
            _db = db;
            _timezoneConverter = timezoneConverter;
            _currentSession = currentSession;
            _attendanceRequestService = attendanceRequestService;
            _eMailManager = eMailManager;
        }

        public RequestModel RejectTimesheetService(List<DailyTimesheetDetail> dailyTimesheetDetails, int filterId = ApplicationConstants.Only)
        {
            if (dailyTimesheetDetails == null)
                throw new HiringBellException("Invalid operation. Please contact to admin.");

            UpdateTimesheetRequest(dailyTimesheetDetails, ItemStatus.Rejected);
            return _attendanceRequestService.GetRequestPageData(_currentSession.CurrentUserDetail.UserId, filterId);
        }

        public RequestModel ApprovalTimesheetService(List<DailyTimesheetDetail> dailyTimesheetDetails, int filterId = ApplicationConstants.Only)
        {
            if (dailyTimesheetDetails == null)
                throw new HiringBellException("Invalid operation. Please contact to admin.");

            UpdateTimesheetRequest(dailyTimesheetDetails, ItemStatus.Approved);
            return _attendanceRequestService.GetRequestPageData(_currentSession.CurrentUserDetail.UserId, filterId);
        }

        public RequestModel UpdateTimesheetRequest(List<DailyTimesheetDetail> dailyTimesheetDetails, ItemStatus itemStatus)
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

            var template = _db.Get<EmailTemplate>("sp_email_template_get", new { EmailTemplateId = ApplicationConstants.RequestTemplate });

            if (template == null)
                throw new HiringBellException("Email template not found", System.Net.HttpStatusCode.NotFound);

            var sortedTimesheetByDate = dailyTimesheetDetails.OrderByDescending(x => x.PresentDate);
            EmailSenderModal emailSenderModal = _attendanceRequestService.PrepareSendEmailNotification(
                new EmployeeNotificationModel
                {
                    DeveloperName = firstItem.EmployeeName,
                    AttendanceRequestType = ApplicationConstants.Timesheet,
                    CompanyName = template.SignatureDetail,
                    BodyContent = template.BodyContent,
                    Subject = template.SubjectLine,
                    To = new List<string> { firstItem.Email },
                    ApprovalType = itemStatus.ToString(),
                    FromDate = sortedTimesheetByDate.Last().PresentDate,
                    ToDate = sortedTimesheetByDate.First().PresentDate,
                    LeaveType = null,
                    ManagerName = _currentSession.CurrentUserDetail.FullName,
                    Message = string.IsNullOrEmpty(firstItem.UserComments)
                            ? "NA"
                            : firstItem.UserComments,
                }, template);

            _eMailManager.SendMailAsync(emailSenderModal);
            return _attendanceRequestService.FetchPendingRequestService(firstItem.ReportingManagerId);
        }

        public EmailSenderModal PrepareSendEmailNotification(EmployeeNotificationModel notification, EmailTemplate template)
        {
            EmailSenderModal emailSenderModal = null;
            var fromDate = _timezoneConverter.ToTimeZoneDateTime(notification.FromDate, _currentSession.TimeZone);
            var toDate = _timezoneConverter.ToTimeZoneDateTime(notification.ToDate, _currentSession.TimeZone);
            if (notification != null)
            {
                var totalDays = notification.ToDate.Date.Subtract(notification.FromDate.Date).TotalDays + 1;
                string subject = notification.Subject
                                 .Replace("[[REQUEST-TYPE]]", notification.AttendanceRequestType)
                                 .Replace("[[ACTION-TYPE]]", notification.ApprovalType);

                string body = JsonConvert.DeserializeObject<string>(notification.BodyContent)
                                .Replace("[[DEVELOPER-NAME]]", notification.DeveloperName)
                                .Replace("[[DAYS-COUNT]]", $"{totalDays}")
                                .Replace("[[REQUEST-TYPE]]", notification.AttendanceRequestType)
                                .Replace("[[TO-DATE]]", fromDate.ToString("dd MMM, yyyy"))
                                .Replace("[[FROM-DATE]]", toDate.ToString("dd MMM, yyyy"))
                                .Replace("[[ACTION-TYPE]]", notification.ApprovalType)
                                .Replace("[[MANAGER-NAME]]", notification.ManagerName)
                                .Replace("[[USER-MESSAGE]]", notification.Message)
                                .Replace("[[COMPANY-NAME]]", notification.CompanyName);

                StringBuilder builder = new StringBuilder();
                builder.Append("<div>" + template.EmailClosingStatement + "</div>");
                builder.Append("<div>" + template.SignatureDetail + "</div>");
                builder.Append("<div>" + template.ContactNo + "</div>");

                emailSenderModal = new EmailSenderModal
                {
                    To = notification.To,
                    Subject = subject,
                    Body = string.Concat(body, builder.ToString()),
                };
            }

            return emailSenderModal;
        }

        public List<DailyTimesheetDetail> ReAssigneTimesheetService(List<DailyTimesheetDetail> dailyTimesheetDetails, int filterId = ApplicationConstants.Only)
        {
            return null;
        }
    }
}
