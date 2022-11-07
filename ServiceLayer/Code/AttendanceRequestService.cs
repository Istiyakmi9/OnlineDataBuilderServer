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
using System.Threading.Tasks;

namespace ServiceLayer.Code
{
    public class AttendanceRequestService : IAttendanceRequestService
    {
        private readonly IDb _db;
        private readonly ITimezoneConverter _timezoneConverter;
        private readonly CurrentSession _currentSession;
        private readonly IEMailManager _eMailManager;
        private readonly ILogger<AttendanceRequestService> _logger;
        private readonly ICommonService _commonService;

        public AttendanceRequestService(IDb db,
            ITimezoneConverter timezoneConverter,
            CurrentSession currentSession,
            ICommonService commonService,
            ILogger<AttendanceRequestService> logger,
            IEMailManager eMailManager)
        {
            _db = db;
            _timezoneConverter = timezoneConverter;
            _currentSession = currentSession;
            _eMailManager = eMailManager;
            _logger = logger;
            _commonService = commonService;
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

        public async Task<RequestModel> ApproveAttendanceService(AttendanceDetails attendanceDetail, int filterId = ApplicationConstants.Only)
        {
            await UpdateAttendanceDetail(attendanceDetail, ItemStatus.Approved);
            return this.GetRequestPageData(_currentSession.CurrentUserDetail.UserId, filterId);
        }

        public async Task<RequestModel> RejectAttendanceService(AttendanceDetails attendanceDetail, int filterId = ApplicationConstants.Only)
        {
            await UpdateAttendanceDetail(attendanceDetail, ItemStatus.Rejected);
            return this.GetRequestPageData(_currentSession.CurrentUserDetail.UserId, filterId);
        }

        public async Task UpdateAttendanceDetail(AttendanceDetails attendanceDetail, ItemStatus status)
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

                _logger.LogInformation("Attendance: " + currentAttendance.AttendanceDay);

                currentAttendance.PresentDayStatus = (int)status;
                currentAttendance.AttendanceId = attendanceDetail.AttendanceId;
                currentAttendance.AttendenceStatus = (int)DayStatus.WorkFromHome;
                // this call is used for only upadate AttendanceDetail json object

                _logger.LogInformation("Final Attendance: " + JsonConvert.SerializeObject(allAttendance));
                var Result = _db.Execute<Attendance>("sp_attendance_update_request", new
                {
                    AttendanceId = attendanceDetail.AttendanceId,
                    AttendanceDetail = JsonConvert.SerializeObject(allAttendance),
                    UserId = _currentSession.CurrentUserDetail.UserId
                }, true);
                if (string.IsNullOrEmpty(Result))
                    throw new HiringBellException("Unable to update attendance status");

                var template = _db.Get<EmailTemplate>("sp_email_template_get", new { EmailTemplateId = ApplicationConstants.AttendanceRequestTemplate });

                if (template == null)
                    throw new HiringBellException("Email template not found", System.Net.HttpStatusCode.NotFound);


                var emailSenderModal = await _commonService.ReplaceActualData(
                    new TemplateReplaceModal //EmployeeNotificationModel
                    {
                        DeveloperName = currentAttendance.EmployeeName,
                        RequestType = "Work From Home",
                        CompanyName = template.SignatureDetail,
                        BodyContent = template.BodyContent,
                        Subject = template.SubjectLine,
                        ToAddress = new List<string> { currentAttendance.Email },
                        ActionType = status.ToString(),
                        FromDate = currentAttendance.AttendanceDay,
                        ToDate = currentAttendance.AttendanceDay,
                        ManagerName = _currentSession.CurrentUserDetail.FullName,
                        Message = string.IsNullOrEmpty(currentAttendance.UserComments)
                                    ? "NA"
                                    : currentAttendance.UserComments,
                    }, template);


                await _eMailManager.SendMailAsync(emailSenderModal);
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
    }
}
