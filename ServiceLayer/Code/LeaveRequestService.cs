using BottomhalfCore.DatabaseLayer.Common.Code;
using BottomhalfCore.Services.Interface;
using ModalLayer;
using ModalLayer.Modal;
using ModalLayer.Modal.Leaves;
using Newtonsoft.Json;
using ServiceLayer.Interface;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ServiceLayer.Code
{
    public class LeaveRequestService : ILeaveRequestService
    {
        private readonly IDb _db;
        private readonly ITimezoneConverter _timezoneConverter;
        private readonly CurrentSession _currentSession;
        private readonly IAttendanceRequestService _attendanceRequestService;
        private readonly IEmailService _eEmailService;

        public LeaveRequestService(IDb db,
            ITimezoneConverter timezoneConverter,
            CurrentSession currentSession,
            IAttendanceRequestService attendanceRequestService,
            IEmailService eEmailService)
        {
            _db = db;
            _timezoneConverter = timezoneConverter;
            _currentSession = currentSession;
            _attendanceRequestService = attendanceRequestService;
            _eEmailService = eEmailService;
        }

        public RequestModel ApprovalLeaveService(LeaveRequestDetail leaveRequestDetail)
        {
            return UpdateLeaveDetail(leaveRequestDetail, ItemStatus.Approved);
        }

        public RequestModel RejectLeaveService(LeaveRequestDetail leaveRequestDetail)
        {
            return UpdateLeaveDetail(leaveRequestDetail, ItemStatus.Rejected);
        }

        public RequestModel UpdateLeaveDetail(LeaveRequestDetail leaveDeatil, ItemStatus status)
        {
            string message = string.Empty;
            var leaveRequestDetail = _db.Get<LeaveRequestDetail>("sp_employee_leave_request_GetById", new
            {
                EmployeeId = leaveDeatil.EmployeeId,
                Year = DateTime.Now.Year
            });

            if (leaveRequestDetail == null || !string.IsNullOrEmpty(leaveRequestDetail.LeaveDetail))
                throw new HiringBellException("Unable to find leave detail. Please contact to admin.");

            List<CompleteLeaveDetail> completeLeaveDetail = JsonConvert
              .DeserializeObject<List<CompleteLeaveDetail>>(leaveRequestDetail.LeaveDetail);

            if (completeLeaveDetail != null)
            {
                var singleLeaveDetail = completeLeaveDetail.Find(x =>
                    leaveDeatil.LeaveFromDay.Subtract(x.LeaveFromDay).TotalDays == 0 &&
                    leaveDeatil.LeaveToDay.Subtract(x.LeaveToDay).TotalDays == 0
                );

                if (singleLeaveDetail != null)
                {
                    singleLeaveDetail.LeaveStatus = (int)status;
                    singleLeaveDetail.RespondedBy = _currentSession.CurrentUserDetail.UserId;
                    leaveRequestDetail.LeaveDetail = JsonConvert.SerializeObject(completeLeaveDetail);
                }
                else
                {
                    throw new HiringBellException("Error");
                }
            }
            else
            {
                throw new HiringBellException("Error");
            }

            if (leaveRequestDetail != null)
            {
                leaveRequestDetail.RequestStatusId = (int)status;
                message = _db.Execute<LeaveRequestNotification>("sp_employee_leave_request_InsUpdate", new
                {
                    leaveRequestDetail.LeaveRequestId,
                    leaveRequestDetail.EmployeeId,
                    leaveRequestDetail.LeaveDetail,
                    leaveRequestDetail.Reason,
                    leaveRequestDetail.AssignTo,
                    leaveRequestDetail.Year,
                    leaveRequestDetail.LeaveFromDay,
                    leaveRequestDetail.LeaveToDay,
                    leaveRequestDetail.LeaveType,
                    leaveRequestDetail.RequestStatusId,
                    leaveRequestDetail.AvailableLeaves,
                    leaveRequestDetail.TotalLeaveApplied,
                    leaveRequestDetail.TotalApprovedLeave,
                    leaveRequestDetail.TotalLeaveQuota,
                    leaveRequestDetail.LeaveQuotaDetail,
                    NumOfDays = 0,
                    leaveDeatil.LeaveRequestNotificationId
                }, true);
                if (string.IsNullOrEmpty(message))
                    throw new HiringBellException("Unable to update leave status. Please contact to admin");
            }

            var template = _db.Get<EmailTemplate>("sp_email_template_get", new { EmailTemplateId = 4 });

            if (template == null)
                throw new HiringBellException("Email template not found", System.Net.HttpStatusCode.NotFound);

            _eEmailService.PrepareSendEmailNotification(new EmployeeNotificationModel
            {
                DeveloperName = leaveRequestDetail.FirstName + " " + leaveRequestDetail.LastName,
                AttendanceRequestType = ApplicationConstants.Timesheet,
                CompanyName = template.SignatureDetail,
                BodyContent = template.BodyContent,
                Subject = template.SubjectLine,
                To = new List<string> { leaveRequestDetail.Email },
                ApprovalType = status.ToString(),
                FromDate = leaveDeatil.LeaveFromDay,
                ToDate = leaveDeatil.LeaveToDay,
                LeaveType = leaveDeatil.LeaveToDay.ToString(),
                ManagerName = _currentSession.CurrentUserDetail.FullName,
                Message = string.IsNullOrEmpty(leaveDeatil.Reason)
                            ? "NA"
                            : leaveDeatil.Reason
            });
            return _attendanceRequestService.FetchPendingRequestService(_currentSession.CurrentUserDetail.UserId);
        }

        public List<LeaveRequestNotification> ReAssigneToOtherManagerService(LeaveRequestNotification leaveRequestNotification)
        {
            return null;
        }
    }
}
