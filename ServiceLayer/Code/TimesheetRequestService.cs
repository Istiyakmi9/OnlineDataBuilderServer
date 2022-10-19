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
    public class TimesheetRequestService : ITimesheetRequestService
    {
        private readonly IDb _db;
        private readonly ITimezoneConverter _timezoneConverter;
        private readonly CurrentSession _currentSession;

        public TimesheetRequestService(IDb db, ITimezoneConverter timezoneConverter, CurrentSession currentSession)
        {
            _db = db;
            _timezoneConverter = timezoneConverter;
            _currentSession = currentSession;
        }

        public dynamic FetchPendingRequestService(long employeeId, int requestTypeId)
        {
            if (employeeId < 0)
                throw new HiringBellException("Invalid employee id.");

            string procedure = "sp_approval_requests_get";
            if (_currentSession.CurrentUserDetail.RoleId == (int)UserType.Admin)
                procedure = "sp_approval_requests_get_by_role";

            DateTime now = _timezoneConverter.ToSpecificTimezoneDateTime(_currentSession.TimeZone);
            var resultSet = _db.FetchDataSet(procedure, new
            {
                ManagerId = employeeId,
                StatusId = requestTypeId,
                ForYear = now.Year,
                ForMonth = now.Month
            });

            if (resultSet != null && resultSet.Tables.Count != 2)
                throw new HiringBellException("Fail to get approval request data for current user.");

            DataTable attendanceTable = null;
            if (resultSet.Tables[1].Rows.Count > 0)
                attendanceTable = resultSet.Tables[1];

            int i = 0;
            List<TimesheetDetail> approvalRequest = Converter.ToList<TimesheetDetail>(resultSet.Tables[0]);
            Parallel.For(i, approvalRequest.Count, x =>
            {
                approvalRequest.ElementAt(i).FromDate = _timezoneConverter.UpdateToUTCTimeZoneOnly(approvalRequest.ElementAt(i).FromDate);
                approvalRequest.ElementAt(i).ToDate = _timezoneConverter.UpdateToUTCTimeZoneOnly(approvalRequest.ElementAt(i).ToDate);
            });

            return new { ApprovalRequest = approvalRequest, AttendaceTable = attendanceTable };
        }

        public List<TimesheetDetail> RejectTimesheetService(TimesheetDetail timesheetDetail)
        {
            return UpdateTimesheetRequest(timesheetDetail, ItemStatus.Rejected);
        }

        public List<TimesheetDetail> ApprovalTimesheetService(TimesheetDetail timesheetDetail)
        {
            return UpdateTimesheetRequest(timesheetDetail, ItemStatus.Approved);
        }

        public List<TimesheetDetail> UpdateTimesheetRequest(TimesheetDetail timesheetDetail, ItemStatus itemStatus)
        {
            string message = string.Empty;
            DbParam[] param = new DbParam[]
            {
                new DbParam(approvalRequest.ApprovalRequestId, typeof(long), "_ApprovalRequestId"),
                new DbParam(approvalRequest.LeaveRequestId, typeof(long), "_LeaveRequestId"),
                new DbParam(2, typeof(int), "_RequestType")
            };

            var result = _db.GetDataset("sp_approval_request_GetById", param);
            if (result.Tables.Count > 0 && result.Tables[0].Rows.Count > 0)
            {
                var attendanceDetailString = result.Tables[0].Rows[0]["AttendanceDetail"].ToString();
                List<AttendenceDetail> attendenceDetails = JsonConvert.DeserializeObject<List<AttendenceDetail>>(attendanceDetailString);
                TimesheetDetail existingRecord = Converter.ToType<ApprovalRequest>(result.Tables[0]);

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
                                                                                  AttendenceStatus = n.AttendenceStatus
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
                        new DbParam(attendanceDetailString, typeof(string), "_AttendanceDetail"),
                        new DbParam(0, typeof(int), "_LeaveType"),
                        new DbParam(RequestType.Attandance, typeof(int), "_RequestType"),
                        new DbParam(approvalRequest.LeaveRequestId, typeof(long), "_LeaveRequestId")
                    };

                    message = _db.ExecuteNonQuery("sp_approval_request_attendace_InsUpdate", param, true);
                    if (!string.IsNullOrEmpty(message))
                    {
                        return FetchPendingRequestService(existingRecord.UserId, RequestId);
                    }
                }
            }
            return null;
        }

        public List<TimesheetDetail> ReAssigneTimesheetService(TimesheetDetail timesheetDetail)
        {
            return null;
        }
    }
}
