using BottomhalfCore.DatabaseLayer.Common.Code;
using BottomhalfCore.Services.Code;
using ModalLayer.Modal;
using Newtonsoft.Json;
using ServiceLayer.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TimeZoneConverter;

namespace ServiceLayer.Code
{
    public class AttendanceService : IAttendanceService
    {
        private readonly IDb _db;

        public AttendanceService(IDb db)
        {
            _db = db;
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
                Parallel.ForEach(attendanceData, x =>
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

        public string InsertUpdateAttendance(List<AttendenceDetail> attendenceDetail)
        {
            string result = string.Empty;
            var firstItem = attendenceDetail.FirstOrDefault();
            if (firstItem == null)
            {
                throw new HiringBellException("Invalid AttendanceDetail submitted.");
            }

            DateTime firstDate = firstItem.AttendanceDay.AddMonths(1).AddDays(-1);
            DateTime lastDate = attendenceDetail.LastOrDefault().AttendanceDay;
            TimeZoneInfo istTimeZome = TZConvert.GetTimeZoneInfo("India Standard Time");
            double TotalHours = 0;
            double ExpectedHours = 0;
            attendenceDetail.ForEach(x =>
            {
                x.AttendanceDay = TimeZoneInfo.ConvertTimeFromUtc(x.AttendanceDay, istTimeZome);
                TotalHours += x.Hours;
                ExpectedHours += 8;
            });


            List<AttendenceDetail> finalAttendanceSet = new List<AttendenceDetail>();
            var resultSet = this.GetAttendanceByUserId(new AttendenceDetail { UserId = firstItem.UserId, UserTypeId = firstItem.UserTypeId, AttendenceForMonth = firstDate });

            List<AttendenceDetail> attendaceList = resultSet.AttendacneDetails;
            if (attendaceList != null)
            {
                attendaceList.ForEach(x =>
                {
                    var item = attendenceDetail.Find(i => i.AttendanceDay.Subtract(x.AttendanceDay).TotalDays == 0);
                    if (item != null)
                    {
                        item.AttendanceId = x.AttendanceId;
                        if (finalAttendanceSet.Count == 0 || finalAttendanceSet.FirstOrDefault(p => p.AttendanceId == item.AttendanceId) == null)
                        {
                            finalAttendanceSet.Add(item);
                        }
                    }
                });
            }


            DbParam[] dbParams = new DbParam[]
            {
                    new DbParam(firstItem.AttendanceId, typeof(long), "_AttendanceId"),
                    new DbParam(firstItem.EmployeeUid, typeof(long), "_EmployeeId"),
                    new DbParam(firstItem.UserTypeId, typeof(int), "_UserTypeId"),
                    new DbParam(JsonConvert.SerializeObject(finalAttendanceSet), typeof(string), "_AttendanceDetail"),
                    new DbParam(TotalHours, typeof(float), "_TotalHoursBurend"),
                    new DbParam(ExpectedHours, typeof(float), "_ExpectedHours"),
                    new DbParam(firstItem.SubmittedOn, typeof(DateTime), "_AttendanceForMonth")
            };

            result = _db.ExecuteNonQuery("sp_attendance_insupd", dbParams, true);

            return result;
        }
    }
}
