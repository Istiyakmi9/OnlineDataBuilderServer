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
using System.Text;

namespace ServiceLayer.Code
{
    public class TimesheetService : ITimesheetService
    {
        private readonly IDb _db;
        private readonly ITimezoneConverter _timezoneConverter;
        public TimesheetService(IDb db, ITimezoneConverter timezoneConverter)
        {
            _db = db;
            _timezoneConverter = timezoneConverter;
        }
        public dynamic GetTimesheetByUserIdService(TimesheetDetail timesheetDetail)
        {
            List<TimesheetDetail> timesheetDetails = null;
            Employee employee = null;

            if (timesheetDetail.ForMonth <= 0)
                throw new HiringBellException("Invalid month num. passed.", nameof(timesheetDetail.ForMonth), timesheetDetail.ForMonth.ToString());

            if (Convert.ToDateTime(timesheetDetail.AttendenceFromDay).Subtract(DateTime.UtcNow).TotalDays > 0)
            {
                throw new HiringBellException("Ohh!!!. Future dates are now allowed.");
            }


            DbParam[] dbParams = new DbParam[]
            {
                new DbParam(timesheetDetail.EmployeeUid, typeof(int), "_EmployeeId"),
                new DbParam(timesheetDetail.ClientId, typeof(int), "_ClientId"),
                new DbParam(timesheetDetail.UserTypeId, typeof(int), "_UserTypeId"),
                new DbParam(timesheetDetail.ForYear, typeof(int), "_ForYear"),
                new DbParam(timesheetDetail.ForMonth, typeof(int), "_ForMonth")
            };

            var Result = _db.GetDataset("sp_attendance_get", dbParams);
            if (Result.Tables.Count == 2)
            {

                if (Result.Tables[1].Rows.Count > 0)
                {
                    employee = Converter.ToType<Employee>(Result.Tables[1]);
                    if (Convert.ToDateTime(timesheetDetail.AttendenceToDay).Subtract(employee.CreatedOn).TotalDays < 0)
                    {
                        throw new HiringBellException("Past date before DOJ not allowed.");
                    }
                }

                if (Result.Tables[0].Rows.Count > 0 && employee != null)
                {
                    var data = Result.Tables[0].Rows[0]["AttendanceDetail"].ToString();
                    var attendanceId = Convert.ToInt64(Result.Tables[0].Rows[0]["AttendanceId"]);
                    timesheetDetails = JsonConvert.DeserializeObject<List<TimesheetDetail>>(data);
                    int status = this.IsGivenDateAllowed((DateTime)timesheetDetail.AttendenceFromDay, (DateTime)timesheetDetail.AttendenceToDay, timesheetDetails);

                    timesheetDetails.ForEach(x =>
                    {
                        x.TimesheetId = attendanceId;
                        x.IsActiveDay = false;
                        x.IsOpen = status == 1 ? true : false;
                    });

                    int i = 0;
                    var generatedTimesheet = this.GenerateWeekAttendaceData(timesheetDetail, status);
                    TimesheetDetail attrDetail = null;
                    foreach (var item in timesheetDetails)
                    {
                        i = 0;
                        while (i < generatedTimesheet.Count)
                        {
                            attrDetail = generatedTimesheet.ElementAt(i);
                            if (attrDetail.AttendanceDay.Date.Subtract(item.AttendanceDay.Date).TotalDays == 0)
                            {
                                generatedTimesheet[i] = item;
                                break;
                            }
                            i++;
                        }
                    }

                    timesheetDetails = generatedTimesheet;
                }
                else
                {
                    int status = this.IsGivenDateAllowed((DateTime)timesheetDetail.AttendenceFromDay, (DateTime)timesheetDetail.AttendenceToDay, null);
                    timesheetDetails = this.GenerateWeekAttendaceData(timesheetDetail, status);
                }

                if (this.IsRegisteredOnPresentWeek(employee.CreatedOn) == 1)
                {
                    timesheetDetails = timesheetDetails.Where(x => employee.CreatedOn.Date.Subtract(x.AttendanceDay.Date).TotalDays <= 0).ToList();
                }
            }

            return new TimesheetWithClientDetail { EmployeeDetail = employee, TimesheetDetails = timesheetDetails.OrderByDescending(x => x.AttendanceDay).ToList() };
        }

        private int IsGivenDateAllowed(DateTime From, DateTime To, List<TimesheetDetail> attendanceData)
        {
            var startDate = _timezoneConverter.ToIstTime(From);
            var endDate = _timezoneConverter.ToIstTime(To);
            var weekFirstDate = _timezoneConverter.FirstDayOfWeekIST();
            var weekLastDate = _timezoneConverter.LastDayOfWeekIST();

            if (startDate.Date.Subtract(weekFirstDate.Date).TotalDays == 0 && endDate.Date.Subtract(weekLastDate.Date).TotalDays == 0)
            {
                return 1;
            }
            else
            {
                if (attendanceData == null)
                    return 0;

                var workingWeek = attendanceData.Where(x => x.AttendanceDay.Date.Subtract(From.Date).TotalDays == 0).FirstOrDefault();
                if (workingWeek == null || !workingWeek.IsOpen)
                    return 0;

                // if (!workingWeek.IsOpen)
                // {
                //     throw new HiringBellException("Only present week attendance is allowed. For previous week please raise a permission to your manager or HR.");
                // }
            }

            return 0;
        }

        private List<TimesheetDetail> GenerateWeekAttendaceData(TimesheetDetail timesheetDetail, int isOpen, DateTime? monthFirstDate = null)
        {
            List<TimesheetDetail> timesheetDetails = new List<TimesheetDetail>();
            DateTime startDate = (DateTime)timesheetDetail.AttendenceFromDay;
            if (monthFirstDate != null)
                startDate = (DateTime)monthFirstDate;

            var endDate = (DateTime)timesheetDetail.AttendenceToDay;

            while (startDate.Subtract(endDate).TotalDays <= 0)
            {
                timesheetDetails.Add(new TimesheetDetail
                {
                    IsActiveDay = false,
                    TotalDays = DateTime.DaysInMonth(startDate.Year, startDate.Month),
                    AttendanceDay = startDate,
                    TimesheetId = 0,
                    AttendenceStatus = (int)ItemStatus.Pending,
                    BillingHours = 480,
                    ClientId = timesheetDetail.ClientId,
                    DaysPending = DateTime.DaysInMonth(startDate.Year, startDate.Month),
                    EmployeeUid = timesheetDetail.EmployeeUid,
                    ForMonth = startDate.Month,
                    ForYear = startDate.Year,
                    TotalMinutes = 480,
                    IsHoliday = (startDate.DayOfWeek == DayOfWeek.Saturday
                                    ||
                                startDate.DayOfWeek == DayOfWeek.Sunday) ? true : false,
                    IsOnLeave = false,
                    LeaveId = 0,
                    UserComments = string.Empty,
                    UserTypeId = (int)UserType.Employee,
                    IsOpen = isOpen == 1 ? true : false
                });

                startDate = startDate.AddDays(1);
            }

            return timesheetDetails;
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
    }
}
