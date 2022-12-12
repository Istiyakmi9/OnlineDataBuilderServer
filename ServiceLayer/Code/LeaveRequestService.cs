using BottomhalfCore.DatabaseLayer.Common.Code;
using BottomhalfCore.Services.Interface;
using EMailService.Service;
using ModalLayer.Modal;
using ModalLayer.Modal.Leaves;
using Newtonsoft.Json;
using ServiceLayer.Interface;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServiceLayer.Code
{
    public class LeaveRequestService : ILeaveRequestService
    {
        private readonly IDb _db;
        private readonly ITimezoneConverter _timezoneConverter;
        private readonly CurrentSession _currentSession;
        private readonly IAttendanceRequestService _attendanceRequestService;
        private readonly ICommonService _commonService;
        private readonly IEmailService _emailService;

        public LeaveRequestService(IDb db,
            ITimezoneConverter timezoneConverter,
            CurrentSession currentSession,
            IEmailService emailService,
            IAttendanceRequestService attendanceRequestService,
            ICommonService commonService)
        {
            _db = db;
            _timezoneConverter = timezoneConverter;
            _currentSession = currentSession;
            _attendanceRequestService = attendanceRequestService;
            _commonService = commonService;
            _emailService = emailService;
        }

        public async Task<RequestModel> ApprovalLeaveService(LeaveRequestDetail leaveRequestDetail, int filterId = ApplicationConstants.Only)
        {
            await UpdateLeaveDetail(leaveRequestDetail, ItemStatus.Approved);
            return _attendanceRequestService.GetRequestPageData(_currentSession.CurrentUserDetail.UserId, filterId);
        }

        public async Task<RequestModel> RejectLeaveService(LeaveRequestDetail leaveRequestDetail, int filterId = ApplicationConstants.Only)
        {
            await UpdateLeaveDetail(leaveRequestDetail, ItemStatus.Rejected);
            return _attendanceRequestService.GetRequestPageData(_currentSession.CurrentUserDetail.UserId, filterId);
        }

        public async Task UpdateLeaveDetail(LeaveRequestDetail leaveDeatil, ItemStatus status)
        {
            string message = string.Empty;
            var leaveRequestDetail = _db.Get<LeaveRequestDetail>("sp_employee_leave_request_GetById", new
            {
                EmployeeId = leaveDeatil.EmployeeId,
                Year = DateTime.Now.Year
            });

            if (leaveRequestDetail == null || string.IsNullOrEmpty(leaveRequestDetail.LeaveDetail))
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
                    singleLeaveDetail.FeedBack = String.Empty;
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
                message = _db.Execute<LeaveRequestNotification>("sp_leave_notification_and_request_InsUpdate", new
                {
                    leaveRequestDetail.LeaveRequestId,
                    leaveRequestDetail.EmployeeId,
                    leaveRequestDetail.LeaveDetail,
                    leaveRequestDetail.Reason,
                    leaveRequestDetail.AssignTo,
                    leaveRequestDetail.Year,
                    leaveRequestDetail.LeaveFromDay,
                    leaveRequestDetail.LeaveToDay,
                    leaveRequestDetail.LeaveTypeId,
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

            var templateReplaceModal = new TemplateReplaceModal
            {
                DeveloperName = leaveRequestDetail.FirstName + " " + leaveRequestDetail.LastName,
                RequestType = ApplicationConstants.Leave,
                ToAddress = new List<string> { leaveRequestDetail.Email },
                ActionType = status.ToString(),
                FromDate = leaveDeatil.LeaveFromDay,
                ToDate = leaveDeatil.LeaveToDay,
                LeaveType = leaveDeatil.LeaveToDay.ToString(),
                ManagerName = _currentSession.CurrentUserDetail.FullName,
                Message = string.IsNullOrEmpty(leaveDeatil.Reason)
                                    ? "NA"
                                    : leaveDeatil.Reason
            };

            await _emailService.SendEmailWithTemplate(ApplicationConstants.ApplyLeaveRequestTemplate, templateReplaceModal);
        }

        public List<LeaveRequestNotification> ReAssigneToOtherManagerService(LeaveRequestNotification leaveRequestNotification, int filterId = ApplicationConstants.Only)
        {
            return null;
        }
    }
}
