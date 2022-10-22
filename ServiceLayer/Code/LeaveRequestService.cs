using BottomhalfCore.DatabaseLayer.Common.Code;
using BottomhalfCore.Services.Code;
using BottomhalfCore.Services.Interface;
using ModalLayer.Modal;
using ModalLayer.Modal.Leaves;
using Newtonsoft.Json;
using ServiceLayer.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceLayer.Code
{
    public class LeaveRequestService : ILeaveRequestService
    {
        private readonly IDb _db;
        private readonly ITimezoneConverter _timezoneConverter;
        private readonly CurrentSession _currentSession;
        private readonly IAttendanceRequestService _attendanceRequestService;
        public LeaveRequestService(IDb db, ITimezoneConverter timezoneConverter, CurrentSession currentSession, IAttendanceRequestService attendanceRequestService)
        {
            _db = db;
            _timezoneConverter = timezoneConverter;
            _currentSession = currentSession;
            _attendanceRequestService = attendanceRequestService;
        }

        public dynamic ApprovalLeaveService(LeaveRequestDetail leaveRequestDetail)
        {
            return UpdateLeaveDetail(leaveRequestDetail, ItemStatus.Approved);
        }

        public dynamic RejectLeaveService(LeaveRequestDetail leaveRequestDetail)
        {
            return UpdateLeaveDetail(leaveRequestDetail, ItemStatus.Rejected);
        }

        public dynamic UpdateLeaveDetail(LeaveRequestDetail leaveDeatil, ItemStatus status)
        {
            string message = string.Empty;
            var leaveRequestDetail = _db.Get<LeaveRequestDetail>("sp_employee_leave_request_GetById", new
            {
                EmployeeId = leaveDeatil.EmployeeId,
                Year = DateTime.Now.Year
            });

            if (leaveRequestDetail != null && !string.IsNullOrEmpty(leaveRequestDetail.LeaveDetail))
            {
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
                            //(from n in completeLeaveDetail
                            // select new
                            // {
                            //     Reason = n.Reason,
                            //     Session = n.Session,
                            //     AssignTo = n.AssignTo,
                            //     LeaveType = n.LeaveType,
                            //     NumOfDays = n.NumOfDays,
                            //     ProjectId = n.ProjectId,
                            //     UpdatedOn = n.UpdatedOn,
                            //     EmployeeId = n.EmployeeId,
                            //     LeaveToDay = n.LeaveToDay,
                            //     LeaveStatus = n.LeaveStatus,
                            //     RequestedOn = n.RequestedOn,
                            //     RespondedBy = n.RespondedBy,
                            //     EmployeeName = n.EmployeeName,
                            //     LeaveFromDay = n.LeaveFromDay
                            // })
                            //);
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
            }
            return _attendanceRequestService.FetchPendingRequestService(leaveRequestDetail.EmployeeId, 0);
        }

        public List<LeaveRequestNotification> ReAssigneToOtherManagerService(LeaveRequestNotification leaveRequestNotification)
        {
            return null;
        }
    }
}
