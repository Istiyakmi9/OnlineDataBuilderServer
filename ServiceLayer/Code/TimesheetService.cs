using BottomhalfCore.DatabaseLayer.Common.Code;
using BottomhalfCore.Services.Code;
using BottomhalfCore.Services.Interface;
using DocumentFormat.OpenXml.Math;
using ModalLayer.Modal;
using Newtonsoft.Json;
using ServiceLayer.Caching;
using ServiceLayer.Code.SendEmail;
using ServiceLayer.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
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

        public async Task RunWeeklyTimesheetCreation(DateTime TimesheetStartDate)
        {
            try
            {
                var counts = await _db.ExecuteAsync("sp_timesheet_runweekly_data", new
                {
                    TimesheetStartDate = TimesheetStartDate
                }, true);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            await Task.CompletedTask;
        }

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

        private async Task CreateTimesheetWeekDays(TimesheetDetail timesheetDetail, ShiftDetail shiftDetail)
        {
            List<WeeklyTimesheetDetail> weeklyTimesheetDetails = new List<WeeklyTimesheetDetail>();
            DateTime startDate = _timezoneConverter.ToTimeZoneDateTime(timesheetDetail.TimesheetStartDate, _currentSession.TimeZone);
            DateTime endDate = _timezoneConverter.ToTimeZoneDateTime(timesheetDetail.TimesheetEndDate, _currentSession.TimeZone);

            while (startDate.Date.Subtract(endDate.Date).TotalDays <= 0)
            {
                var item = timesheetDetail.TimesheetWeeklyData.Find(x => x.WeekDay == startDate.DayOfWeek);
                if (item == null)
                {
                    var isweekened = false;
                    switch (startDate.DayOfWeek)
                    {
                        case DayOfWeek.Sunday:
                            isweekened = !shiftDetail.IsSun;
                            break;
                        case DayOfWeek.Monday:
                            isweekened = !shiftDetail.IsMon;
                            break;
                        case DayOfWeek.Tuesday:
                            isweekened = !shiftDetail.IsTue;
                            break;
                        case DayOfWeek.Wednesday:
                            isweekened = !shiftDetail.IsWed;
                            break;
                        case DayOfWeek.Thursday:
                            isweekened = !shiftDetail.IsThu;
                            break;
                        case DayOfWeek.Friday:
                            isweekened = !shiftDetail.IsFri;
                            break;
                        case DayOfWeek.Saturday:
                            isweekened = !shiftDetail.IsSat;
                            break;
                    }
                    weeklyTimesheetDetails.Add(new WeeklyTimesheetDetail
                    {
                        WeekDay = startDate.DayOfWeek,
                        PresentDate = startDate,
                        ActualBurnedMinutes = 0,
                        IsHoliday = false,
                        IsWeekEnd = isweekened,
                        ExpectedBurnedMinutes = isweekened ? 0 :shiftDetail.Duration
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
            if (timesheetDetail.TimesheetId <= 0)
                throw new HiringBellException("Invalid Timesheet id passed.");

            (TimesheetDetail timesheet, ShiftDetail shiftDetail) = _db.Get<TimesheetDetail, ShiftDetail>("sp_employee_timesheet_getby_id", new
            {
                TimesheetId = timesheetDetail.TimesheetId
            });

            if (shiftDetail == null)
                throw HiringBellException.ThrowBadRequest("Shift detail not found");

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

            await CreateTimesheetWeekDays(timesheet, shiftDetail);
            return timesheet;
        }

        private string UpdateOrInsertTimesheetDetail(TimesheetDetail timeSheetDetail, ShiftDetail shiftDetail)
        {
            int ExpectedBurnedMinutes = 0;
            int ActualBurnedMinutes = 0;
            timeSheetDetail.TimesheetWeeklyJson = JsonConvert.SerializeObject(timeSheetDetail.TimesheetWeeklyData);
            timeSheetDetail.TimesheetWeeklyData.ForEach(i =>
            {
                ExpectedBurnedMinutes += shiftDetail.Duration;
                ActualBurnedMinutes += i.ActualBurnedMinutes;
            });

            var result = _db.Execute<TimesheetDetail>(ApplicationConstants.InsertUpdateTimesheet, new
            {
                timeSheetDetail.TimesheetId,
                timeSheetDetail.EmployeeId,
                timeSheetDetail.ClientId,
                timeSheetDetail.TimesheetWeeklyJson,
                ExpectedBurnedMinutes = ExpectedBurnedMinutes,
                ActualBurnedMinutes = ActualBurnedMinutes,
                TotalWeekDays = shiftDetail.TotalWorkingDays,
                TotalWorkingDays = timeSheetDetail.TimesheetWeeklyData.Count(i => i.ActualBurnedMinutes > 0),
                timeSheetDetail.TimesheetStatus,
                timeSheetDetail.TimesheetStartDate,
                timeSheetDetail.TimesheetEndDate,
                timeSheetDetail.UserComments,
                timeSheetDetail.ForYear,
                AdminId = _currentSession.CurrentUserDetail.UserId
            }, true);

            if (string.IsNullOrEmpty(result))
                return null;
            return result;
        }

        public async Task<TimesheetDetail> SubmitTimesheetService(TimesheetDetail timesheetDetail)
        {
            if (timesheetDetail == null || timesheetDetail.TimesheetWeeklyData == null || timesheetDetail.TimesheetWeeklyData.Count == 0)
                throw HiringBellException.ThrowBadRequest("Invalid data submitted. Please check you detail.");

            if (timesheetDetail.ClientId <= 0)
                throw HiringBellException.ThrowBadRequest("Invalid data submitted. Client id is not valid.");

            ShiftDetail shiftDetail = _db.Get<ShiftDetail>("sp_work_shifts_by_clientId", new { ClientId = timesheetDetail.ClientId });

            timesheetDetail.TimesheetStatus = (int)ItemStatus.Submitted;
            var result = this.UpdateOrInsertTimesheetDetail(timesheetDetail, shiftDetail);
            if (string.IsNullOrEmpty(result))
                throw new HiringBellException("Unable to insert/update record. Please contact to admin.");

            _timesheetEmailService.SendSubmitTimesheetEmail(timesheetDetail);
            return await Task.FromResult(timesheetDetail);
        }

        public async Task<string> ExecuteActionOnTimesheetService(TimesheetDetail timesheetDetail)
        {
            if (timesheetDetail == null || timesheetDetail.TimesheetWeeklyData == null || timesheetDetail.TimesheetWeeklyData.Count == 0)
                throw HiringBellException.ThrowBadRequest("Invalid data submitted. Please check you detail.");

            if (timesheetDetail.ClientId <= 0)
                throw HiringBellException.ThrowBadRequest("Invalid data submitted. Client id is not valid.");

            ShiftDetail shiftDetail = _db.Get<ShiftDetail>("sp_work_shifts_by_clientId", new { ClientId = timesheetDetail.ClientId });

            timesheetDetail.TimesheetStatus = (int)ItemStatus.Submitted;
            var result = this.UpdateOrInsertTimesheetDetail(timesheetDetail, shiftDetail);
            if (string.IsNullOrEmpty(result))
                throw new HiringBellException("Unable to insert/update record. Please contact to admin.");

            _timesheetEmailService.SendSubmitTimesheetEmail(timesheetDetail);
            return await Task.FromResult("successfull");
        }

        public List<TimesheetDetail> GetTimesheetByFilterService(FilterModel filterModel)
        {
            var result = _db.GetList<TimesheetDetail>("SP_employee_timesheet_filter", new
            {
                filterModel.SearchString,
                filterModel.PageIndex,
                filterModel.PageSize,
                filterModel.SortBy
            });
            return result;
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
