using BottomhalfCore.DatabaseLayer.Common.Code;
using BottomhalfCore.Services.Code;
using BottomhalfCore.Services.Interface;
using ModalLayer.Modal;
using Newtonsoft.Json;
using NUnit.Framework.Constraints;
using ServiceLayer.Caching;
using ServiceLayer.Code.SendEmail;
using ServiceLayer.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ServiceLayer.Code
{
    public class TimesheetService : ITimesheetService
    {
        private readonly IDb _db;
        private readonly ITimezoneConverter _timezoneConverter;
        private readonly CurrentSession _currentSession;
        private readonly ICacheManager _cacheManager;
        private readonly TimesheetEmailService _timesheetEmailService;

        public TimesheetService(
            IDb db,
            ITimezoneConverter timezoneConverter,
            CurrentSession currentSession,
            ICacheManager cacheManager,
            TimesheetEmailService timesheetEmailService)
        {
            _db = db;
            _cacheManager = cacheManager;
            _timezoneConverter = timezoneConverter;
            _currentSession = currentSession;
            _timesheetEmailService = timesheetEmailService;
        }

        #region NEW CODE

        public List<TimesheetDetail> GetTimesheetByUserIdService(TimesheetDetail timesheetDetail)
        {
            if (timesheetDetail.EmployeeId <= 0 || timesheetDetail.ClientId <= 0)
                throw new HiringBellException("Invalid Employee or Client id passed.");

            var Result = _db.GetList<TimesheetDetail>("sp_employee_timesheet_get", new
            {
                EmployeeId = timesheetDetail.EmployeeId,
                ClientId = timesheetDetail.ClientId,
                TimesheetStatus = (int)ItemStatus.Pending,
                ForYear = timesheetDetail.ForYear
            });

            if (Result == null)
                throw HiringBellException.ThrowBadRequest("Unable to get client detail. Please contact to admin.");

            return Result;
        }

        private async Task CreateTimesheetWeekDays(TimesheetDetail timesheetDetail)
        {
            List<WeeklyTimesheetDetail> weeklyTimesheetDetails = new List<WeeklyTimesheetDetail>();
            DateTime startDate = _timezoneConverter.ToTimeZoneDateTime(timesheetDetail.TimesheetStartDate, _currentSession.TimeZone);
            DateTime endDate = _timezoneConverter.ToTimeZoneDateTime(timesheetDetail.TimesheetEndDate, _currentSession.TimeZone);

            while (startDate.Date.Subtract(endDate.Date).TotalDays <= 0)
            {
                var item = timesheetDetail.TimesheetWeeklyData.Find(x => x.WeekDay == startDate.DayOfWeek);
                if (item == null)
                {
                    weeklyTimesheetDetails.Add(new WeeklyTimesheetDetail
                    {
                        WeekDay = startDate.DayOfWeek,
                        PresentDate = startDate,
                        ActualBurnedMinutes = 0,
                        IsHoliday = false,
                        IsWeekEnd = false,
                        ExpectedBurnedMinutes = 0
                    });
                }
                else
                {
                    weeklyTimesheetDetails.Add(item);
                }

                startDate = startDate.AddDays(1);
            }

            timesheetDetail.TimesheetWeeklyData = weeklyTimesheetDetails;
            await Task.CompletedTask;
        }

        public async Task<TimesheetDetail> GetWeekTimesheetDataService(TimesheetDetail timesheetDetail)
        {
            if (timesheetDetail.EmployeeId <= 0 || timesheetDetail.ClientId <= 0)
                throw new HiringBellException("Invalid Employee or Client id passed.");

            var timesheet = _db.Get<TimesheetDetail>("sp_employee_timesheet_get_by_status", new
            {
                EmployeeId = timesheetDetail.EmployeeId,
                ClientId = timesheetDetail.ClientId,
                TimesheetStatus = (int)ItemStatus.Pending,
                timesheetDetail.TimesheetStartDate,
                ForYear = timesheetDetail.ForYear
            });

            if (timesheet == null)
            {
                timesheet = timesheetDetail;
                timesheet.TimesheetWeeklyData = new List<WeeklyTimesheetDetail>();
                timesheet.TimesheetStartDate = _timezoneConverter.ToTimeZoneDateTime(timesheetDetail.TimesheetStartDate, _currentSession.TimeZone);
                timesheet.TimesheetEndDate = timesheet.TimesheetStartDate.AddDays(6);
            }
            else
            {
                if (!string.IsNullOrEmpty(timesheet.TimesheetWeeklyJson))
                    timesheet.TimesheetWeeklyData = JsonConvert.DeserializeObject<List<WeeklyTimesheetDetail>>(timesheet.TimesheetWeeklyJson);
                else
                    timesheet.TimesheetWeeklyData = new List<WeeklyTimesheetDetail>();

            }

            await CreateTimesheetWeekDays(timesheet);
            return timesheet;
        }

        #endregion



        private List<DailyTimesheetDetail> BuildTimesheetTillDate(TimesheetDetail timesheetDetail, Employee employee)
        {
            UpdateDateOfJoining(timesheetDetail, employee);

            List<DailyTimesheetDetail> timesheets = new List<DailyTimesheetDetail>();
            DateTime now = DateTime.Now; //timesheetDetail.TimesheetToDate;
            DateTime presentDate = DateTime.Now; //timesheetDetail.TimesheetFromDate;

            while (now.Subtract(presentDate).TotalDays >= 0)
            {
                timesheets.Add(new DailyTimesheetDetail
                {
                    ClientId = timesheetDetail.ClientId,
                    EmployeeId = timesheetDetail.EmployeeId,
                    TimesheetId = 0,
                    TotalMinutes = 8 * 60,
                    TimesheetStatus = ItemStatus.Generated,
                    PresentDate = presentDate,
                    IsHoliday = false,
                    IsWeekEnd = false,
                    UserComments = string.Empty,
                    UserTypeId = (int)UserType.Employee,
                    EmployeeName = "",
                    Email = "",
                    ManagerName = "",
                    Mobile = "",
                    ReportingManagerId = 0

                });

                presentDate = presentDate.AddDays(1);
            }

            return timesheets;
        }

        private void RemoveHolidaysAndWeekOff(List<DailyTimesheetDetail> dailyTimesheetDetails, ShiftDetail shiftDetail)
        {
            DateTime date = DateTime.Now;
            dailyTimesheetDetails.ForEach(item =>
            {
                date = _timezoneConverter.ToTimeZoneDateTime(item.PresentDate, _currentSession.TimeZone);

                switch (date.DayOfWeek)
                {
                    case DayOfWeek.Sunday:
                        if (!shiftDetail.IsSun)
                            item.IsWeekEnd = true;
                        else
                            item.IsWeekEnd = false;
                        break;
                    case DayOfWeek.Monday:
                        if (!shiftDetail.IsMon)
                            item.IsWeekEnd = true;
                        else
                            item.IsWeekEnd = false;
                        break;
                    case DayOfWeek.Tuesday:
                        if (!shiftDetail.IsTue)
                            item.IsWeekEnd = true;
                        else
                            item.IsWeekEnd = false;
                        break;
                    case DayOfWeek.Wednesday:
                        if (!shiftDetail.IsWed)
                            item.IsWeekEnd = true;
                        else
                            item.IsWeekEnd = false;
                        break;
                    case DayOfWeek.Thursday:
                        if (!shiftDetail.IsThu)
                            item.IsWeekEnd = true;
                        else
                            item.IsWeekEnd = false;
                        break;
                    case DayOfWeek.Friday:
                        if (!shiftDetail.IsFri)
                            item.IsWeekEnd = true;
                        else
                            item.IsWeekEnd = false;
                        break;
                    case DayOfWeek.Saturday:
                        if (!shiftDetail.IsSat)
                            item.IsWeekEnd = true;
                        else
                            item.IsWeekEnd = false;
                        break;
                }
            });

            dailyTimesheetDetails = dailyTimesheetDetails.OrderByDescending(x => x.PresentDate).ToList();
        }

        private void UpdateDateOfJoining(TimesheetDetail timesheetDetail, Employee employee)
        {
            if (employee.DateOfJoining == null)
                throw HiringBellException.ThrowBadRequest("Invalid date of joining. Please contact to admin.");

            var doj = _timezoneConverter.ToTimeZoneDateTime((DateTime)employee.DateOfJoining, _currentSession.TimeZone);
            var fromDate = _timezoneConverter.ToTimeZoneDateTime(timesheetDetail.TimesheetStartDate, _currentSession.TimeZone);
            if (fromDate.Date.Subtract(doj.Date).TotalDays < 0)
                timesheetDetail.TimesheetStartDate = (DateTime)employee.DateOfJoining;
        }

        private void IsGivenDateAllowed(DateTime From, DateTime To)
        {
            TimeZoneInfo timeZoneInfo = _currentSession.TimeZone;
            var fromDate = _timezoneConverter.ToTimeZoneDateTime(From, timeZoneInfo);
            var toDate = _timezoneConverter.ToTimeZoneDateTime(To, timeZoneInfo);

            var lastDayOfPresentWeek = _timezoneConverter.LastDayOfPresentWeek(DateTime.UtcNow, timeZoneInfo);
            var firstDayOfPreviousForthWeek = toDate.AddDays((-1) * 5 * 7);

            if (firstDayOfPreviousForthWeek.Date.Subtract(fromDate.Date).TotalDays > 0)
                throw new HiringBellException("Before 5 weeks date not allowed");

            if (lastDayOfPresentWeek.Date.Subtract(toDate.Date).TotalDays < 0)
                throw new HiringBellException("Future date attendance not allowed.");
        }

        private string UpdateOrInsertTimesheetDetail(List<DailyTimesheetDetail> finalDailyTimesheetDetails, TimesheetDetail currentTimesheet)
        {
            currentTimesheet.TimesheetWeeklyJson = JsonConvert.SerializeObject(finalDailyTimesheetDetails);
            var result = _db.Execute<TimesheetDetail>(ApplicationConstants.InsertUpdateTimesheet, new
            {
                currentTimesheet.TimesheetId,
                currentTimesheet.EmployeeId,
                currentTimesheet.ClientId,
                currentTimesheet.TimesheetWeeklyJson,
                currentTimesheet.ExpectedBurnedMinutes,
                currentTimesheet.ActualBurnedMinutes,
                currentTimesheet.TotalWeekDays,
                currentTimesheet.TotalWorkingDays,
                currentTimesheet.TimesheetStatus,
                currentTimesheet.TimesheetStartDate,
                currentTimesheet.TimesheetEndDate,
                currentTimesheet.UserComments,
                currentTimesheet.ForYear,
                AdminId = _currentSession.CurrentUserDetail.UserId
            }, true);

            if (string.IsNullOrEmpty(result))
                return null;
            return result;
        }

        public async Task<List<DailyTimesheetDetail>> InsertUpdateTimesheet(List<DailyTimesheetDetail> dailyTimesheetDetails)
        {
            string result = string.Empty;
            var firstItem = dailyTimesheetDetails.FirstOrDefault();
            if (firstItem == null)
                throw new HiringBellException("Invalid TimesheetDetail submitted.");

            var invalidAttendaceData = dailyTimesheetDetails.FindAll(x => x.EmployeeId <= 0 || x.ClientId <= 0);
            if (invalidAttendaceData.Count > 0)
                throw new HiringBellException("Invalid Employee/Client Id passed.");
            DateTime firstDate = firstItem.PresentDate;
            DateTime lastDate = firstItem.PresentDate;

            TimeZoneInfo timeZoneInfo = _currentSession.TimeZone;
            var fromDate = _timezoneConverter.ToTimeZoneDateTime(firstItem.PresentDate, timeZoneInfo);
            int j = 0;
            while (j < dailyTimesheetDetails.Count)
            {
                var x = dailyTimesheetDetails.ElementAt(j);
                if (x.PresentDate.Subtract(firstDate).TotalDays <= 0)
                    firstDate = x.PresentDate;

                if (x.PresentDate.Subtract(lastDate).TotalDays >= 0)
                    lastDate = x.PresentDate;

                j++;
            }

            var Result = _db.FetchDataSet("sp_employee_timesheet_get", new
            {
                firstItem.EmployeeId,
                firstItem.ClientId,
                firstItem.UserTypeId,
                ForYear = fromDate.Year,
                ForMonth = fromDate.Month
            });

            if (Result.Tables.Count != 2 && Result.Tables[0].Rows.Count == 0)
            {
                throw new HiringBellException("Timesheet detail is invalid.");
            }

            List<DailyTimesheetDetail> otherMonthTimesheetDetail = new List<DailyTimesheetDetail>();
            List<DailyTimesheetDetail> finalTimesheetSet = new List<DailyTimesheetDetail>();

            Employee employee = new Employee();
            if (Result.Tables[1].Rows.Count > 0)
                employee = Converter.ToType<Employee>(Result.Tables[1]);
            else
                throw new HiringBellException("User detail not found.");

            TimesheetDetail currentTimesheetDetail = null;
            if (Result.Tables[0].Rows.Count > 0)
            {
                currentTimesheetDetail = Converter.ToType<TimesheetDetail>(Result.Tables[0]);
                if (!string.IsNullOrEmpty(currentTimesheetDetail.TimesheetWeeklyJson))
                    finalTimesheetSet = JsonConvert.DeserializeObject<List<DailyTimesheetDetail>>(currentTimesheetDetail.TimesheetWeeklyJson);

                this.IsGivenDateAllowed(firstDate, lastDate);
            }
            else
            {
                var currentMonthDateTime = _timezoneConverter.ToIstTime(firstItem.PresentDate);
                int totalDays = DateTime.DaysInMonth(currentMonthDateTime.Year, currentMonthDateTime.Month);
                currentTimesheetDetail = new TimesheetDetail
                {
                    EmployeeId = firstItem.EmployeeId,
                    TimesheetId = 0,
                    ForYear = currentMonthDateTime.Year,
                    TimesheetWeeklyJson = "[]",
                    TimesheetStartDate = firstDate,
                    TimesheetEndDate = lastDate,
                    ClientId = firstItem.ClientId,
                    ExpectedBurnedMinutes = 0,
                    ActualBurnedMinutes = 0,
                    TotalWeekDays = 0,
                    TotalWorkingDays = 0,
                    TimesheetStatus = (int)ItemStatus.Pending
                };

                //finalTimesheetSet = this.GenerateWeekAttendaceData(currentTimesheetDetail);
                finalTimesheetSet = this.BuildTimesheetTillDate(currentTimesheetDetail, employee);
            }

            if (finalTimesheetSet == null)
                throw new HiringBellException("Unable to get record. Please contact to admin.");

            int i = 0;
            DateTime clientKindDateTime = DateTime.Now;
            while (i < dailyTimesheetDetails.Count)
            {
                var x = dailyTimesheetDetails.ElementAt(i);
                var item = finalTimesheetSet.Find(i => i.PresentDate.Subtract(x.PresentDate).TotalDays == 0);
                if (item != null)
                {
                    clientKindDateTime = _timezoneConverter.ToIstTime(x.PresentDate);
                    item.TotalMinutes = x.TotalMinutes;
                    item.UserTypeId = x.UserTypeId;
                    item.EmployeeId = x.EmployeeId;
                    item.TimesheetId = firstItem.TimesheetId;
                    item.UserComments = x.UserComments;
                    item.TimesheetStatus = ItemStatus.Pending;
                    item.ClientId = x.ClientId;
                    item.Email = employee.Email;
                    item.EmployeeName = string.Concat(employee.FirstName,
                                    " ",
                                    employee.LastName).Trim();
                    item.Mobile = employee.Mobile;
                    item.ReportingManagerId = employee.ReportingManagerId;
                    item.ManagerName = "NA";
                }
                else
                {
                    finalTimesheetSet.Add(new DailyTimesheetDetail
                    {
                        TotalMinutes = x.TotalMinutes,
                        UserTypeId = x.UserTypeId,
                        EmployeeId = x.EmployeeId,
                        TimesheetId = firstItem.TimesheetId,
                        UserComments = x.UserComments,
                        PresentDate = x.PresentDate,
                        TimesheetStatus = ItemStatus.Pending,
                        ClientId = x.ClientId,
                        Email = employee.Email,
                        EmployeeName = string.Concat(employee.FirstName,
                                        " ",
                                        employee.LastName).Trim(),
                        Mobile = employee.Mobile,
                        ReportingManagerId = employee.ReportingManagerId,
                        ManagerName = "NA"
                    });
                }

                i++;
            }

            result = this.UpdateOrInsertTimesheetDetail(finalTimesheetSet, currentTimesheetDetail);
            if (string.IsNullOrEmpty(result))
                throw new HiringBellException("Unable to insert/update record. Please contact to admin.");

            _timesheetEmailService.SendSubmitTimesheetEmail(currentTimesheetDetail);

            return await Task.FromResult(finalTimesheetSet);
        }

        public List<TimesheetDetail> GetPendingTimesheetByIdService(long employeeId, long clientId)
        {
            List<TimesheetDetail> timesheetDetail = new List<TimesheetDetail>();
            DateTime current = DateTime.UtcNow;

            var currentTimesheetDetail = _db.Get<TimesheetDetail>("sp_employee_timesheet_get", new
            {
                EmployeeId = employeeId,
                ClientId = clientId,
                UserTypeId = _currentSession.CurrentUserDetail.RoleId,
                ForYear = current.Year,
                ForMonth = current.Month,
            });

            timesheetDetail = JsonConvert.DeserializeObject<List<TimesheetDetail>>(currentTimesheetDetail.TimesheetWeeklyJson);
            return timesheetDetail;
        }

        public dynamic GetEmployeeTimeSheetService(TimesheetDetail timesheetDetail)
        {
            (TimesheetDetail currentTimesheetDetail, Employee employee) =
                _db.GetMulti<TimesheetDetail, Employee>("sp_employee_timesheet_getby_empid", new
                {
                    timesheetDetail.EmployeeId,
                    timesheetDetail.ForYear,
                    timesheetDetail.ClientId
                });

            if (currentTimesheetDetail == null)
                currentTimesheetDetail = new TimesheetDetail
                {
                    ForYear = timesheetDetail.ForYear
                };

            var result = BuildFinalTimesheet(currentTimesheetDetail);
            return new { TimesheetDetails = result.Item1, MissingDate = result.Item2 };
        }

        public (List<DailyTimesheetDetail>, List<DateTime>) BuildFinalTimesheet(TimesheetDetail currentTimesheetDetail)
        {
            List<DailyTimesheetDetail> dailyTimesheetDetails = new List<DailyTimesheetDetail>();
            List<DateTime> missingDayList = new List<DateTime>();
            if (currentTimesheetDetail != null && currentTimesheetDetail.TimesheetWeeklyJson != null)
            {
                dailyTimesheetDetails = JsonConvert
                    .DeserializeObject<List<DailyTimesheetDetail>>(currentTimesheetDetail.TimesheetWeeklyJson);

                Parallel.ForEach(dailyTimesheetDetails, x => x.TimesheetId = currentTimesheetDetail.TimesheetId);
            }
            else
            {
                currentTimesheetDetail = new TimesheetDetail
                {
                    ForYear = currentTimesheetDetail.ForYear
                };
            }

            return (dailyTimesheetDetails, missingDayList);
        }

        public void UpdateTimesheetService(List<DailyTimesheetDetail> dailyTimesheetDetails, TimesheetDetail timesheetDetail, string comment)
        {
            List<DateTime> missingDayList = new List<DateTime>();
            if (dailyTimesheetDetails == null || dailyTimesheetDetails.Count == 0 || timesheetDetail == null)
                throw new HiringBellException("Incorrect data passed. Please verify your input.");

            var firstDate = dailyTimesheetDetails.First().PresentDate;
            firstDate = _timezoneConverter.ToTimeZoneDateTime(firstDate, _currentSession.TimeZone);
            int days = DateTime.DaysInMonth(firstDate.Year, firstDate.Month);

            if (dailyTimesheetDetails.Count > days)
                throw new HiringBellException("Incorrect data passed. Please verify your input.");

            (TimesheetDetail currentTimesheetDetail, Employee employee) =
                _db.GetMulti<TimesheetDetail, Employee>("sp_employee_timesheet_getby_empid", new
                {
                    timesheetDetail.EmployeeId,
                    timesheetDetail.ForYear,
                    timesheetDetail.ClientId
                });

            if (employee == null)
                throw new HiringBellException("Employee detail not found. Please contact to admin.");

            var utcJoiningDate = _timezoneConverter.ToTimeZoneDateTime(employee.CreatedOn, _currentSession.TimeZone);
            if (currentTimesheetDetail == null)
                currentTimesheetDetail = timesheetDetail;

            UpdateTimesheetList(dailyTimesheetDetails, utcJoiningDate);

            int i = 0;
            DailyTimesheetDetail item = default(DailyTimesheetDetail);
            while (i < dailyTimesheetDetails.Count)
            {
                item = dailyTimesheetDetails.ElementAt(i);
                if (item.TimesheetStatus != ItemStatus.Approved)
                {
                    missingDayList.Add(item.PresentDate);
                    dailyTimesheetDetails.RemoveAt(i);
                }
                else
                    i++;

            }

            currentTimesheetDetail.TimesheetWeeklyJson = JsonConvert.SerializeObject(dailyTimesheetDetails);
            var result = this.UpdateOrInsertTimesheetDetail(dailyTimesheetDetails, currentTimesheetDetail);
            if (string.IsNullOrEmpty(result))
                throw new HiringBellException("Unable to insert/update record. Please contact to admin.");

            //return new { TimesheetDetails = dailyTimesheetDetails, MissingDate = missingDayList };
        }

        private void UpdateTimesheetList(List<DailyTimesheetDetail> dailyTimesheetDetails, DateTime utcJoiningDate)
        {
            var now = DateTime.UtcNow;
            Parallel.ForEach(dailyTimesheetDetails, i =>
            {
                if (utcJoiningDate.Date.
                    Subtract(_timezoneConverter.ToTimeZoneDateTime(i.PresentDate, _currentSession.TimeZone)).TotalDays > 0 ||
                    now.Date
                    .Subtract(_timezoneConverter.ToTimeZoneDateTime(i.PresentDate, _currentSession.TimeZone)).TotalDays < 0)
                    i.TimesheetStatus = ItemStatus.NotGenerated;
                else
                {
                    if (i.TimesheetStatus != ItemStatus.Approved)
                        i.TimesheetStatus = ItemStatus.Absent;
                    else
                        i.TimesheetStatus = ItemStatus.Approved;
                }
            });
        }

        public BillingDetail EditEmployeeBillDetailService(GenerateBillFileDetail fileDetail)
        {
            BillingDetail billingDetail = default(BillingDetail);
            var Result = _db.FetchDataSet("sp_EmployeeBillDetail_ById", new
            {
                AdminId = _currentSession.CurrentUserDetail.UserId,
                EmployeeId = fileDetail.EmployeeId,
                ClientId = fileDetail.ClientId,
                FileId = fileDetail.FileId,
                UserTypeId = fileDetail.UserTypeId,
                ForMonth = fileDetail.ForMonth,
                ForYear = fileDetail.ForYear
            });

            if (Result.Tables.Count == 3)
            {
                billingDetail = new BillingDetail();
                billingDetail.FileDetail = Result.Tables[0];
                billingDetail.Employees = Result.Tables[1];

                if (Result.Tables[2] == null || Result.Tables[2].Rows.Count == 0)
                {
                    bool flag = false;
                    billingDetail.TimesheetDetail = new TimesheetDetail
                    {
                        ForYear = 0
                    };

                    if (billingDetail.FileDetail.Rows[0]["BillForMonth"] == DBNull.Value)
                        flag = true;

                    if (billingDetail.FileDetail.Rows[0]["BillYear"] == DBNull.Value)
                        flag = true;
                    else
                        billingDetail.TimesheetDetail.ForYear = Convert.ToInt32(billingDetail.FileDetail.Rows[0]["BillYear"]);

                    DateTime billingOn = DateTime.Now;
                    if (flag)
                    {
                        billingOn = Convert.ToDateTime(billingDetail.FileDetail.Rows[0]["BillYear"]);
                        billingDetail.TimesheetDetail.ForYear = billingOn.Year;
                    }

                }
                else
                    billingDetail.TimesheetDetail = Converter.ToType<TimesheetDetail>(Result.Tables[2]);

                var attrs = BuildFinalTimesheet(billingDetail.TimesheetDetail);
                billingDetail.TimesheetDetails = attrs.Item1;
                billingDetail.MissingDate = attrs.Item2;

                var companies = _cacheManager.Get(CacheTable.Company);
                billingDetail.Organizations = companies;
            }

            return billingDetail;
        }
    }
}
