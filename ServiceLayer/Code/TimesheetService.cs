using BottomhalfCore.DatabaseLayer.Common.Code;
using BottomhalfCore.Services.Code;
using BottomhalfCore.Services.Interface;
using ModalLayer.Modal;
using Newtonsoft.Json;
using ServiceLayer.Caching;
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
        public TimesheetService(IDb db, ITimezoneConverter timezoneConverter, CurrentSession currentSession, ICacheManager cacheManager)
        {
            _db = db;
            _cacheManager = cacheManager;
            _timezoneConverter = timezoneConverter;
            _currentSession = currentSession;
        }


        private List<DailyTimesheetDetail> BuildTimesheetTillDate(TimesheetDetail timesheetDetail)
        {
            List<DailyTimesheetDetail> timesheets = new List<DailyTimesheetDetail>();
            DateTime now = timesheetDetail.TimesheetToDate;
            DateTime presentDate = timesheetDetail.TimesheetFromDate;

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
                    IsWeekEnd = (presentDate.DayOfWeek == DayOfWeek.Saturday
                                    ||
                                presentDate.DayOfWeek == DayOfWeek.Sunday) ? true : false,
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

        public dynamic GetTimesheetByUserIdService(TimesheetDetail timesheetDetail)
        {
            List<DailyTimesheetDetail> dailyTimesheetDetails = null;
            Employee employee = null;

            if (timesheetDetail.ForMonth <= 0)
                throw new HiringBellException("Invalid month num. passed.", nameof(timesheetDetail.ForMonth), timesheetDetail.ForMonth.ToString());

            if (timesheetDetail.EmployeeId <= 0 || timesheetDetail.ClientId <= 0)
                throw new HiringBellException("Invalid Employee or Client id passed.");

            DbParam[] dbParams = new DbParam[]
            {
                new DbParam(timesheetDetail.EmployeeId, typeof(int), "_EmployeeId"),
                new DbParam(timesheetDetail.ClientId, typeof(int), "_ClientId"),
                new DbParam(timesheetDetail.UserTypeId, typeof(int), "_UserTypeId"),
                new DbParam(timesheetDetail.ForYear, typeof(int), "_ForYear"),
                new DbParam(timesheetDetail.ForMonth, typeof(int), "_ForMonth")
            };

            var Result = _db.GetDataset("sp_employee_timesheet_get", dbParams);
            if (Result.Tables.Count == 2)
            {

                if (Result.Tables[1].Rows.Count > 0)
                    employee = Converter.ToType<Employee>(Result.Tables[1]);
                else
                    throw new HiringBellException("Invalid employee detail passed.");

                if (Result.Tables[0].Rows.Count > 0 && employee != null)
                {
                    var employeeTimesheetDetail = Converter.ToType<TimesheetDetail>(Result.Tables[0]);
                    var generatedTimesheet = BuildTimesheetTillDate(timesheetDetail);

                    if (!string.IsNullOrEmpty(employeeTimesheetDetail.TimesheetMonthJson))
                    {
                        dailyTimesheetDetails = JsonConvert.DeserializeObject<List<DailyTimesheetDetail>>(employeeTimesheetDetail.TimesheetMonthJson);

                        dailyTimesheetDetails.ForEach(x =>
                        {
                            x.TimesheetId = employeeTimesheetDetail.TimesheetId;
                            x.ClientId = employeeTimesheetDetail.ClientId;
                        });

                        int i = 0;
                        //var generatedTimesheet = this.GenerateWeekAttendaceData(timesheetDetail);
                        DailyTimesheetDetail attrDetail = null;
                        foreach (var item in dailyTimesheetDetails)
                        {
                            i = 0;
                            while (i < generatedTimesheet.Count)
                            {
                                attrDetail = generatedTimesheet.ElementAt(i);
                                if (attrDetail.PresentDate.Date.Subtract(item.PresentDate.Date).TotalDays == 0)
                                {
                                    generatedTimesheet[i] = item;
                                    break;
                                }
                                i++;
                            }
                        }

                        dailyTimesheetDetails = generatedTimesheet;

                    }
                    else
                        dailyTimesheetDetails = generatedTimesheet;
                }
                else
                {
                    //dailyTimesheetDetails = this.GenerateWeekAttendaceData(timesheetDetail);
                    dailyTimesheetDetails = BuildTimesheetTillDate(timesheetDetail);
                }

                if (this.IsRegisteredOnPresentWeek(employee.CreatedOn) == 1)
                {
                    dailyTimesheetDetails = dailyTimesheetDetails.Where(x => employee.CreatedOn.Date.Subtract(x.PresentDate.Date).TotalDays <= 0).ToList();
                }
            }

            return new { EmployeeDetail = employee, DailyTimesheetDetails = dailyTimesheetDetails.OrderByDescending(x => x.PresentDate).ToList() };
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

        private List<DailyTimesheetDetail> GenerateWeekAttendaceData(TimesheetDetail timesheetDetail, DateTime? monthFirstDate = null)
        {
            List<DailyTimesheetDetail> dailyTimesheetDetail = new List<DailyTimesheetDetail>();
            DateTime startDate = (DateTime)timesheetDetail.TimesheetFromDate;
            if (monthFirstDate != null)
                startDate = (DateTime)monthFirstDate;

            var endDate = (DateTime)timesheetDetail.TimesheetToDate;

            while (startDate.Subtract(endDate).TotalDays <= 0)
            {
                dailyTimesheetDetail.Add(new DailyTimesheetDetail
                {
                    EmployeeId = timesheetDetail.EmployeeId,
                    ClientId = timesheetDetail.ClientId,
                    TimesheetId = 0,
                    TotalMinutes = 8 * 60,
                    TimesheetStatus = ItemStatus.Pending,
                    PresentDate = startDate,
                    IsHoliday = false,
                    IsWeekEnd = (startDate.DayOfWeek == DayOfWeek.Saturday
                                    ||
                                startDate.DayOfWeek == DayOfWeek.Sunday) ? true : false,
                    UserComments = string.Empty,
                    UserTypeId = (int)UserType.Employee
                });

                startDate = startDate.AddDays(1);
            }

            return dailyTimesheetDetail;
        }

        private int IsRegisteredOnPresentWeek(DateTime RegistratedOn)
        {
            var weekFirstDate = _timezoneConverter.FirstDayOfWeekIST();
            var weekLastDate = _timezoneConverter.LastDayOfWeekIST();

            if (RegistratedOn.Date.Subtract(weekFirstDate.Date).TotalDays >= 0 && RegistratedOn.Date.Subtract(weekLastDate.Date).TotalDays <= 0)
            {
                return 1;
            }

            return 0;
        }

        private string UpdateOrInsertTimesheetDetail(List<DailyTimesheetDetail> finalDailyTimesheetDetails, TimesheetDetail currentTimesheet)
        {
            var TimesheetDetail = JsonConvert.SerializeObject(finalDailyTimesheetDetails);

            decimal MonthsMinutes = 0;
            currentTimesheet.DaysAbsent = 0;
            finalDailyTimesheetDetails.ForEach(x =>
            {
                MonthsMinutes += x.TotalMinutes;
                if ((int)x.TimesheetStatus == 8)
                    currentTimesheet.DaysAbsent++;
            });

            currentTimesheet.TimesheetMonthJson = JsonConvert.SerializeObject(finalDailyTimesheetDetails);
            var result = _db.Execute<TimesheetDetail>(ApplicationConstants.InsertUpdateTimesheet, new
            {
                currentTimesheet.TimesheetId,
                currentTimesheet.EmployeeId,
                currentTimesheet.ClientId,
                currentTimesheet.UserTypeId,
                currentTimesheet.TimesheetMonthJson,
                currentTimesheet.TotalDays,
                currentTimesheet.DaysAbsent,
                currentTimesheet.ExpectedBurnedMinutes,
                currentTimesheet.ActualBurnedMinutes,
                currentTimesheet.TotalWeekDays,
                currentTimesheet.TotalWorkingDays,
                currentTimesheet.TotalHolidays,
                currentTimesheet.MonthTimesheetApprovalState,
                currentTimesheet.ForYear,
                currentTimesheet.ForMonth,
                AdminId = _currentSession.CurrentUserDetail.UserId
            }, true);

            if (string.IsNullOrEmpty(result))
                return null;
            return result;
        }

        public List<DailyTimesheetDetail> InsertUpdateTimesheet(List<DailyTimesheetDetail> dailyTimesheetDetails)
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

            DbParam[] dbParams = new DbParam[]
            {
                new DbParam(firstItem.EmployeeId, typeof(int), "_EmployeeId"),
                new DbParam(firstItem.ClientId, typeof(int), "_ClientId"),
                new DbParam(firstItem.UserTypeId, typeof(int), "_UserTypeId"),
                new DbParam(fromDate.Year, typeof(int), "_ForYear"),
                new DbParam(fromDate.Month, typeof(int), "_ForMonth")
            };

            var Result = _db.GetDataset("sp_employee_timesheet_get", dbParams);
            if (Result.Tables.Count != 2 && Result.Tables[0].Rows.Count == 0)
            {
                throw new HiringBellException("Timesheet detail is invalid.");
            }

            List<DailyTimesheetDetail> otherMonthTimesheetDetail = new List<DailyTimesheetDetail>();
            List<DailyTimesheetDetail> finalTimesheetSet = new List<DailyTimesheetDetail>();

            TimesheetDetail currentTimesheetDetail = null;
            if (Result.Tables[0].Rows.Count > 0)
            {
                currentTimesheetDetail = Converter.ToType<TimesheetDetail>(Result.Tables[0]);
                if (!string.IsNullOrEmpty(currentTimesheetDetail.TimesheetMonthJson))
                    finalTimesheetSet = JsonConvert.DeserializeObject<List<DailyTimesheetDetail>>(currentTimesheetDetail.TimesheetMonthJson);

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
                    TimesheetMonthJson = "[]",
                    UserTypeId = (int)UserType.Employee,
                    TotalDays = totalDays,
                    DaysAbsent = totalDays,
                    ForMonth = currentMonthDateTime.Month,
                    TimesheetFromDate = firstDate,
                    TimesheetToDate = lastDate,
                    ClientId = firstItem.ClientId,
                    ExpectedBurnedMinutes = 0,
                    ActualBurnedMinutes = 0,
                    TotalWeekDays = 0,
                    TotalWorkingDays = 0,
                    TotalHolidays = 0,
                    MonthTimesheetApprovalState = (int)ItemStatus.Pending
                };

                //finalTimesheetSet = this.GenerateWeekAttendaceData(currentTimesheetDetail);
                finalTimesheetSet = this.BuildTimesheetTillDate(currentTimesheetDetail);
            }

            Employee employee = new Employee();
            if (Result.Tables[1].Rows.Count > 0)
                employee = Converter.ToType<Employee>(Result.Tables[1]);
            else
                throw new HiringBellException("User detail not found.");

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
                    if (x.PresentDate.Month != currentTimesheetDetail.ForMonth)
                    {
                        x.TimesheetId = -1;
                        otherMonthTimesheetDetail.Add(x);
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
                }

                i++;
            }

            if (this.IsRegisteredOnPresentWeek(employee.CreatedOn) == 1)
            {
                var utcJoiningDate = _timezoneConverter.ToUtcTime(employee.CreatedOn.Date);
                finalTimesheetSet = finalTimesheetSet.Where(x => utcJoiningDate.Date.Subtract(x.PresentDate.Date).TotalDays <= 0).ToList();
            }

            result = this.UpdateOrInsertTimesheetDetail(finalTimesheetSet, currentTimesheetDetail);
            if (string.IsNullOrEmpty(result))
            {
                throw new HiringBellException("Unable to insert/update record. Please contact to admin.");
            }

            return finalTimesheetSet;
        }

        public List<TimesheetDetail> GetPendingTimesheetByIdService(long employeeId, long clientId)
        {
            List<TimesheetDetail> timesheetDetail = new List<TimesheetDetail>();
            DateTime current = DateTime.UtcNow;

            DbParam[] dbParams = new DbParam[]
            {
                new DbParam(employeeId, typeof(long), "_EmployeeId"),
                new DbParam(clientId, typeof(int), "_ClientId"),
                new DbParam(_currentSession.CurrentUserDetail.RoleId, typeof(int), "_UserTypeId"),
                new DbParam(current.Year, typeof(int), "_ForYear"),
                new DbParam(current.Month, typeof(int), "_ForMonth")
            };

            var Result = _db.GetDataset("sp_employee_timesheet_get", dbParams);
            if (Result.Tables.Count == 1 && Result.Tables[0].Rows.Count > 0)
            {
                var currentTimesheetDetail = Converter.ToType<TimesheetDetail>(Result.Tables[0]);
                timesheetDetail = JsonConvert.DeserializeObject<List<TimesheetDetail>>(currentTimesheetDetail.TimesheetMonthJson);
            }

            return timesheetDetail;
        }

        public dynamic GetEmployeeTimeSheetService(TimesheetDetail timesheetDetail)
        {
            (TimesheetDetail currentTimesheetDetail, Employee employee) =
                _db.GetMulti<TimesheetDetail, Employee>("sp_employee_timesheet_getby_empid", new
                {
                    timesheetDetail.EmployeeId,
                    timesheetDetail.UserTypeId,
                    timesheetDetail.ForMonth,
                    timesheetDetail.ForYear,
                    timesheetDetail.ClientId
                });

            if (currentTimesheetDetail == null)
                currentTimesheetDetail = new TimesheetDetail
                {
                    ForMonth = timesheetDetail.ForMonth,
                    ForYear = timesheetDetail.ForYear
                };

            var result = BuildFinalTimesheet(currentTimesheetDetail);
            return new { TimesheetDetails = result.Item1, MissingDate = result.Item2 };
        }

        public (List<DailyTimesheetDetail>, List<DateTime>) BuildFinalTimesheet(TimesheetDetail currentTimesheetDetail)
        {
            List<DailyTimesheetDetail> dailyTimesheetDetails = new List<DailyTimesheetDetail>();
            List<DateTime> missingDayList = new List<DateTime>();
            if (currentTimesheetDetail != null && currentTimesheetDetail.TimesheetMonthJson != null)
            {
                dailyTimesheetDetails = JsonConvert
                    .DeserializeObject<List<DailyTimesheetDetail>>(currentTimesheetDetail.TimesheetMonthJson);

                Parallel.ForEach(dailyTimesheetDetails, x => x.TimesheetId = currentTimesheetDetail.TimesheetId);
            }
            else
            {
                currentTimesheetDetail = new TimesheetDetail
                {
                    ForMonth = currentTimesheetDetail.ForMonth,
                    ForYear = currentTimesheetDetail.ForYear
                };
            }

            int days = DateTime.DaysInMonth(currentTimesheetDetail.ForYear, currentTimesheetDetail.ForMonth);
            int i = 1;
            while (i <= days)
            {
                var value = dailyTimesheetDetails
                    .Where(x => _timezoneConverter.ToTimeZoneDateTime(x.PresentDate, _currentSession.TimeZone).Day == i)
                    .FirstOrDefault();
                if (value == null)
                {
                    missingDayList.Add(new DateTime(currentTimesheetDetail.ForYear, currentTimesheetDetail.ForMonth, i));
                }
                i++;
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
                    timesheetDetail.UserTypeId,
                    timesheetDetail.ForMonth,
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
                if (item.TimesheetStatus != ItemStatus.Submitted)
                {
                    missingDayList.Add(item.PresentDate);
                    dailyTimesheetDetails.RemoveAt(i);
                }
                else
                    i++;

            }

            currentTimesheetDetail.TimesheetMonthJson = JsonConvert.SerializeObject(dailyTimesheetDetails);
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
                    if (i.TimesheetStatus != ItemStatus.Submitted)
                        i.TimesheetStatus = ItemStatus.Absent;
                    else
                        i.TimesheetStatus = ItemStatus.Submitted;
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
                        ForMonth = 0,
                        ForYear = 0
                    };

                    if (billingDetail.FileDetail.Rows[0]["BillForMonth"] == DBNull.Value)
                        flag = true;
                    else
                        billingDetail.TimesheetDetail.ForMonth = Convert.ToInt32(billingDetail.FileDetail.Rows[0]["BillForMonth"]);

                    if (billingDetail.FileDetail.Rows[0]["BillYear"] == DBNull.Value)
                        flag = true;
                    else
                        billingDetail.TimesheetDetail.ForYear = Convert.ToInt32(billingDetail.FileDetail.Rows[0]["BillYear"]);

                    DateTime billingOn = DateTime.Now;
                    if (flag)
                    {
                        billingOn = Convert.ToDateTime(billingDetail.FileDetail.Rows[0]["BillYear"]);
                        billingDetail.TimesheetDetail.ForMonth = billingOn.Month;
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
