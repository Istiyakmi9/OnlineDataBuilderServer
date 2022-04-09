using BottomhalfCore.DatabaseLayer.Common.Code;
using BottomhalfCore.Services.Code;
using BottomhalfCore.Services.Interface;
using ModalLayer.Modal;
using Newtonsoft.Json;
using ServiceLayer.Interface;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ServiceLayer.Code
{
    public class AttendanceService : IAttendanceService
    {
        private readonly IDb _db;
        private readonly CurrentSession _currentSession;
        private readonly ITimezoneConverter _timezoneConverter;

        public AttendanceService(IDb db, ITimezoneConverter timezoneConverter, CurrentSession currentSession)
        {
            _db = db;
            _currentSession = currentSession;
            _timezoneConverter = timezoneConverter;
        }

        public AttendanceWithClientDetail GetAttendanceByUserId(AttendenceDetail attendenceDetail)
        {
            List<AttendenceDetail> attendenceDetails = null;
            Employee employee = null;
            if (attendenceDetail.AttendenceFromDay != null)
                attendenceDetail.AttendenceFromDay = TimeZoneInfo.ConvertTimeToUtc((DateTime)attendenceDetail.AttendenceFromDay);

            if (attendenceDetail.AttendenceToDay != null)
                attendenceDetail.AttendenceToDay = TimeZoneInfo.ConvertTimeToUtc((DateTime)attendenceDetail.AttendenceToDay);

            if (attendenceDetail.ForMonth <= 0)
                throw new HiringBellException("Invalid month num. passed.", nameof(attendenceDetail.ForMonth), attendenceDetail.ForMonth.ToString());

            if (Convert.ToDateTime(attendenceDetail.AttendenceFromDay).Subtract(DateTime.UtcNow).TotalDays > 0)
            {
                throw new HiringBellException("Selected date are blocked. Please contact to admin.");
            }

            if (Convert.ToDateTime(attendenceDetail.AttendenceFromDay).Month != DateTime.UtcNow.Month &&
                Convert.ToDateTime(attendenceDetail.AttendenceToDay).Month != DateTime.UtcNow.Month)
            {
                throw new HiringBellException("Selected date are blocked. Please contact to admin.");
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
            if (Result.Tables.Count == 2)
            {
                if (Result.Tables[1].Rows.Count > 0)
                {
                    employee = Converter.ToType<Employee>(Result.Tables[1]);
                    if (Convert.ToDateTime(attendenceDetail.AttendenceToDay).Subtract(employee.CreatedOn).TotalDays < 0)
                    {
                        throw new HiringBellException("Past date before DOJ not allowed.");
                    }
                }

                if (Result.Tables[0].Rows.Count > 0 && employee != null)
                {
                    var data = Result.Tables[0].Rows[0]["AttendanceDetail"].ToString();
                    var attendanceId = Convert.ToInt64(Result.Tables[0].Rows[0]["AttendanceId"]);
                    var attendanceData = JsonConvert.DeserializeObject<List<AttendenceDetail>>(data);

                    attendenceDetails = new List<AttendenceDetail>();
                    attendanceData.ForEach(x =>
                    {
                        if (employee.CreatedOn.Subtract(TimeZoneInfo.ConvertTimeToUtc(x.AttendanceDay)).TotalDays <= 0)
                        {
                            if (x.AttendanceDay >= TimeZoneInfo.ConvertTimeToUtc((DateTime)attendenceDetail.AttendenceFromDay)
                                    && x.AttendanceDay <= TimeZoneInfo.ConvertTimeToUtc((DateTime)attendenceDetail.AttendenceToDay))
                            {
                                x.AttendanceId = attendanceId;
                                x.IsActiveDay = true;
                                attendenceDetails.Add(x);
                            }
                        }
                        else
                        {
                            x.AttendanceId = attendanceId;
                            x.IsActiveDay = false;
                            attendenceDetails.Add(x);
                        }
                    });
                }
                else
                {
                    attendenceDetails = this.GenerateWeekAttendaceData(attendenceDetail);
                }
            }

            return new AttendanceWithClientDetail { EmployeeDetail = employee, AttendacneDetails = attendenceDetails };
        }

        public List<DateTime> GetAllPendingAttendanceByUserIdService(long employeeId)
        {
            List<DateTime> pendingDays = new List<DateTime>();
            List<AttendenceDetail> attendanceSet = null;
            DateTime current = DateTime.UtcNow;

            DbParam[] dbParams = new DbParam[]
            {
                new DbParam(employeeId, typeof(int), "_EmployeeId"),
                new DbParam(_currentSession.CurrentUserDetail.UserTypeId, typeof(int), "_UserTypeId"),
                new DbParam(current.Year, typeof(int), "_ForYear"),
                new DbParam(current.Month, typeof(int), "_ForMonth")
            };

            var Result = _db.GetDataset("sp_attendance_getAll", dbParams);
            if (Result.Tables.Count == 0 && Result.Tables[0].Rows.Count > 0)
            {
                var currentAttendance = Converter.ToType<Attendance>(Result.Tables[0]);
                attendanceSet = JsonConvert.DeserializeObject<List<AttendenceDetail>>(currentAttendance.AttendanceDetail);

                DateTime firstDay = new DateTime(current.Year, current.Month, 1);
                while (firstDay.Subtract(current).TotalDays <= 0)
                {
                    var result = attendanceSet.Find(x => x.AttendanceDay.Subtract(firstDay).TotalDays == 0);
                    if (result == null)
                        pendingDays.Add(firstDay);
                    firstDay = firstDay.AddDays(1);
                }
            }

            return pendingDays;
        }

        public List<AttendenceDetail> InsertUpdateAttendance(List<AttendenceDetail> attendenceDetail)
        {
            string result = string.Empty;
            var firstItem = attendenceDetail.FirstOrDefault();
            if (firstItem == null)
            {
                throw new HiringBellException("Invalid AttendanceDetail submitted.");
            }

            var invalidAttendaceData = attendenceDetail.FindAll(x => x.EmployeeUid <= 0 || x.ClientId <= 0);
            if (invalidAttendaceData.Count > 0)
                throw new HiringBellException("Invalid Employee/Client Id passed.");

            DateTime firstDate = firstItem.AttendanceDay.AddYears(2);
            DateTime lastDate = firstItem.AttendanceDay.AddYears(-2);
            int j = 0;
            while (j < attendenceDetail.Count)
            {
                var x = attendenceDetail.ElementAt(j);
                x.AttendanceDay = TimeZoneInfo.ConvertTimeToUtc(x.AttendanceDay);
                if ((x.AttendanceDay - firstDate).TotalDays < 0)
                    firstDate = x.AttendanceDay;

                if ((x.AttendanceDay - lastDate).TotalDays > 0)
                    lastDate = x.AttendanceDay;

                j++;
            }

            DbParam[] dbParams = new DbParam[]
            {
                new DbParam(firstItem.EmployeeUid, typeof(int), "_EmployeeId"),
                new DbParam(firstItem.ClientId, typeof(int), "_ClientId"),
                new DbParam(firstItem.UserTypeId, typeof(int), "_UserTypeId"),
                new DbParam(firstItem.ForYear, typeof(int), "_ForYear"),
                new DbParam(firstItem.ForMonth, typeof(int), "_ForMonth")
            };

            var Result = _db.GetDataset("sp_attendance_get", dbParams);
            if (Result.Tables.Count != 2 && Result.Tables[0].Rows.Count == 0)
            {
                throw new HiringBellException("Attendance detail is invalid.");
            }

            List<AttendenceDetail> otherMonthAttendanceDetail = new List<AttendenceDetail>();
            List<AttendenceDetail> finalAttendanceSet = new List<AttendenceDetail>();

            Attendance currentAttendance = null;
            if (Result.Tables[0].Rows.Count > 0)
            {
                currentAttendance = new Attendance();
                currentAttendance = Converter.ToType<Attendance>(Result.Tables[0]);
                finalAttendanceSet = JsonConvert.DeserializeObject<List<AttendenceDetail>>(currentAttendance.AttendanceDetail);
            }
            else
            {
                var currentMonthDateTime = firstDate.AddDays(10);
                int totalDays = DateTime.DaysInMonth(currentMonthDateTime.Year, currentMonthDateTime.Month);
                currentAttendance = new Attendance
                {
                    EmployeeId = firstItem.EmployeeUid,
                    AttendanceId = 0,
                    ForYear = currentMonthDateTime.Year,
                    AttendanceDetail = "[]",
                    UserTypeId = (int)UserType.Employee,
                    TotalDays = totalDays,
                    TotalWeekDays = (int)_timezoneConverter.GetBusinessDays(new DateTime(currentMonthDateTime.Year, currentMonthDateTime.Month, 1),
                                        new DateTime(currentMonthDateTime.Year, currentMonthDateTime.Month, totalDays)),
                    DaysPending = totalDays,
                    ForMonth = currentMonthDateTime.Month
                };

                firstItem.AttendenceFromDay = firstDate;
                firstItem.AttendenceToDay = lastDate;
                finalAttendanceSet = this.GenerateWeekAttendaceData(firstItem);
            }

            Employee employee = new Employee();
            if (Result.Tables[1].Rows.Count > 0)
            {
                employee = Converter.ToType<Employee>(Result.Tables[1]);
            }

            if (finalAttendanceSet == null)
            {
                throw new HiringBellException("Unable to get record. Please contact to admin.");
            }

            int i = 0;
            while (i < attendenceDetail.Count)
            {
                var x = attendenceDetail.ElementAt(i);
                x.AttendanceDay = TimeZoneInfo.ConvertTimeToUtc(x.AttendanceDay);
                if (employee.CreatedOn.Subtract(TimeZoneInfo.ConvertTimeToUtc(x.AttendanceDay)).TotalDays <= 0)
                {
                    var item = finalAttendanceSet.Find(i => (TimeZoneInfo.ConvertTimeToUtc(i.AttendanceDay))
                                                            .Subtract(TimeZoneInfo.ConvertTimeToUtc(x.AttendanceDay)).TotalDays == 0);
                    if (item != null)
                    {
                        item.TotalMinutes = x.TotalMinutes;
                        item.UserTypeId = x.UserTypeId;
                        item.EmployeeUid = x.EmployeeUid;
                        item.IsHoliday = (x.AttendanceDay.DayOfWeek == DayOfWeek.Saturday
                                           ||
                                          x.AttendanceDay.DayOfWeek == DayOfWeek.Sunday) ? true : false;
                        item.IsOnLeave = false;
                        item.AttendanceId = firstItem.AttendanceId;
                        item.UserComments = x.UserComments;
                        item.AttendanceDay = x.AttendanceDay;
                        item.AttendenceStatus = x.AttendenceStatus;
                    }
                    else
                    {
                        if (x.AttendanceDay.Month != currentAttendance.ForMonth)
                        {
                            x.AttendanceId = -1;
                            x.ForMonth = x.AttendanceDay.Month;
                            x.ForYear = x.AttendanceDay.Year;
                            x.IsHoliday = (x.AttendanceDay.DayOfWeek == DayOfWeek.Saturday
                                            ||
                                           x.AttendanceDay.DayOfWeek == DayOfWeek.Sunday) ? true : false;
                            x.IsOnLeave = false;
                            otherMonthAttendanceDetail.Add(x);
                        }
                        else
                        {
                            finalAttendanceSet.Add(new AttendenceDetail
                            {
                                TotalMinutes = x.TotalMinutes,
                                UserTypeId = x.UserTypeId,
                                EmployeeUid = x.EmployeeUid,
                                IsHoliday = (x.AttendanceDay.DayOfWeek == DayOfWeek.Saturday
                                                   ||
                                                  x.AttendanceDay.DayOfWeek == DayOfWeek.Sunday) ? true : false,
                                IsOnLeave = false,
                                AttendanceId = firstItem.AttendanceId,
                                UserComments = x.UserComments,
                                AttendanceDay = x.AttendanceDay,
                                AttendenceStatus = x.AttendenceStatus
                            });
                        }
                    }
                }

                i++;
            }

            result = this.UpdateOrInsertAttendanceDetail(finalAttendanceSet, currentAttendance, ApplicationConstants.InserUpdateAttendance);
            if (string.IsNullOrEmpty(result))
            {
                throw new HiringBellException("Unable to insert/update record. Please contact to admin.");
            }

            List<AttendenceDetail> attendenceDetails = new List<AttendenceDetail>();
            finalAttendanceSet.ForEach(x =>
            {
                if (x.AttendanceDay >= TimeZoneInfo.ConvertTimeToUtc(firstDate) && x.AttendanceDay <= TimeZoneInfo.ConvertTimeToUtc(lastDate))
                {
                    attendenceDetails.Add(x);
                }
            });

            if (otherMonthAttendanceDetail.Count > 0)
            {
                currentAttendance.ForMonth = otherMonthAttendanceDetail.First().ForMonth;
                currentAttendance.ForYear = otherMonthAttendanceDetail.First().ForYear;
                result = this.UpdateOrInsertAttendanceDetail(otherMonthAttendanceDetail, currentAttendance, "sp_attendance_insupd_by_monthandyear");
                if (string.IsNullOrEmpty(result))
                {
                    throw new HiringBellException("Unable to insert/update record. Please contact to admin.");
                }

                otherMonthAttendanceDetail.ForEach(x =>
                {
                    if (x.AttendanceDay >= TimeZoneInfo.ConvertTimeToUtc(firstDate) && x.AttendanceDay <= TimeZoneInfo.ConvertTimeToUtc(lastDate))
                    {
                        attendenceDetails.Add(x);
                    }
                });
            }

            return attendenceDetails;
        }

        private List<AttendenceDetail> GenerateWeekAttendaceData(AttendenceDetail attendenceDetail)
        {
            List<AttendenceDetail> attendenceDetails = new List<AttendenceDetail>();
            var startDate = TimeZoneInfo.ConvertTimeToUtc(Convert.ToDateTime(attendenceDetail.AttendenceFromDay));
            var endDate = TimeZoneInfo.ConvertTimeToUtc(Convert.ToDateTime(attendenceDetail.AttendenceToDay));
            var days = endDate.Subtract(startDate);

            if (days.TotalDays > 0)
            {
                while (startDate.Subtract(endDate).TotalDays <= 0)
                {
                    attendenceDetails.Add(new AttendenceDetail
                    {
                        IsActiveDay = false,
                        TotalDays = DateTime.DaysInMonth(startDate.Year, startDate.Month),
                        AttendanceDay = startDate,
                        AttendanceId = 0,
                        AttendenceStatus = 4,
                        BillingHours = 480,
                        ClientId = attendenceDetail.ClientId,
                        DaysPending = DateTime.DaysInMonth(startDate.Year, startDate.Month),
                        EmployeeUid = attendenceDetail.EmployeeUid,
                        ForMonth = startDate.Month,
                        ForYear = startDate.Year,
                        TotalMinutes = 480,
                        IsHoliday = (startDate.DayOfWeek == DayOfWeek.Saturday
                                        ||
                                    startDate.DayOfWeek == DayOfWeek.Sunday) ? true : false,
                        IsOnLeave = false,
                        LeaveId = 0,
                        UserComments = string.Empty,
                        UserTypeId = (int)UserType.Employee
                    });

                    startDate = startDate.AddDays(1);
                }
            }

            return attendenceDetails;
        }

        private string UpdateOrInsertAttendanceDetail(List<AttendenceDetail> finalAttendanceSet, Attendance currentAttendance, string procedure)
        {
            var AttendaceDetail = JsonConvert.SerializeObject((from n in finalAttendanceSet
                                                               select new
                                                               {
                                                                   TotalMinutes = n.TotalMinutes,
                                                                   UserTypeId = n.UserTypeId,
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
    }
}
