using BottomhalfCore.DatabaseLayer.Common.Code;
using BottomhalfCore.Services.Code;
using BottomhalfCore.Services.Interface;
using EAGetMail;
using Google.Protobuf;
using ModalLayer;
using ModalLayer.Modal;
using ModalLayer.Modal.Accounts;
using Newtonsoft.Json;
using ServiceLayer.Code.SendEmail;
using ServiceLayer.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace ServiceLayer.Code
{
    public class AttendanceService : IAttendanceService
    {
        private readonly IDb _db;
        private readonly CurrentSession _currentSession;
        private readonly ITimezoneConverter _timezoneConverter;
        private readonly AttendanceEmailService _attendanceEmailService;
        private readonly ICompanyService _companyService;

        public AttendanceService(IDb db,
            ITimezoneConverter timezoneConverter,
            CurrentSession currentSession,
            ICompanyService companyService,
            AttendanceEmailService attendanceEmailService)
        {
            _db = db;
            _companyService = companyService;
            _currentSession = currentSession;
            _timezoneConverter = timezoneConverter;
            _attendanceEmailService = attendanceEmailService;
        }

        private DateTime GetBarrierDate(int limit)
        {
            int i = limit;
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

        private List<AttendenceDetail> CreateAttendanceTillDate(AttendanceDetailBuildModal attendanceModal)
        {
            List<AttendenceDetail> attendenceDetails = new List<AttendenceDetail>();
            if (!string.IsNullOrEmpty(attendanceModal.attendance.AttendanceDetail))
                attendenceDetails = JsonConvert.DeserializeObject<List<AttendenceDetail>>(attendanceModal.attendance.AttendanceDetail);

            DateTime presentDate = _timezoneConverter.ToTimeZoneDateTime(attendanceModal.presentDate, _currentSession.TimeZone);
            var firstDate = _timezoneConverter.ToTimeZoneDateTime(attendanceModal.firstDate, _currentSession.TimeZone);

            double days = 0;
            var barrierDate = GetBarrierDate(attendanceModal.attendanceSubmissionLimit);

            while (presentDate.Date.Subtract(firstDate.Date).TotalDays >= 0)
            {
                var detail = attendenceDetails
                    .Find(x => _timezoneConverter.ToTimeZoneDateTime(x.AttendanceDay, _currentSession.TimeZone).Date
                    .Subtract(firstDate.Date).TotalDays == 0);

                var isHoliday = CheckIsHoliday(firstDate, attendanceModal.calendars);
                var isWeekend = CheckWeekend(firstDate);
                var totalMinute = attendanceModal.shiftDetail.Duration;
                var officetime = attendanceModal.shiftDetail.OfficeTime;
                days = firstDate.Date.Subtract(barrierDate.Date).TotalDays;
                if (isHoliday || isWeekend)
                {
                    officetime = "00:00";
                    totalMinute = 0;
                }

                if (detail == null)
                {
                    attendenceDetails.Add(new AttendenceDetail
                    {
                        IsActiveDay = false,
                        TotalDays = DateTime.DaysInMonth(presentDate.Year, presentDate.Month),
                        AttendanceDay = firstDate,
                        AttendanceId = attendanceModal.attendance.AttendanceId,
                        AttendenceStatus = (int)DayStatus.WorkFromOffice,
                        BillingHours = totalMinute,
                        ClientId = 0,
                        DaysPending = DateTime.DaysInMonth(firstDate.Year, firstDate.Month),
                        EmployeeUid = attendanceModal.attendance.EmployeeId,
                        ForMonth = firstDate.Month,
                        ForYear = firstDate.Year,
                        TotalMinutes = totalMinute,
                        IsHoliday = isHoliday,
                        IsOnLeave = false,
                        IsWeekend = isWeekend,
                        LeaveId = 0,
                        UserComments = string.Empty,
                        UserTypeId = (int)UserType.Employee,
                        IsOpen = days >= 0 ? true : false,
                        LogOn = officetime,
                        LogOff = "00:00",
                        SessionType = attendanceModal.SessionType,
                        LunchBreanInMinutes = attendanceModal.shiftDetail.LunchDuration
                    });
                }


                firstDate = firstDate.AddDays(1);
            }

            return attendenceDetails;
        }

        private bool CheckWeekend(DateTime date)
        {
            bool flag = false;

            if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                flag = true;

            return flag;
        }

        private bool CheckIsHoliday(DateTime date, List<Calendar> calendars)
        {
            bool flag = false;

            var records = calendars.FirstOrDefault(x => x.StartDate.Date >= date.Date && x.EndDate.Date <= date.Date);
            if (records != null)
                flag = true;

            return flag;
        }

        public AttendanceWithClientDetail GetAttendanceByUserId(AttendenceDetail attendenceDetail)
        {
            List<AttendenceDetail> attendenceDetails = new List<AttendenceDetail>();
            Employee employee = null;
            AttendanceDetailBuildModal attendanceDetailBuildModal = new AttendanceDetailBuildModal();

            if (attendenceDetail.ForMonth <= 0)
                throw new HiringBellException("Invalid month num. passed.", nameof(attendenceDetail.ForMonth), attendenceDetail.ForMonth.ToString());

            var Result = _db.FetchDataSet("sp_attendance_get", new
            {
                EmployeeId = attendenceDetail.EmployeeUid,
                UserTypeId = attendenceDetail.UserTypeId,
                ForYear = attendenceDetail.ForYear,
                ForMonth = attendenceDetail.ForMonth,
                CompanyId = attendenceDetail.CompanyId
            });

            if (Result.Tables.Count != 4)
                throw HiringBellException.ThrowBadRequest("Fail to get attendance detail. Please contact to admin.");

            if (Result.Tables[3].Rows.Count != 1)
                throw HiringBellException.ThrowBadRequest("Company regular shift is not configured. Please complete company setting first.");

            attendanceDetailBuildModal.shiftDetail = Converter.ToType<ShiftDetail>(Result.Tables[3]);

            if (!ApplicationConstants.ContainSingleRow(Result.Tables[1]))
                throw new HiringBellException("Err!! fail to get employee detail. Plaese contact to admin.");

            employee = Converter.ToType<Employee>(Result.Tables[1]);

            if (ApplicationConstants.ContainSingleRow(Result.Tables[0]) && !string.IsNullOrEmpty(Result.Tables[0].Rows[0]["AttendanceDetail"].ToString()))
            {
                attendanceDetailBuildModal.attendance = Converter.ToType<Attendance>(Result.Tables[0]);
            }
            else
            {
                attendanceDetailBuildModal.attendance = new Attendance
                {
                    AttendanceDetail = "[]",
                    AttendanceId = 0,
                    EmployeeId = attendenceDetail.EmployeeUid,
                };

            }

            attendanceDetailBuildModal.calendars = Converter.ToList<Calendar>(Result.Tables[2]);

            attendanceDetailBuildModal.attendanceSubmissionLimit = employee.AttendanceSubmissionLimit;
            attendanceDetailBuildModal.presentDate = (DateTime)attendenceDetail.AttendenceToDay;
            attendanceDetailBuildModal.firstDate = _timezoneConverter.GetUtcFirstDay(attendanceDetailBuildModal.presentDate.Year, attendanceDetailBuildModal.presentDate.Month);
            if (attendanceDetailBuildModal.presentDate.Year == employee.CreatedOn.Year
                && attendanceDetailBuildModal.presentDate.Month == employee.CreatedOn.Month)
                attendanceDetailBuildModal.firstDate = employee.CreatedOn;

            attendenceDetails = CreateAttendanceTillDate(attendanceDetailBuildModal);
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

            var currentAttendance = _db.Get<Attendance>("sp_attendance_detall_pending", new
            {
                EmployeeId = employeeId,
                UserTypeId = UserTypeId == 0 ? _currentSession.CurrentUserDetail.UserTypeId : UserTypeId,
                ForYear = current.Year,
                ForMonth = current.Month
            });

            if (currentAttendance == null)
                throw new HiringBellException("Fail to get attendance detail. Please contact to admin.");

            attendanceSet = JsonConvert.DeserializeObject<List<AttendenceDetail>>(currentAttendance.AttendanceDetail);
            return attendanceSet;
        }

        public async Task<string> SubmitAttendanceService(AttendenceDetail attendenceApplied)
        {
            string Result = string.Empty;
            var attendancemonth = _timezoneConverter.ToIstTime(attendenceApplied.AttendanceDay).Month;
            var attendanceyear = _timezoneConverter.ToIstTime(attendenceApplied.AttendanceDay).Year;
            // this value should come from database as configured by user.
            int dailyWorkingHours = 9;
            DateTime workingDate = (DateTime)attendenceApplied.AttendenceFromDay;
            DateTime workingDateUserTimezone = _timezoneConverter.ToTimeZoneDateTime(workingDate, _currentSession.TimeZone);

            if (workingDate.Subtract((DateTime)attendenceApplied.AttendenceToDay).TotalDays != 0)
                throw new HiringBellException("Apply attendace only for one day. Multiple days attendance is not allowed.");

            var attendanceList = new List<AttendenceDetail>();

            // check back date limit to allow attendance
            var companySetting = await _companyService.GetCompanySettingByCompanyId(_currentSession.CurrentUserDetail.CompanyId);
            DateTime barrierDate = this.GetBarrierDate(companySetting.AttendanceSubmissionLimit);
            if (attendenceApplied.AttendanceDay.Subtract(barrierDate).TotalDays < 0)
                throw new HiringBellException("Ops!!! You are not allow to submit this date attendace. Please raise a request to your direct manager.");

            // check for leave, holiday and weekends
            await this.IsGivenDateAllowed(workingDate, workingDateUserTimezone);
            attendenceApplied.AttendanceDay = workingDate;
            (Attendance attendance, Employee employee) = _db.Get<Attendance, Employee>("sp_Attendance_YearMonth", new
            {
                EmployeeId = attendenceApplied.EmployeeUid,
                UserTypeId = attendenceApplied.UserTypeId,
                ForMonth = attendancemonth,
                ForYear = attendanceyear
            });
            if (employee == null)
                throw HiringBellException.ThrowBadRequest("Employee deatil not found. Please contact to admin");

            if (attendance != null && !string.IsNullOrEmpty(attendance.AttendanceDetail))
            {
                attendanceList = JsonConvert
                    .DeserializeObject<List<AttendenceDetail>>(attendance.AttendanceDetail);
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
            else
            {
                attendance = new Attendance();
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
                                        select new // AttendenceDetail use dynamic object for minimal json data
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
                                            Emails = n.EmailList == null ? "[]" : JsonConvert.SerializeObject(n.EmailList),
                                            ManagerName = _currentSession.CurrentUserDetail.ManagerName,
                                            LogOn = n.LogOn,
                                            LogOff = n.LogOff,
                                            SessionType = n.SessionType,
                                            LunchBreanInMinutes = n.LunchBreanInMinutes
                                        }));

            string ProjectName = string.Empty;
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
                UserId = _currentSession.CurrentUserDetail.UserId,
                PendingRequestCount = 1
            }, true);

            if (string.IsNullOrEmpty(Result))
                throw new HiringBellException("Unable submit the attendace");

            Task task = Task.Run(async () => await _attendanceEmailService.SendSubmitAttendanceEmail(attendenceApplied));
            return Result;
        }

        public async Task<List<CompalintOrRequest>> GetMissingAttendanceRequestService(FilterModel filter)
        {
            if (string.IsNullOrEmpty(filter.SearchString) || filter.EmployeeId > 0)
                filter.SearchString = $"1=1 and EmployeeId = {filter.EmployeeId}";

            var result = _db.GetList<CompalintOrRequest>("sp_complaint_or_request_get_by_employeeid", new
            {
                filter.SearchString,
                filter.SortBy,
                filter.PageSize,
                filter.PageIndex
            });

            return await Task.FromResult(result);
        }

        public async Task<string> RaiseMissingAttendanceRequestService(CompalintOrRequest compalintOrRequest)
        {
            if (compalintOrRequest.RequestedId == 0)
                throw HiringBellException.ThrowBadRequest("Invalid attendance selected. Please check your form again.");

            DateTime workingDate = (DateTime)compalintOrRequest.AttendanceDate;
            DateTime workingDateUserTimezone = _timezoneConverter.ToTimeZoneDateTime(workingDate, _currentSession.TimeZone);

            (Attendance attendance, Employee managerDetail) = _db.Get<Attendance, Employee>("sp_Attendance_YearMonth", new
            {
                EmployeeId = _currentSession.CurrentUserDetail.ReportingManagerId,
                UserTypeId = UserType.Employee,
                ForMonth = workingDateUserTimezone.Month,
                ForYear = workingDateUserTimezone.Year
            });

            if (managerDetail == null)
                throw HiringBellException.ThrowBadRequest("Employee deatil not found. Please contact to admin");


            var Result = _db.Execute<CompalintOrRequest>("sp_compalint_or_request_InsUpdate", new
            {
                CompalintOrRequestId = compalintOrRequest.CompalintOrRequestId,
                RequestTypeId = (int)RequestType.Attandance,
                RequestedId = compalintOrRequest.RequestedId,
                EmployeeId = compalintOrRequest.EmployeeId,
                EmployeeName = compalintOrRequest.EmployeeName,
                Email = compalintOrRequest.Email,
                Mobile = compalintOrRequest.Mobile,
                ManageId = _currentSession.CurrentUserDetail.ReportingManagerId,
                ManagerName = managerDetail.FirstName + " " + managerDetail.LastName,
                ManagerEmail = managerDetail.Email,
                ManagerMobile = managerDetail.Mobile,
                EmployeeMessage = compalintOrRequest.EmployeeMessage,
                ManagerComments = string.Empty,
                CurrentStatus = (int)ItemStatus.Pending,
                RequestForDate = ApplicationConstants.NullValue,
                AttendanceDate = compalintOrRequest.AttendanceDate,
                LeaveFromDate = ApplicationConstants.NullValue,
                LeaveToDate = ApplicationConstants.NullValue
            }, true);

            return await Task.FromResult(Result);
        }

        public AttendanceWithClientDetail EnablePermission(AttendenceDetail attendenceDetail)
        {
            if (attendenceDetail.ForMonth <= 0)
                throw new HiringBellException("Invalid month num. passed.", nameof(attendenceDetail.ForMonth), attendenceDetail.ForMonth.ToString());

            if (Convert.ToDateTime(attendenceDetail.AttendenceFromDay).Subtract(DateTime.UtcNow).TotalDays > 0)
            {
                throw new HiringBellException("Ohh!!!. Future dates are now allowed.");
            }


            var Result = _db.FetchDataSet("sp_attendance_get", new
            {
                EmployeeId = attendenceDetail.EmployeeUid,
                ClientId = attendenceDetail.ClientId,
                UserTypeId = attendenceDetail.UserTypeId,
                ForYear = attendenceDetail.ForYear,
                ForMonth = attendenceDetail.ForMonth
            });

            return null;
        }

        public dynamic GetEmployeePerformanceService(AttendenceDetail attendanceDetail)
        {
            if (attendanceDetail.EmployeeUid <= 0)
                throw HiringBellException.ThrowBadRequest("Invalid employee. Please login again");
            var result = _db.GetList<Attendance>("sp_employee_performance_get", new
            {
                EmployeeId = attendanceDetail.EmployeeUid,
                UserTypeId = attendanceDetail.UserTypeId,
                ForYear = attendanceDetail.ForYear
            });

            var monthlyAttendance = result.Find(x => x.ForMonth == attendanceDetail.ForMonth);

            return new { MonthlyAttendance = monthlyAttendance, YearlyAttendance = result };
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
            CalculateLogOffTime(attendenceDetail);
            await Task.CompletedTask;
        }

        private void CalculateLogOffTime(AttendenceDetail attendenceDetail)
        {
            var logontime = attendenceDetail.LogOn.Replace(":", ".");
            decimal logon = decimal.Parse(logontime);
            var totaltime = 0;
            if (attendenceDetail.SessionType == 1)
                totaltime = (int)(logon * 60 - attendenceDetail.LunchBreanInMinutes);
            else
                totaltime = (int)(logon * 60 - attendenceDetail.LunchBreanInMinutes) / 2;
            var time = ConvertToMin(totaltime);
            attendenceDetail.LogOff = time.ToString();
        }

        public String ConvertToMin(int mins)
        {
            int hours = (mins - mins % 60) / 60;
            return "" + hours + ":" + (mins - hours * 60);
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

            var Result = _db.Execute<string>(procedure, new
            {
                AttendanceId = currentAttendance.AttendanceId,
                EmployeeId = currentAttendance.EmployeeId,
                UserTypeId = currentAttendance.UserTypeId,
                AttendanceDetail = AttendaceDetail,
                TotalDays = currentAttendance.TotalDays,
                TotalWeekDays = currentAttendance.TotalWeekDays,
                DaysPending = currentAttendance.DaysPending,
                TotalBurnedMinutes = MonthsMinutes,
                ForYear = currentAttendance.ForYear,
                ForMonth = currentAttendance.ForMonth,
                UserId = _currentSession.CurrentUserDetail.UserId
            }, true);

            if (string.IsNullOrEmpty(Result))
                throw new HiringBellException("Fail to insert or update attendance detail. Pleasa contact to admin.");

            return Result;
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
    }
}
