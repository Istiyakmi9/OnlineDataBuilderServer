using BottomhalfCore.DatabaseLayer.Common.Code;
using BottomhalfCore.Services.Code;
using BottomhalfCore.Services.Interface;
using EMailService.Service;
using ModalLayer;
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
    public class AttendanceRequestService : IAttendanceRequestService
    {
        private readonly IDb _db;
        private readonly ITimezoneConverter _timezoneConverter;
        private readonly CurrentSession _currentSession;
        private readonly IEmailService _eEmailService;

        public AttendanceRequestService(IDb db,
            ITimezoneConverter timezoneConverter,
            CurrentSession currentSession,
            IEmailService eEmailService)
        {
            _db = db;
            _timezoneConverter = timezoneConverter;
            _currentSession = currentSession;
            _eEmailService = eEmailService;
        }

        private dynamic GetEmployeeRequestedDataService(long employeeId, int requestTypeId, string procedure)
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

        public RequestModel GetManagerAndUnAssignedRequestService(long employeeId, int requestTypeId)
        {
            return GetEmployeeRequestedDataService(employeeId, requestTypeId, "sp_approval_requests_get_by_role");
        }

        public RequestModel FetchPendingRequestService(long employeeId, int requestTypeId)
        {
            return GetEmployeeRequestedDataService(employeeId, requestTypeId, "sp_approval_requests_get");
        }

        public RequestModel ApprovalAttendanceService(AttendanceDetails attendanceDetail)
        {
            return UpdateAttendanceDetail(attendanceDetail, ItemStatus.Approved);
        }

        public RequestModel RejectAttendanceService(AttendanceDetails attendanceDetail)
        {
            return UpdateAttendanceDetail(attendanceDetail, ItemStatus.Rejected);
        }

        public RequestModel UpdateAttendanceDetail(AttendanceDetails attendanceDetail, ItemStatus status)
        {
            if (attendanceDetail.AttendanceId <= 0)
                throw new HiringBellException("Invalid attendance day selected");

            var attendance = _db.Get<Attendance>("sp_Attendance_GetById", new
            {
                AttendanceId = attendanceDetail.AttendanceId
            });

            var allAttendance = JsonConvert.DeserializeObject<List<AttendanceDetails>>(attendance.AttendanceDetail);
            var currentAttendance = allAttendance.Find(x => x.AttendanceDay == attendanceDetail.AttendanceDay);
            if (currentAttendance == null)
                throw new HiringBellException("Unable to update present request. Please contact to admin.");

            currentAttendance.PresentDayStatus = (int)status;
            currentAttendance.AttendanceId = attendanceDetail.AttendanceId;
            // this call is used for only upadate AttendanceDetail json object
            var Result = _db.Execute<Attendance>("sp_attendance_update_request", new
            {
                AttendanceId = attendanceDetail.AttendanceId,
                AttendanceDetail = JsonConvert.SerializeObject(allAttendance),
                UserTypeId = 0,
                EmployeeId = 0,
                TotalDays = 0,
                TotalWeekDays = 0,
                DaysPending = 0,
                TotalBurnedMinutes = 0,
                ForYear = 0,
                ForMonth = 0,
                UserId = _currentSession.CurrentUserDetail.UserId
            }, true);
            if (string.IsNullOrEmpty(Result))
                throw new HiringBellException("Unable to update attendance status");

            var template = _db.Get<EmailTemplate>("sp_email_template_get", new { EmailTemplateId = 4 });

            if (template == null)
                throw new HiringBellException("Email template not found", System.Net.HttpStatusCode.NotFound);

            _eEmailService.PrepareSendEmailNotification(new EmployeeNotificationModel
            {
                DeveloperName = currentAttendance.EmployeeName,
                AttendanceRequestType = "Work From Home",
                CompanyName = template.SignatureDetail,
                BodyContent = template.BodyContent,
                Subject = template.SubjectLine,
                To = new List<string> { currentAttendance.Email },
                ApprovalType = status.ToString(),
                FromDate = currentAttendance.AttendanceDay.ToString("dd MMM, yyyy"),
                ToDate = currentAttendance.AttendanceDay.ToString("dd MMM, yyyy"),
                LeaveType = null,
                ManagerName = _currentSession.CurrentUserDetail.FullName,
                Message = string.IsNullOrEmpty(currentAttendance.UserComments)
                            ? "NA"
                            : currentAttendance.UserComments,
            });

            return this.FetchPendingRequestService(_currentSession.CurrentUserDetail.UserId, 0);
        }

        public List<Attendance> ReAssigneAttendanceService(AttendanceDetails attendanceDetail)
        {
            return null;
        }
    }
}
