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

        public LeaveRequestService(IDb db, ITimezoneConverter timezoneConverter, CurrentSession currentSession)
        {
            _db = db;
            _timezoneConverter = timezoneConverter;
            _currentSession = currentSession;
        }

        public List<LeaveRequestNotification> ApprovalLeaveService(LeaveRequestNotification leaveRequestNotification)
        {
            return UpdateLeaveDetail(leaveRequestNotification, ItemStatus.Approved);
        }

        public List<LeaveRequestNotification> RejectLeaveService(LeaveRequestNotification leaveRequestNotification)
        {
            return UpdateLeaveDetail(leaveRequestNotification, ItemStatus.Rejected);
        }

        public List<LeaveRequestNotification> UpdateLeaveDetail(LeaveRequestNotification leaveRequestNotification, ItemStatus status)
        {
            string message = string.Empty;
            var requestNotification = _db.Get<LeaveRequestNotification>("sp_leave_request_notification_get_byId", new
            {
                leaveRequestNotification.LeaveRequestNotificationId
            });

            if (requestNotification != null)
            {
                List<CompleteLeaveDetail> completeLeaveDetail = JsonConvert
                  .DeserializeObject<List<CompleteLeaveDetail>>(requestNotification.LeaveDetail);

                if (completeLeaveDetail != null)
                {
                    var singleLeaveDetail = completeLeaveDetail.Find(x =>
                        requestNotification.FromDate.Subtract(x.LeaveFromDay).TotalDays == 0 &&
                        requestNotification.ToDate.Subtract(x.LeaveToDay).TotalDays == 0
                    );

                    if (singleLeaveDetail != null)
                    {
                        singleLeaveDetail.LeaveStatus = (int)status;
                        singleLeaveDetail.RespondedBy = _currentSession.CurrentUserDetail.UserId;
                        requestNotification.LeaveDetail = JsonConvert.SerializeObject(
                            (from n in completeLeaveDetail
                             select new
                             {
                                 Reason = n.Reason,
                                 Session = n.Session,
                                 AssignTo = n.AssignTo,
                                 LeaveType = n.LeaveType,
                                 NumOfDays = n.NumOfDays,
                                 ProjectId = n.ProjectId,
                                 UpdatedOn = n.UpdatedOn,
                                 EmployeeId = n.EmployeeId,
                                 LeaveToDay = n.LeaveToDay,
                                 LeaveStatus = n.LeaveStatus,
                                 RequestedOn = n.RequestedOn,
                                 RespondedBy = n.RespondedBy,
                                 EmployeeName = n.EmployeeName,
                                 LeaveFromDay = n.LeaveFromDay
                             })
                            );
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

                if (requestNotification != null)
                {
                    requestNotification.LastReactedOn = DateTime.UtcNow;
                    requestNotification.RequestStatusId = leaveRequestNotification.RequestStatusId;
                    message = _db.Execute<LeaveRequestNotification>("sp_leave_request_notification_InsUpdate", new
                    {
                        requestNotification.LeaveRequestNotificationId,
                        requestNotification.LeaveRequestId,
                        requestNotification.UserMessage,
                        requestNotification.EmployeeId,
                        requestNotification.AssigneeId,
                        requestNotification.ProjectId,
                        requestNotification.ProjectName,
                        requestNotification.FromDate,
                        requestNotification.ToDate,
                        requestNotification.NumOfDays,
                        requestNotification.RequestStatusId,
                        requestNotification.LeaveTypeId,
                        requestNotification.FeedBackMessage,
                        requestNotification.LastReactedOn,
                        requestNotification.LeaveDetail
                    }, true);
                }
            }
            return null;
        }

        public List<LeaveRequestNotification> ReAssigneToOtherManagerService(LeaveRequestNotification leaveRequestNotification)
        {
            return null;
        }
    }
}
