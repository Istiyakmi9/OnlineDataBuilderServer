using BottomhalfCore.DatabaseLayer.Common.Code;
using BottomhalfCore.Services.Code;
using ModalLayer.Modal;
using Newtonsoft.Json;
using ServiceLayer.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using TimeZoneConverter;

namespace ServiceLayer.Code
{
    public class AttendanceService : IAttendanceService
    {
        private readonly IDb _db;
        private readonly CurrentSession _currentSession;

        public AttendanceService(IDb db, CurrentSession currentSession)
        {
            _db = db;
            _currentSession = currentSession;
        }

        public AttendanceWithClientDetail GetAttendanceByUserId(AttendenceDetail attendenceDetail)
        {
            List<AttendenceDetail> attendenceDetails = null;
            AssignedClients client = null;
            TimeZoneInfo istTimeZome = TZConvert.GetTimeZoneInfo("India Standard Time");
            if (attendenceDetail.AttendenceFromDay != null)
                attendenceDetail.AttendenceFromDay = TimeZoneInfo.ConvertTimeFromUtc((DateTime)attendenceDetail.AttendenceFromDay, istTimeZome);

            if (attendenceDetail.AttendenceToDay != null)
                attendenceDetail.AttendenceToDay = TimeZoneInfo.ConvertTimeFromUtc((DateTime)attendenceDetail.AttendenceToDay, istTimeZome);

            if (attendenceDetail.AttendenceForMonth != null)
                attendenceDetail.AttendenceForMonth = TimeZoneInfo.ConvertTimeFromUtc((DateTime)attendenceDetail.AttendenceForMonth, istTimeZome);

            DbParam[] dbParams = new DbParam[]
            {
                new DbParam(attendenceDetail.EmployeeUid, typeof(int), "_EmployeeId"),
                new DbParam(attendenceDetail.ClientId, typeof(int), "_ClientId"),
                new DbParam(attendenceDetail.UserTypeId, typeof(int), "_UserTypeId"),
                new DbParam(attendenceDetail.AttendenceForMonth, typeof(DateTime), "_ForTime")
            };

            var Result = _db.GetDataset("sp_attendance_get", dbParams);
            if (Result.Tables.Count == 2 && Result.Tables[0].Rows.Count > 0)
            {
                var data = Result.Tables[0].Rows[0]["AttendanceDetail"].ToString();
                var attendanceId = Convert.ToInt64(Result.Tables[0].Rows[0]["AttendanceId"]);
                var attendanceData = JsonConvert.DeserializeObject<List<AttendenceDetail>>(data);
                client = Converter.ToType<AssignedClients>(Result.Tables[1]);

                attendenceDetails = new List<AttendenceDetail>();
                attendanceData.ForEach(x =>
                {
                    if (TimeZoneInfo.ConvertTimeFromUtc((DateTime)x.AttendanceDay, istTimeZome) >= attendenceDetail.AttendenceFromDay
                            && TimeZoneInfo.ConvertTimeFromUtc((DateTime)x.AttendanceDay, istTimeZome) <= attendenceDetail.AttendenceToDay)
                    {
                        x.AttendanceId = attendanceId;
                        attendenceDetails.Add(x);
                    }
                });
            }

            return new AttendanceWithClientDetail { Client = client, AttendacneDetails = attendenceDetails };
        }

        public List<AttendenceDetail> InsertUpdateAttendance(List<AttendenceDetail> attendenceDetail)
        {
            string result = string.Empty;
            var firstItem = attendenceDetail.FirstOrDefault();
            if (firstItem == null)
            {
                throw new HiringBellException("Invalid AttendanceDetail submitted.");
            }

            TimeZoneInfo istTimeZome = TZConvert.GetTimeZoneInfo("India Standard Time");
            int presentWeekOfMonth = 0;
            DateTime currentMonthDateTime = firstItem.AttendanceDay.AddMonths(2);
            DateTime firstDate = firstItem.AttendanceDay.AddMonths(2);
            DateTime lastDate = firstItem.AttendanceDay.AddMonths(-2);
            double TotalHours = 0;

            DbParam[] dbParams = new DbParam[]
            {
                new DbParam(firstItem.AttendanceId, typeof(long), "_AttendanceId")
            };

            var resultSet = _db.GetDataset("sp_attendance_get_byid", dbParams);

            if (resultSet == null || resultSet.Tables.Count == 0)
            {
                throw new HiringBellException("Unable to get attendance data. Incorrect data supplied.");
            }

            Attendance currentAttendance = Converter.ToType<Attendance>(resultSet.Tables[0]);

            List<AttendenceDetail> finalAttendanceSet = JsonConvert.DeserializeObject<List<AttendenceDetail>>(currentAttendance.AttendanceDetail);
            if (finalAttendanceSet == null)
            {
                throw new HiringBellException("Unable to get record. Please contact to admin.");
            }

            currentMonthDateTime = finalAttendanceSet.FirstOrDefault().AttendanceDay;

            int i = 0;
            while(i < attendenceDetail.Count)
            {
                var x = attendenceDetail.ElementAt(i);
                x.AttendanceDay = TimeZoneInfo.ConvertTimeFromUtc(x.AttendanceDay, istTimeZome);
                var item = finalAttendanceSet.Find(i => (TimeZoneInfo.ConvertTimeFromUtc(i.AttendanceDay, istTimeZome)).Subtract(TimeZoneInfo.ConvertTimeFromUtc(x.AttendanceDay, istTimeZome)).TotalDays == 0);
                if (item != null)
                {
                    item.Hours = x.Hours;
                    item.UserTypeId = x.UserTypeId;
                    item.EmployeeUid = x.EmployeeUid;
                    item.AttendanceId = firstItem.AttendanceId;
                    item.UserComments = x.UserComments;
                    item.AttendanceDay = x.AttendanceDay;
                    item.AttendenceStatus = x.AttendenceStatus;
                }
                else
                {
                    finalAttendanceSet.Add(new AttendenceDetail
                    {
                        Hours = x.Hours,
                        UserTypeId = x.UserTypeId,
                        EmployeeUid = x.EmployeeUid,
                        AttendanceId = firstItem.AttendanceId,
                        UserComments = x.UserComments,
                        AttendanceDay = x.AttendanceDay,
                        AttendenceStatus = x.AttendenceStatus
                    });
                }

                if ((x.AttendanceDay - firstDate).TotalDays < 0)
                    firstDate = x.AttendanceDay;

                if ((x.AttendanceDay - lastDate).TotalDays > 0)
                    lastDate = x.AttendanceDay;

                i++;
            }

            if (lastDate.Day < 20 && lastDate.Month == currentMonthDateTime.Month)
            {
                presentWeekOfMonth = this.GetWeekNumberOfMonth(lastDate, true);
            }
            else
            {
                presentWeekOfMonth = this.GetWeekNumberOfMonth(firstDate, false);
            }

            finalAttendanceSet.ForEach(x => TotalHours += x.Hours);
            var AttendaceDetail = JsonConvert.SerializeObject((from n in finalAttendanceSet
                                                               select new
                                                               {
                                                                   Hours = n.Hours,
                                                                   UserTypeId = n.UserTypeId,
                                                                   EmployeeUid = n.EmployeeUid,
                                                                   AttendanceId = n.AttendanceId,
                                                                   UserComments = n.UserComments,
                                                                   AttendanceDay = n.AttendanceDay,
                                                                   AttendenceStatus = n.AttendenceStatus
                                                               }));

            switch (presentWeekOfMonth)
            {
                case 1:
                    currentAttendance.FirstWeek = true;
                    break;
                case 2:
                    currentAttendance.SecondWeek = true;
                    break;
                case 3:
                    currentAttendance.ThirdWeek = true;
                    break;
                case 4:
                    currentAttendance.ForthWeek = true;
                    break;
                case 5:
                    currentAttendance.FifthWeek = true;
                    break;
                case 6:
                    currentAttendance.SixthWeek = true;
                    break;
            }

            dbParams = new DbParam[]
            {
                new DbParam(currentAttendance.AttendanceId, typeof(long), "_AttendanceId"),
                new DbParam(currentAttendance.EmployeeId, typeof(long), "_EmployeeId"),
                new DbParam(currentAttendance.UserTypeId, typeof(int), "_UserTypeId"),
                new DbParam(AttendaceDetail, typeof(string), "_AttendanceDetail"),
                new DbParam(currentAttendance.FirstWeek, typeof(bool), "_FirstWeek"),
                new DbParam(currentAttendance.SecondWeek, typeof(bool), "_SecondWeek"),
                new DbParam(currentAttendance.ThirdWeek, typeof(bool), "_ThirdWeek"),
                new DbParam(currentAttendance.ForthWeek, typeof(bool), "_ForthWeek"),
                new DbParam(currentAttendance.FifthWeek, typeof(bool), "_FifthWeek"),
                new DbParam(currentAttendance.SixthWeek, typeof(bool), "_SixthWeek"),
                new DbParam(TotalHours, typeof(double), "_TotalHoursBurned"),
                new DbParam(lastDate, typeof(DateTime), "_AttendanceForMonth"),
                new DbParam(_currentSession.CurrentUserDetail.UserId, typeof(long), "_UserId")
            };

            result = _db.ExecuteNonQuery("sp_attendance_insupd", dbParams, true);
            if (string.IsNullOrEmpty(result))
                return null;

            List<AttendenceDetail> attendenceDetails = new List<AttendenceDetail>();
            finalAttendanceSet.ForEach(x =>
            {
                if (TimeZoneInfo.ConvertTimeFromUtc((DateTime)x.AttendanceDay, istTimeZome) >= TimeZoneInfo.ConvertTimeFromUtc(firstDate, istTimeZome)
                        && TimeZoneInfo.ConvertTimeFromUtc((DateTime)x.AttendanceDay, istTimeZome) <= TimeZoneInfo.ConvertTimeFromUtc(lastDate, istTimeZome))
                {
                    attendenceDetails.Add(x);
                }
            });
            return attendenceDetails;
        }

        private int GetWeekNumberOfMonth(DateTime date, bool IsLastDayOfWeek)
        {
            date = date.Date;
            int weekOfMonth = 0;
            DateTime firstMonthDay = new DateTime(date.Year, date.Month, 1);
            DateTime firstMonthMonday = firstMonthDay.AddDays((DayOfWeek.Monday + 7 - firstMonthDay.DayOfWeek) % 7);
            DateTime previousMonthLastMonday = firstMonthDay.AddDays((DayOfWeek.Monday - 7 - firstMonthDay.DayOfWeek) % 7);
            if (firstMonthMonday > date)
            {
                weekOfMonth = 1;
            }
            else
            {
                weekOfMonth = 2;
                int suppliedDay = date.Day / 7;
                if (!IsLastDayOfWeek)
                    suppliedDay += 1;
                var nextDate = previousMonthLastMonday.AddDays(7 * suppliedDay);
                int noOfDays = nextDate.Day - firstMonthMonday.Day;
                weekOfMonth = weekOfMonth + noOfDays / 7;
            }
            return weekOfMonth;
        }
    }
}
