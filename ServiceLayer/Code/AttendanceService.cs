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
    public class AttendanceService : IAttendanceService
    {
        private readonly IDb _db;
        private readonly CurrentSession _currentSession;
        private readonly ITimezoneConverter _timezoneConverter;
        private readonly IEMailManager _eMailManager;
        private readonly IAttendanceRequestService _requestService;

        public AttendanceService(IDb db,
            ITimezoneConverter timezoneConverter,
            CurrentSession currentSession,
            IEMailManager eMailManager,
            IAttendanceRequestService requestService)
        {
            _db = db;
            _currentSession = currentSession;
            _timezoneConverter = timezoneConverter;
            _eMailManager = eMailManager;
            _requestService = requestService;
        }

        private List<AttendenceDetail> CreateAttendanceTillDate(Attendance attendence, DateTime FromDate, DateTime ToDate)
        {
            List<AttendenceDetail> attendenceDetails = new List<AttendenceDetail>();
            if (!string.IsNullOrEmpty(attendence.AttendanceDetail))
                attendenceDetails = JsonConvert.DeserializeObject<List<AttendenceDetail>>(attendence.AttendanceDetail);

            DateTime presentDate = _timezoneConverter.ToTimeZoneDateTime(ToDate, _currentSession.TimeZone);
            var firstDate = _timezoneConverter.ToTimeZoneDateTime(FromDate, _currentSession.TimeZone);

            while (presentDate.Date.Subtract(firstDate.Date).TotalDays >= 0)
            {
                var detail = attendenceDetails
                    .Find(x => _timezoneConverter.ToTimeZoneDateTime(x.AttendanceDay, _currentSession.TimeZone).Date
                    .Subtract(firstDate.Date).TotalDays == 0);

                if (detail == null)
                {
                    attendenceDetails.Add(new AttendenceDetail
                    {
                        IsActiveDay = false,
                        TotalDays = DateTime.DaysInMonth(presentDate.Year, presentDate.Month),
                        AttendanceDay = firstDate,
                        AttendanceId = 0,
                        AttendenceStatus = (int)DayStatus.WorkFromOffice,
                        BillingHours = 480,
                        ClientId = 0,
                        DaysPending = DateTime.DaysInMonth(firstDate.Year, firstDate.Month),
                        EmployeeUid = attendence.EmployeeId,
                        ForMonth = firstDate.Month,
                        ForYear = firstDate.Year,
                        TotalMinutes = 480,
                        IsHoliday = (firstDate.DayOfWeek == DayOfWeek.Saturday
                                        ||
                                    firstDate.DayOfWeek == DayOfWeek.Sunday) ? true : false,
                        IsOnLeave = false,
                        LeaveId = 0,
                        UserComments = string.Empty,
                        UserTypeId = (int)UserType.Employee,
                        IsOpen = true
                    });
                }


                firstDate = firstDate.AddDays(1);
            }

            return attendenceDetails;
        }

        public AttendanceWithClientDetail GetAttendanceByUserId(AttendenceDetail attendenceDetail)
        {
            List<AttendenceDetail> attendenceDetails = new List<AttendenceDetail>();
            Employee employee = null;
            Attendance attendance = null;

            if (attendenceDetail.ForMonth <= 0)
                throw new HiringBellException("Invalid month num. passed.", nameof(attendenceDetail.ForMonth), attendenceDetail.ForMonth.ToString());

            var Result = _db.FetchDataSet("sp_attendance_get", new
            {
                EmployeeId = attendenceDetail.EmployeeUid,
                ClientId = attendenceDetail.ClientId,
                UserTypeId = attendenceDetail.UserTypeId,
                ForYear = attendenceDetail.ForYear,
                ForMonth = attendenceDetail.ForMonth
            });

            if (Result.Tables.Count == 2)
            {
                if (!ApplicationConstants.ContainSingleRow(Result.Tables[1]))
                    throw new HiringBellException("Err!! fail to get employee detail. Plaese contact to admin.");

                employee = Converter.ToType<Employee>(Result.Tables[1]);

                if (ApplicationConstants.ContainSingleRow(Result.Tables[0]) && !string.IsNullOrEmpty(Result.Tables[0].Rows[0]["AttendanceDetail"].ToString()))
                {
                    attendance = Converter.ToType<Attendance>(Result.Tables[0]);
                }
                else
                {
                    attendance = new Attendance
                    {
                        AttendanceDetail = "[]",
                        EmployeeId = attendenceDetail.EmployeeUid,
                    };

                }
            }

            DateTime presentDate = (DateTime)attendenceDetail.AttendenceToDay;
            var firstDate = _timezoneConverter.GetUtcFirstDay(presentDate.Year, presentDate.Month);
            if (presentDate.Year == employee.CreatedOn.Year && presentDate.Month == employee.CreatedOn.Month)
                firstDate = employee.CreatedOn;

            attendenceDetails = CreateAttendanceTillDate(attendance, firstDate, presentDate);
            return new AttendanceWithClientDetail
            {
                EmployeeDetail = employee,
                AttendacneDetails = attendenceDetails.OrderBy(x => x.AttendanceDay).ToList()
            };
        }

        public List<AttendenceDetail> GetAllPendingAttendanceByUserIdService(long employeeId, int UserTypeId, long clientId)
        {
            List<AttendenceDetail> attendanceSet = new List<AttendenceDetail>();
            DateTime current = DateTime.UtcNow;

            DbParam[] dbParams = new DbParam[]
            {
                new DbParam(employeeId, typeof(long), "_EmployeeId"),
                new DbParam(UserTypeId == 0 ? _currentSession.CurrentUserDetail.UserTypeId : UserTypeId, typeof(int), "_UserTypeId"),
                new DbParam(current.Year, typeof(int), "_ForYear"),
                new DbParam(current.Month, typeof(int), "_ForMonth")
            };

            var Result = _db.GetDataset("sp_attendance_detall_pending", dbParams);
            if (Result.Tables.Count == 1 && Result.Tables[0].Rows.Count > 0)
            {
                var currentAttendance = Converter.ToType<Attendance>(Result.Tables[0]);
                attendanceSet = JsonConvert.DeserializeObject<List<AttendenceDetail>>(currentAttendance.AttendanceDetail);
            }

            return attendanceSet;
        }

        private DateTime GetOpenDateForAttendance()
        {
            int i = 4;
            DateTime todayDate = DateTime.UtcNow.Date;
            while (true)
            {
                todayDate = todayDate.AddDays(-1);
                switch (todayDate.DayOfWeek)
                {
                    case DayOfWeek.Saturday:
                    case DayOfWeek.Sunday:
                        break;
                    default:
                        i--;
                        break;
                }

                if (i == 0)
                    break;
            }

            if (i > 0)
                todayDate = todayDate.AddDays(-1 * i);

            return todayDate;
        }

        public async Task<string> SubmitAttendanceService(AttendenceDetail attendenceApplied)
        {
            string Result = string.Empty;
            // this value should come from database as configured by user.
            int dailyWorkingHours = 9;
            DateTime workingDate = (DateTime)attendenceApplied.AttendenceFromDay;
            DateTime workingDateUserTimezone = _timezoneConverter.ToTimeZoneDateTime(workingDate, _currentSession.TimeZone);

            if (workingDate.Subtract((DateTime)attendenceApplied.AttendenceToDay).TotalDays != 0)
                throw new HiringBellException("Apply attendace only for one day. Multiple days attendance is not allowed.");

            Attendance attendance = new Attendance();
            var attendanceList = new List<AttendenceDetail>();

            // check back date limit to allow attendance
            DateTime barrierDate = this.GetOpenDateForAttendance();
            if (attendenceApplied.AttendanceDay.Subtract(barrierDate).TotalDays < 0)
                throw new HiringBellException("Ops!!! You are not allow to submit this date attendace. Please raise a request to your direct manager.");

            // check for leave, holiday and weekends
            await this.IsGivenDateAllowed(workingDate, workingDateUserTimezone);
            attendenceApplied.AttendanceDay = workingDate;

            var result = _db.FetchDataSet("sp_Attendance_YearMonth", new
            {
                EmployeeId = attendenceApplied.EmployeeUid,
                UserTypeId = attendenceApplied.UserTypeId,
                ForMonth = attendenceApplied.AttendanceDay.Month,
                ForYear = attendenceApplied.AttendanceDay.Year
            });

            if (result.Tables.Count == 1 && result.Tables[0].Rows.Count == 1)
            {
                attendance = Converter.ToType<Attendance>(result.Tables[0]);
                if (attendance != null && !string.IsNullOrEmpty(attendance.AttendanceDetail))
                {
                    attendanceList = JsonConvert
                        .DeserializeObject<List<AttendenceDetail>>(result.Tables[0].Rows[0]["AttendanceDetail"].ToString());
                    if (attendanceList.Count != 0)
                    {
                        var workingAttendance = attendanceList
                            .Find(x => x.AttendanceDay.Date.Subtract(workingDate.Date).TotalDays == 0);

                        if (workingAttendance == null)
                        {
                            await this.CreatePresentDayAttendance(attendenceApplied, workingDateUserTimezone);
                            attendanceList.Add(attendenceApplied);
                        }
                        else
                        {
                            await this.CheckAndCreateAttendance(workingAttendance);
                        }

                        int pendingDays = attendanceList.Count(x => x.PresentDayStatus == (int)ItemStatus.Pending);
                        attendance.DaysPending = pendingDays;
                        attendance.TotalHoursBurend = pendingDays * dailyWorkingHours;
                    }
                }
            }
            else
            {
                await this.CreatePresentDayAttendance(attendenceApplied, workingDateUserTimezone);
                attendanceList.Add(attendenceApplied);
                attendance.AttendanceId = 0;
                attendance.EmployeeId = attendenceApplied.EmployeeUid;
                attendance.TotalDays = attendenceApplied.TotalDays;
                attendance.TotalWeekDays = 0;
                attendance.DaysPending = attendenceApplied.DaysPending;
                attendance.ForYear = workingDateUserTimezone.Year;
                attendance.ForMonth = workingDateUserTimezone.Month;
                attendance.TotalHoursBurend = 0;
            }


            var AttendaceDetail = JsonConvert
                                    .SerializeObject((
                                        from n in attendanceList
                                        select new AttendanceDetails
                                        {
                                            TotalMinutes = n.TotalMinutes,
                                            UserTypeId = n.UserTypeId,
                                            PresentDayStatus = n.PresentDayStatus,
                                            EmployeeUid = n.EmployeeUid,
                                            AttendanceId = n.AttendanceId,
                                            UserComments = n.UserComments,
                                            AttendanceDay = n.AttendanceDay,
                                            AttendenceStatus = n.AttendenceStatus,
                                            Email = _currentSession.CurrentUserDetail.Email,
                                            EmployeeName = _currentSession.CurrentUserDetail.FullName,
                                            Mobile = _currentSession.CurrentUserDetail.Mobile,
                                            ReportingManagerId = _currentSession.CurrentUserDetail.ReportingManagerId,
                                            ManagerName = _currentSession.CurrentUserDetail.ManagerName
                                        }));


            Result = _db.Execute<Attendance>("sp_attendance_insupd", new
            {
                AttendanceId = attendance.AttendanceId,
                AttendanceDetail = AttendaceDetail,
                UserTypeId = (int)UserType.Employee,
                EmployeeId = attendance.EmployeeId,
                TotalDays = attendance.TotalDays,
                TotalWeekDays = attendance.TotalWeekDays,
                DaysPending = attendance.DaysPending,
                TotalBurnedMinutes = attendance.TotalHoursBurend,
                ForYear = attendance.ForYear,
                ForMonth = attendance.ForMonth,
                UserId = _currentSession.CurrentUserDetail.UserId
            }, true);

            if (string.IsNullOrEmpty(Result))
                throw new HiringBellException("Unable submit the attendace");

            await SendAttendanceNotification(attendenceApplied);
            return Result;
        }


        public AttendanceWithClientDetail EnablePermission(AttendenceDetail attendenceDetail)
        {
            if (attendenceDetail.ForMonth <= 0)
                throw new HiringBellException("Invalid month num. passed.", nameof(attendenceDetail.ForMonth), attendenceDetail.ForMonth.ToString());

            if (Convert.ToDateTime(attendenceDetail.AttendenceFromDay).Subtract(DateTime.UtcNow).TotalDays > 0)
            {
                throw new HiringBellException("Ohh!!!. Future dates are now allowed.");
            }


            DbParam[] dbParams = new DbParam[]
            {
                new DbParam(attendenceDetail.EmployeeUid, typeof(int), "_EmployeeId"),
                new DbParam(attendenceDetail.ClientId, typeof(int), "_ClientId"),
                new DbParam(attendenceDetail.UserTypeId, typeof(int), "_UserTypeId"),
                new DbParam(attendenceDetail.ForYear, typeof(int), "_ForYear"),
                new DbParam(attendenceDetail.ForMonth, typeof(int), "_ForMonth")
            };

            var Result = _db.GetDataset("sp_attendance_get", dbParams);
            return null;
        }

        private async Task CheckAndCreateAttendance(AttendenceDetail workingAttendance)
        {
            switch (workingAttendance.PresentDayStatus)
            {
                case (int)ItemStatus.Approved:
                    throw new HiringBellException($"Attendance for: {workingAttendance.AttendanceDay.ToString("dd MMM, yyyy")} " +
                        $"already has been {nameof(ItemStatus.Approved)}");
                case (int)ItemStatus.Pending:
                    throw new HiringBellException($"Attendance for: {workingAttendance.AttendanceDay.ToString("dd MMM, yyyy")} " +
                        $"currently is in {nameof(ItemStatus.Approved)} state.");
            }

            workingAttendance.PresentDayStatus = (int)ItemStatus.Pending;
            await Task.CompletedTask;
        }

        private async Task CreatePresentDayAttendance(AttendenceDetail attendenceDetail, DateTime workingTimezoneDate)
        {
            var totalDays = DateTime.DaysInMonth(workingTimezoneDate.Year, workingTimezoneDate.Month);

            attendenceDetail.IsActiveDay = false;
            attendenceDetail.TotalDays = totalDays;
            attendenceDetail.AttendanceId = 0;
            attendenceDetail.AttendenceStatus = (int)DayStatus.WorkFromOffice;
            attendenceDetail.BillingHours = 480;
            attendenceDetail.ClientId = attendenceDetail.ClientId;
            attendenceDetail.DaysPending = totalDays;
            attendenceDetail.EmployeeUid = attendenceDetail.EmployeeUid;
            attendenceDetail.ForMonth = workingTimezoneDate.Month;
            attendenceDetail.ForYear = workingTimezoneDate.Year;
            attendenceDetail.TotalMinutes = 480;
            attendenceDetail.IsHoliday = (workingTimezoneDate.DayOfWeek == DayOfWeek.Saturday
                            ||
                        workingTimezoneDate.DayOfWeek == DayOfWeek.Sunday) ? true : false;
            attendenceDetail.IsOnLeave = false;
            attendenceDetail.LeaveId = 0;
            attendenceDetail.PresentDayStatus = (int)ItemStatus.Pending;
            attendenceDetail.UserTypeId = (int)UserType.Employee;
            attendenceDetail.IsOpen = true;

            await Task.CompletedTask;
        }

        private async Task IsGivenDateAllowed(DateTime workingDate, DateTime workingDateUserTimezone)
        {
            // check if from date is holiday

            // check if from date already applied for leave

            // check if from date is weekend
            switch (workingDateUserTimezone.DayOfWeek)
            {
                case DayOfWeek.Sunday:
                case DayOfWeek.Saturday:
                    throw new HiringBellException("You are trying to apply weekends. Weekends attendacen not allowed.");
            }

            await Task.CompletedTask;
        }

        private string UpdateOrInsertAttendanceDetail(List<AttendenceDetail> finalAttendanceSet, Attendance currentAttendance, string procedure)
        {
            var firstAttn = finalAttendanceSet.FirstOrDefault();

            var AttendaceDetail = JsonConvert.SerializeObject((from n in finalAttendanceSet
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

            double MonthsMinutes = 0;
            currentAttendance.DaysPending = 0;
            finalAttendanceSet.ForEach(x =>
            {
                MonthsMinutes += x.TotalMinutes;
                if (x.AttendenceStatus == 8)
                    currentAttendance.DaysPending++;
            });

            var dbParams = new DbParam[]
            {
                new DbParam(currentAttendance.AttendanceId, typeof(long), "_AttendanceId"),
                new DbParam(currentAttendance.EmployeeId, typeof(long), "_EmployeeId"),
                new DbParam(currentAttendance.UserTypeId, typeof(int), "_UserTypeId"),
                new DbParam(AttendaceDetail, typeof(string), "_AttendanceDetail"),
                new DbParam(currentAttendance.TotalDays, typeof(int), "_TotalDays"),
                new DbParam(currentAttendance.TotalWeekDays, typeof(int), "_TotalWeekDays"),
                new DbParam(currentAttendance.DaysPending, typeof(int), "_DaysPending"),
                new DbParam(MonthsMinutes, typeof(double), "_TotalBurnedMinutes"),
                new DbParam(currentAttendance.ForYear, typeof(int), "_ForYear"),
                new DbParam(currentAttendance.ForMonth, typeof(int), "_ForMonth"),
                new DbParam(_currentSession.CurrentUserDetail.UserId, typeof(long), "_UserId")
            };

            var result = _db.ExecuteNonQuery(procedure, dbParams, true);
            if (string.IsNullOrEmpty(result))
                return null;
            return result;
        }

        private void ValidateDateOfAttendanceSubmission(DateTime firstDate, DateTime lastDate)
        {
            DateTime now = DateTime.Now;
            DateTime presentDate = _timezoneConverter.GetUtcDateTime(now.Year, now.Month, now.Day);

            // handling future date
            if (presentDate.Subtract(lastDate).TotalDays > 0)
            {
                throw new HiringBellException("Future date's are not allowed.");
            }
            // handling past date
            else if (presentDate.Subtract(firstDate).TotalDays < 0)
            {
                if (_currentSession.CurrentUserDetail.RoleId != (int)UserType.Admin)
                {
                    throw new HiringBellException("Past week's are not allowed.");
                }
            }
        }

        private async Task SendAttendanceNotification(AttendenceDetail attendenceApplied)
        {
            var fromDate = _timezoneConverter.ToTimeZoneDateTime((DateTime)attendenceApplied.AttendenceFromDay, _currentSession.TimeZone);
            var toDate = _timezoneConverter.ToTimeZoneDateTime((DateTime)attendenceApplied.AttendenceToDay, _currentSession.TimeZone);
            FilterModel filterModel = new FilterModel
            {
                SearchString = $"1=1 and EmployeeUid = {_currentSession.CurrentUserDetail.ReportingManagerId}",
                SortBy = "",
                PageIndex = 1,
                PageSize = 10
            };
            var managerDetail = _db.Get<Employee>("SP_Employees_Get", filterModel);
            if (managerDetail == null)
                throw new Exception("No manager record found. Please add manager first.");

            var numOfDays = fromDate.Date.Subtract(toDate.Date).TotalDays + 1;

            EmailTemplate template = _eMailManager.GetTemplate(
                new EmailRequestModal
                {
                    DeveloperName = _currentSession.CurrentUserDetail.FullName,
                    ManagerName = "NA",
                    RequestType = ApplicationConstants.DailyAttendance,
                    FromDate = fromDate,
                    ToDate = toDate,
                    ActionType = nameof(ItemStatus.Submitted),
                    Message = attendenceApplied.UserComments,
                    TotalNumberOfDays = numOfDays,
                    TemplateId = ApplicationConstants.AttendanceRequestTemplate
                });

            var body = JsonConvert.DeserializeObject<string>(template.BodyContent);
            EmailSenderModal emailSenderModal = new EmailSenderModal
            {
                To = new List<string> { managerDetail.Email },
                Body = body + template.Footer,
                Title = String.IsNullOrEmpty(template.EmailTitle) ? "Attendance Request" : template.EmailTitle,
                Subject = $"{_currentSession.CurrentUserDetail.FullName} | {ApplicationConstants.WorkFromHome} request | {nameof(ItemStatus.Submitted)}"
            };

            await _eMailManager.SendMailAsync(emailSenderModal);
        }
    }
}
