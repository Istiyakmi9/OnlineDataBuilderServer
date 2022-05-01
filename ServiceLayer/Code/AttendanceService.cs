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
                    List<AttendenceDetail> attendanceData = JsonConvert.DeserializeObject<List<AttendenceDetail>>(data);
                    int status = this.IsGivenDateAllowed((DateTime)attendenceDetail.AttendenceFromDay, (DateTime)attendenceDetail.AttendenceToDay, attendanceData);

                    attendenceDetails = new List<AttendenceDetail>();
                    attendanceData.ForEach(x =>
                    {
                        if (employee.CreatedOn.Subtract(x.AttendanceDay).TotalDays <= 0)
                        {
                            if (x.AttendanceDay >= attendenceDetail.AttendenceFromDay && x.AttendanceDay <= attendenceDetail.AttendenceToDay)
                            {
                                x.AttendanceId = attendanceId;
                                x.IsActiveDay = true;
                                x.IsOpen = status == 1 ? true : false;
                                attendenceDetails.Add(x);
                            }
                        }
                        else
                        {
                            x.AttendanceId = attendanceId;
                            x.IsActiveDay = false;
                            x.IsOpen = status == 1 ? true : false;
                            attendenceDetails.Add(x);
                        }
                    });

                    if (attendenceDetails.Count == 0)
                        attendenceDetails = this.GenerateWeekAttendaceData(attendenceDetail, status);
                }
                else
                {
                    int status = this.IsGivenDateAllowed((DateTime)attendenceDetail.AttendenceFromDay, (DateTime)attendenceDetail.AttendenceToDay, null);
                    attendenceDetails = this.GenerateWeekAttendaceData(attendenceDetail, status);
                }
            }

            return new AttendanceWithClientDetail { EmployeeDetail = employee, AttendacneDetails = attendenceDetails };
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

        public dynamic GetAttendamceById(AttendenceDetail attendenceDetail)
        {
            List<DateTime> missingDayList = new List<DateTime>();
            var attendenceList = new List<AttendenceDetail>();

            DbParam[] dbParams = new DbParam[]
            {
                new DbParam(attendenceDetail.EmployeeUid, typeof(long), "_EmployeeId"),
                new DbParam(attendenceDetail.UserTypeId, typeof(int), "_UserTypeId"),
                new DbParam(attendenceDetail.ForMonth, typeof(int), "_ForMonth"),
                new DbParam(attendenceDetail.ForYear, typeof(int), "_ForYear")
            };

            var result = _db.GetDataset("Sp_Attendance_GetById", dbParams);
            if (result.Tables.Count == 1 && result.Tables[0].Rows.Count > 0)
            {
                var currentAttendence = Converter.ToType<Attendance>(result.Tables[0]);
                attendenceList = JsonConvert.DeserializeObject<List<AttendenceDetail>>(currentAttendence.AttendanceDetail);
                attendenceList.OrderBy(DateTime => DateTime);
                int days = DateTime.DaysInMonth(currentAttendence.ForYear, currentAttendence.ForMonth);
                int i = 1;
                while (i <= days)
                {
                    var value = attendenceList.Where(x => x.AttendanceDay.Day == i).FirstOrDefault();
                    if (value == null)
                    {
                        missingDayList.Add(new DateTime(currentAttendence.ForYear, currentAttendence.ForMonth, i));
                    }
                    i++;
                }
            }

            return new { AttendanceDetail = attendenceList, MissingDate = missingDayList };
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

            int status = 1;
            Attendance currentAttendance = null;
            if (Result.Tables[0].Rows.Count > 0)
            {
                currentAttendance = new Attendance();
                currentAttendance = Converter.ToType<Attendance>(Result.Tables[0]);
                finalAttendanceSet = JsonConvert.DeserializeObject<List<AttendenceDetail>>(currentAttendance.AttendanceDetail);
                status = this.IsGivenDateAllowed(firstDate, lastDate, finalAttendanceSet);
            }
            else
            {
                status = 0;
                var currentMonthDateTime = _timezoneConverter.ToIstTime(DateTime.UtcNow);
                int totalDays = DateTime.DaysInMonth(currentMonthDateTime.Year, currentMonthDateTime.Month);
                currentAttendance = new Attendance
                {
                    EmployeeId = firstItem.EmployeeUid,
                    AttendanceId = 0,
                    ForYear = currentMonthDateTime.Year,
                    AttendanceDetail = "[]",
                    UserTypeId = (int)UserType.Employee,
                    TotalDays = totalDays,
                    TotalWeekDays = (int)_timezoneConverter.GetBusinessDays(
                                        _timezoneConverter.GetUtcFirstDay(currentMonthDateTime.Year, currentMonthDateTime.Month),
                                        _timezoneConverter.GetUtcDateTime(currentMonthDateTime.Year, currentMonthDateTime.Month, totalDays)),
                    DaysPending = totalDays,
                    ForMonth = currentMonthDateTime.Month
                };

                firstItem.AttendenceFromDay = firstDate;
                firstItem.AttendenceToDay = lastDate;
                finalAttendanceSet = this.GenerateWeekAttendaceData(firstItem, status);
            }

            if (_currentSession.CurrentUserDetail.RoleId != (int)UserType.Admin)
            {
                if (status == 0)
                {
                    throw new HiringBellException("Past and future week date's are not allowed to submit.");
                }
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
                var item = finalAttendanceSet.Find(i => i.AttendanceDay.Subtract(x.AttendanceDay).TotalDays == 0);
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
                    item.ClientId = x.ClientId;
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
                            AttendenceStatus = x.AttendenceStatus,
                            ClientId = x.ClientId
                        });
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

        public string AddComment(AttendenceDetail commentDetails)
        {
            DateTime todayDate = DateTime.UtcNow.Date;
            var value = todayDate.AddDays(-3);
            if (commentDetails.AttendanceDay >= value)
            {
                var attendenceList = new List<AttendenceDetail>();
                DbParam[] dbParams = new DbParam[]
                {
                    new DbParam(commentDetails.EmployeeUid, typeof(long), "_EmployeeId"),
                    new DbParam(commentDetails.UserTypeId, typeof(int), "_UserTypeId"),
                    new DbParam(commentDetails.AttendanceDay.Month, typeof(int), "_ForMonth"),
                    new DbParam(commentDetails.AttendanceDay.Year, typeof(int), "_ForYear")
                };

                var result = _db.GetDataset("Sp_Attendance_GetById", dbParams);
                if (result.Tables.Count == 1 && result.Tables[0].Rows.Count > 0)
                {
                    var currentAttendence = Converter.ToType<Attendance>(result.Tables[0]);
                    attendenceList = JsonConvert.DeserializeObject<List<AttendenceDetail>>(currentAttendence.AttendanceDetail);
                    var attendanceOn = attendenceList.Where(x => x.AttendanceDay == commentDetails.AttendanceDay).ToList();
                }

            }
            return null;
        }
        private List<AttendenceDetail> GenerateWeekAttendaceData(AttendenceDetail attendenceDetail, int isOpen)
        {
            List<AttendenceDetail> attendenceDetails = new List<AttendenceDetail>();
            var startDate = (DateTime)attendenceDetail.AttendenceFromDay;
            var endDate = (DateTime)attendenceDetail.AttendenceToDay;

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
                    UserTypeId = (int)UserType.Employee,
                    IsOpen = isOpen == 0 ? true : false
                });

                startDate = startDate.AddDays(1);
            }

            return attendenceDetails;
        }

        private int IsGivenDateAllowed(DateTime From, DateTime To, List<AttendenceDetail> attendanceData)
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

        private string UpdateOrInsertAttendanceDetail(List<AttendenceDetail> finalAttendanceSet, Attendance currentAttendance, string procedure)
        {
            var firstAttn = finalAttendanceSet.FirstOrDefault();
            TimeSheet timeSheet = new TimeSheet
            {
                ClientId = firstAttn.ClientId,
                Comments = "N/A",
                SubmittedOn = DateTime.UtcNow
            };

            var AttendaceDetail = JsonConvert.SerializeObject((from n in finalAttendanceSet
                                                               select new
                                                               {
                                                                   TotalMinutes = n.TotalMinutes,
                                                                   UserTypeId = n.UserTypeId,
                                                                   EmployeeUid = n.EmployeeUid,
                                                                   AttendanceId = n.AttendanceId,
                                                                   UserComments = n.UserComments,
                                                                   AttendanceDay = n.AttendanceDay,
                                                                   AttendenceStatus = n.AttendenceStatus,
                                                                   ClientTimeSheet = new List<TimeSheet> { timeSheet }
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
