using BottomhalfCore.DatabaseLayer.Common.Code;
using BottomhalfCore.Services.Interface;
using EMailService.Service;
using Microsoft.Extensions.Logging;
using ModalLayer;
using ModalLayer.Modal;
using Newtonsoft.Json;
using ServiceLayer.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace ServiceLayer.Code
{
    public class AttendanceRequestService : IAttendanceRequestService
    {
        private readonly IDb _db;
        private readonly ITimezoneConverter _timezoneConverter;
        private readonly CurrentSession _currentSession;
        private readonly IEMailManager _eMailManager;
        private readonly ILogger<AttendanceRequestService> _logger;

        public AttendanceRequestService(IDb db,
            ITimezoneConverter timezoneConverter,
            CurrentSession currentSession,
            ILogger<AttendanceRequestService> logger,
            IEMailManager eMailManager)
        {
            _db = db;
            _timezoneConverter = timezoneConverter;
            _currentSession = currentSession;
            _eMailManager = eMailManager;
            _logger = logger;
        }

        private RequestModel GetEmployeeRequestedDataService(long employeeId, string procedure)
        {
            if (employeeId < 0)
                throw new HiringBellException("Invalid employee id.");

            DateTime now = _timezoneConverter.ToSpecificTimezoneDateTime(_currentSession.TimeZone);
            var resultSet = _db.FetchDataSet(procedure, new
            {
                ManagerId = employeeId,
                StatusId = 0,
                ForYear = now.Year,
                ForMonth = now.Month
            });

            if (resultSet != null && resultSet.Tables.Count != 3)
                throw new HiringBellException("Fail to get approval request data for current user.");

            DataTable attendanceTable = null;
            DataTable timsesheetTable = null;
            DataTable leaveTable = null;
            if (resultSet.Tables[1].Rows.Count > 0)
                attendanceTable = resultSet.Tables[1];

            if (resultSet.Tables[2].Rows.Count > 0)
                timsesheetTable = resultSet.Tables[2];

            if (resultSet.Tables[0].Rows.Count > 0)
                leaveTable = resultSet.Tables[0];

            return new RequestModel
            {
                ApprovalRequest = leaveTable,
                AttendaceTable = attendanceTable,
                TimesheetTable = timsesheetTable
            };
        }

        public RequestModel GetRequestPageData(long employeeId, int filterId)
        {
            if (filterId == ApplicationConstants.Only)
                return FetchPendingRequestService(_currentSession.CurrentUserDetail.UserId);
            else
                return GetManagerAndUnAssignedRequestService(_currentSession.CurrentUserDetail.UserId);
        }

        public RequestModel GetManagerAndUnAssignedRequestService(long employeeId)
        {
            return GetEmployeeRequestedDataService(employeeId, "sp_leave_timesheet_and_attendance_requests_get_by_role");
        }

        public RequestModel FetchPendingRequestService(long employeeId)
        {
            return GetEmployeeRequestedDataService(employeeId, "sp_leave_timesheet_and_attendance_requests_get");
        }

        public RequestModel ApproveAttendanceService(AttendanceDetails attendanceDetail, int filterId = ApplicationConstants.Only)
        {
            UpdateAttendanceDetail(attendanceDetail, ItemStatus.Approved);
            return this.GetRequestPageData(_currentSession.CurrentUserDetail.UserId, filterId);
        }

        public RequestModel RejectAttendanceService(AttendanceDetails attendanceDetail, int filterId = ApplicationConstants.Only)
        {
            UpdateAttendanceDetail(attendanceDetail, ItemStatus.Rejected);
            return this.GetRequestPageData(_currentSession.CurrentUserDetail.UserId, filterId);
        }

        public void UpdateAttendanceDetail(AttendanceDetails attendanceDetail, ItemStatus status)
        {
            if (attendanceDetail.AttendanceId <= 0)
                throw new HiringBellException("Invalid attendance day selected");
            try
            {
                var attendance = _db.Get<Attendance>("sp_attendance_get_byid", new
                {
                    AttendanceId = attendanceDetail.AttendanceId
                });

                var allAttendance = JsonConvert.DeserializeObject<List<AttendanceDetails>>(attendance.AttendanceDetail);
                var currentAttendance = allAttendance.Find(x => x.AttendanceDay == attendanceDetail.AttendanceDay);
                if (currentAttendance == null)
                    throw new HiringBellException("Unable to update present request. Please contact to admin.");

                _logger.LogInformation("Attendance: " + JsonConvert.SerializeObject(currentAttendance));

                currentAttendance.PresentDayStatus = (int)status;
                currentAttendance.AttendanceId = attendanceDetail.AttendanceId;
                currentAttendance.AttendenceStatus = (int)DayStatus.WorkFromHome;
                // this call is used for only upadate AttendanceDetail json object

                _logger.LogInformation("Final Attendance: " + JsonConvert.SerializeObject(currentAttendance));
                var Result = _db.Execute<Attendance>("sp_attendance_update_request", new
                {
                    AttendanceId = attendanceDetail.AttendanceId,
                    AttendanceDetail = JsonConvert.SerializeObject(allAttendance),
                    UserId = _currentSession.CurrentUserDetail.UserId
                }, true);
                if (string.IsNullOrEmpty(Result))
                    throw new HiringBellException("Unable to update attendance status");

                var template = _db.Get<EmailTemplate>("sp_email_template_get", new { EmailTemplateId = ApplicationConstants.RequestTemplate });

                if (template == null)
                    throw new HiringBellException("Email template not found", System.Net.HttpStatusCode.NotFound);


                var emailSenderModal = PrepareSendEmailNotification(
                    new EmployeeNotificationModel
                    {
                        DeveloperName = currentAttendance.EmployeeName,
                        AttendanceRequestType = "Work From Home",
                        CompanyName = template.SignatureDetail,
                        BodyContent = template.BodyContent,
                        Subject = template.SubjectLine,
                        To = new List<string> { currentAttendance.Email },
                        ApprovalType = status.ToString(),
                        FromDate = currentAttendance.AttendanceDay,
                        ToDate = currentAttendance.AttendanceDay,
                        LeaveType = null,
                        ManagerName = _currentSession.CurrentUserDetail.FullName,
                        Message = string.IsNullOrEmpty(currentAttendance.UserComments)
                                    ? "NA"
                                    : currentAttendance.UserComments,
                    }, template);

                _eMailManager.SendMailAsync(emailSenderModal);
            }
            catch (Exception)
            {
                throw new HiringBellException("Encounter error while sending email notification.", System.Net.HttpStatusCode.NotFound);
            }
        }

        public List<Attendance> ReAssigneAttendanceService(AttendanceDetails attendanceDetail)
        {
            return null;
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
                                .Replace("[[USER-MESSAGE]]", notification.Message);

                StringBuilder builder = new StringBuilder();
                builder.AppendLine();
                builder.AppendLine();
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
    }
}
