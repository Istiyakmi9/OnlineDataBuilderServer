using BottomhalfCore.DatabaseLayer.Common.Code;
using BottomhalfCore.Services.Interface;
using ModalLayer.Modal;
using ModalLayer.Modal.Leaves;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.BC;
using ServiceLayer.Code.ApprovalChain;
using ServiceLayer.Code.SendEmail;
using ServiceLayer.Interface;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServiceLayer.Code
{
    public class LeaveRequestService : ILeaveRequestService
    {
        private readonly IDb _db;
        private readonly CurrentSession _currentSession;
        private readonly IAttendanceRequestService _attendanceRequestService;
        private readonly ApprovalEmailService _approvalEmailService;
        private readonly WorkFlowChain _workFlowChain;

        public LeaveRequestService(IDb db,
            ApprovalEmailService approvalEmailService,
            CurrentSession currentSession,
            WorkFlowChain workFlowChain,
            IAttendanceRequestService attendanceRequestService)
        {
            _db = db;
            _workFlowChain = workFlowChain;
            _currentSession = currentSession;
            _attendanceRequestService = attendanceRequestService;
            _approvalEmailService = approvalEmailService;
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

        private void updateLeaveCountOnRejected(LeaveRequestDetail LeaveRequestDetail, int leaveTypeId, decimal leaveCount)
        {
            if (leaveTypeId <= 0)
                throw HiringBellException.ThrowBadRequest("Invalid leave type id");


            if (!string.IsNullOrEmpty(LeaveRequestDetail.LeaveQuotaDetail))
            {
                var records = JsonConvert.DeserializeObject<List<LeaveTypeBrief>>(LeaveRequestDetail.LeaveQuotaDetail);
                if (records.Count > 0)
                {
                    var item = records.Find(x => x.LeavePlanTypeId == leaveTypeId);
                    item.AvailableLeaves += leaveCount;
                    LeaveRequestDetail.LeaveQuotaDetail = JsonConvert.SerializeObject(records);
                }
            }
        }

        public async Task UpdateLeaveDetail(LeaveRequestDetail requestDetail, ItemStatus status)
        {
            if (requestDetail.LeaveRequestNotificationId <= 0)
                throw HiringBellException.ThrowBadRequest("Invalid request. Please check your detail first.");

            string message = string.Empty;
            (var leaveRequestDetail, LeavePlanType leavePlanType) = _db.Get<LeaveRequestDetail, LeavePlanType>("sp_employee_leave_request_GetById", new
            {
                LeaveRequestNotificationId = requestDetail.LeaveRequestNotificationId
            });

            if (leaveRequestDetail == null || string.IsNullOrEmpty(leaveRequestDetail.LeaveDetail))
                throw new HiringBellException("Unable to find leave detail. Please contact to admin.");

            List<CompleteLeaveDetail> completeLeaveDetail = JsonConvert
              .DeserializeObject<List<CompleteLeaveDetail>>(leaveRequestDetail.LeaveDetail);

            if (completeLeaveDetail == null)
                throw new HiringBellException("Unable to find applied leave detail. Please contact to admin");

            var singleLeaveDetail = completeLeaveDetail.Find(x => x.RecordId.Equals(requestDetail.RecordId));
            if (singleLeaveDetail == null)
                throw new HiringBellException("Unable to find applied leave. Please contact to admin");


            long nextId = 0;
            leaveRequestDetail.RequestStatusId = (int)status;
            singleLeaveDetail.LeaveStatus = (int)status;
            if (ItemStatus.Rejected == status)
            {
                var totalLeaves = (decimal)requestDetail.LeaveToDay.Date.Subtract(requestDetail.LeaveFromDay.Date).TotalDays + 1;
                updateLeaveCountOnRejected(leaveRequestDetail, requestDetail.LeaveTypeId, totalLeaves);
            }
            else
            {
                nextId = _workFlowChain.GetNextRequestor(leavePlanType, singleLeaveDetail, leaveRequestDetail.AssigneeId);
                if (nextId > 0)
                {
                    leaveRequestDetail.AssigneeId = nextId;
                    leaveRequestDetail.RequestStatusId = (int)ItemStatus.Pending;
                    singleLeaveDetail.LeaveStatus = (int)ItemStatus.Pending;
                }
                else
                {
                    leaveRequestDetail.AssigneeId = 0;
                }
            }

            singleLeaveDetail.RespondedBy = _currentSession.CurrentUserDetail.UserId;
            leaveRequestDetail.LeaveDetail = JsonConvert.SerializeObject(completeLeaveDetail);

            message = _db.Execute<LeaveRequestNotification>("sp_leave_notification_and_request_InsUpdate", new
            {
                leaveRequestDetail.LeaveRequestId,
                leaveRequestDetail.EmployeeId,
                leaveRequestDetail.LeaveDetail,
                leaveRequestDetail.Reason,
                leaveRequestDetail.AssigneeId,
                leaveRequestDetail.ReportingManagerId,
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
                requestDetail.LeaveRequestNotificationId,
                RecordId = requestDetail.RecordId
            }, true);
            if (string.IsNullOrEmpty(message))
                throw new HiringBellException("Unable to update leave status. Please contact to admin");

            leaveRequestDetail.LeaveFromDay = requestDetail.LeaveFromDay;
            leaveRequestDetail.LeaveToDay = requestDetail.LeaveToDay;
            leaveRequestDetail.Reason = requestDetail.Reason;
            leaveRequestDetail.LeaveType = requestDetail.LeaveType;
            Task task = Task.Run(async () => await _approvalEmailService.LeaveApprovalStatusSendEmail(leaveRequestDetail, status));

            await Task.CompletedTask;
        }

        public List<LeaveRequestNotification> ReAssigneToOtherManagerService(LeaveRequestNotification leaveRequestNotification, int filterId = ApplicationConstants.Only)
        {
            return null;
        }
    }
}
