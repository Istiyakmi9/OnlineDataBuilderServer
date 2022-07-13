using BottomhalfCore.DatabaseLayer.Common.Code;
using BottomhalfCore.Services.Code;
using BottomhalfCore.Services.Interface;
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
    public class RequestService : IRequestService
    {
        private readonly IDb _db;
        private readonly ITimezoneConverter _timezoneConverter;

        public RequestService(IDb db, ITimezoneConverter timezoneConverter)
        {
            _db = db;
            _timezoneConverter = timezoneConverter;
        }

        public List<ApprovalRequest> FetchPendingRequestService(int employeeId, int requestTypeId)
        {
            if (employeeId < 0)
                throw new HiringBellException("Invalid employee id.");

            List<ApprovalRequest> result = _db.GetList<ApprovalRequest>("sp_attendance_get_pending_requests", new { ManagerId = employeeId, StatusId = requestTypeId });
            if (result != null && result.Count > 0)
            {
                int i = 0;
                Parallel.For(i, result.Count, x =>
                {
                    result.ElementAt(i).FromDate = _timezoneConverter.UpdateToUTCTimeZoneOnly(result.ElementAt(i).FromDate);
                    result.ElementAt(i).ToDate = _timezoneConverter.UpdateToUTCTimeZoneOnly(result.ElementAt(i).ToDate);
                });
            }
            return result;
        }

        public string ApprovalOrRejectActionService(ApprovalRequest approvalRequest, ItemStatus status)
        {
            string message = string.Empty;
            DbParam[] param = new DbParam[]
            {
                new DbParam(approvalRequest.ApprovalRequestId, typeof(long), "_ApprovalRequestId")
            };

            var result = _db.GetDataset("sp_approval_request_GetById", param);
            if (result.Tables.Count > 0 && result.Tables[0].Rows.Count > 0)
            {
                var attendanceDetailString = result.Tables[0].Rows[0]["AttendanceDetail"].ToString();
                List<AttendenceDetail> attendenceDetails = JsonConvert.DeserializeObject<List<AttendenceDetail>>(attendanceDetailString);
                ApprovalRequest existingRecord = Converter.ToType<ApprovalRequest>(result.Tables[0]);

                if (attendenceDetails != null)
                {
                    var attendenceDetail = attendenceDetails.Find(x => existingRecord.FromDate.Subtract((DateTime)x.AttendanceDay).TotalDays == 0);
                    if (attendenceDetail != null)
                    {
                        attendenceDetail.AttendenceStatus = (int)status;
                        attendanceDetailString = JsonConvert.SerializeObject((from n in attendenceDetails
                                                                              select new
                                                                              {
                                                                                  TotalMinutes = n.TotalMinutes,
                                                                                  UserTypeId = n.UserTypeId,
                                                                                  PresentDayStatus = n.PresentDayStatus,
                                                                                  EmployeeUid = n.EmployeeUid,
                                                                                  AttendanceId = n.AttendanceId,
                                                                                  UserComments = n.UserComments,
                                                                                  AttendanceDay = n.AttendanceDay,
                                                                                  AttendenceStatus = n.AttendenceStatus,
                                                                                  ClientTimeSheet = n.ClientTimeSheet
                                                                              }));
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

                if (existingRecord != null)
                {
                    existingRecord.RequestStatusId = approvalRequest.RequestStatusId;

                    param = new DbParam[]
                    {
                        new DbParam(existingRecord.ApprovalRequestId, typeof(long), "_ApprovalRequestId"),
                        new DbParam(existingRecord.Message, typeof(string), "_Message"),
                        new DbParam(existingRecord.UserName, typeof(string), "_UserName"),
                        new DbParam(existingRecord.UserId, typeof(long), "_UserId"),
                        new DbParam(existingRecord.UserTypeId, typeof(int), "_UserTypeId"),
                        new DbParam(DateTime.Now, typeof(DateTime), "_RequestedOn"),
                        new DbParam(existingRecord.Email, typeof(string), "_Email"),
                        new DbParam(existingRecord.Mobile, typeof(string), "_Mobile"),
                        new DbParam(existingRecord.FromDate, typeof(DateTime), "_FromDate"),
                        new DbParam(existingRecord.ToDate, typeof(DateTime), "_ToDate"),
                        new DbParam(existingRecord.AssigneeId, typeof(long), "_AssigneeId"),
                        new DbParam(existingRecord.ProjectId, typeof(long), "_ProjectId"),
                        new DbParam(existingRecord.ProjectName, typeof(string), "_ProjectName"),
                        new DbParam(existingRecord.RequestStatusId, typeof(int), "_RequestStatusId"),
                        new DbParam(existingRecord.AttendanceId, typeof(long), "_AttendanceId"),
                        new DbParam(attendanceDetailString, typeof(string), "_AttendanceDetail")
                    };

                    message = _db.ExecuteNonQuery("sp_approval_request_InsUpdate", param, true);
                }
            }
            return message;
        }

        public string ReAssigneToOtherManagerService(ApprovalRequest approvalRequest)
        {
            return null;
        }
    }
}
