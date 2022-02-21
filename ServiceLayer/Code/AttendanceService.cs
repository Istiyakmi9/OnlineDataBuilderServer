using BottomhalfCore.DatabaseLayer.Common.Code;
using BottomhalfCore.Services.Code;
using ModalLayer.Modal;
using ServiceLayer.Interface;
using System;
using System.Collections.Generic;
using System.Data;
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

        public DataSet GetAttendanceByUserId(AttendenceDetail attendenceDetail)
        {
            TimeZoneInfo istTimeZome = TZConvert.GetTimeZoneInfo("India Standard Time");
            attendenceDetail.AttendenceFromDay = TimeZoneInfo.ConvertTimeFromUtc((DateTime)attendenceDetail.AttendenceFromDay, istTimeZome);
            attendenceDetail.AttendenceToDay = TimeZoneInfo.ConvertTimeFromUtc((DateTime)attendenceDetail.AttendenceToDay, istTimeZome);

            DbParam[] dbParams = new DbParam[]
            {
                new DbParam(attendenceDetail.UserId, typeof(int), "_userId"),
                new DbParam(attendenceDetail.UserTypeId, typeof(int), "_userTypeId"),
                new DbParam(attendenceDetail.AttendenceFromDay, typeof(DateTime), "_attendanceFromDay"),
                new DbParam(attendenceDetail.AttendenceToDay, typeof(DateTime), "_attendanceToDay"),
            };

            var Result = _db.GetDataset("sp_attendance_get", dbParams);
            if (Result != null && Result.Tables.Count == 1)
            {
                Result.Tables[0].TableName = "Attendance";
                return Result;
            }

            return null;
        }

        public string InsertUpdateAttendance(List<AttendenceDetail> attendenceDetail)
        {
            var firstItem = attendenceDetail.FirstOrDefault();
            DateTime firstDate = firstItem.AttendanceDay;
            DateTime lastDate = attendenceDetail.LastOrDefault().AttendanceDay;
            TimeZoneInfo istTimeZome = TZConvert.GetTimeZoneInfo("India Standard Time");
            attendenceDetail.ForEach(x =>
            {
                if ((x.AttendanceDay - firstDate).TotalDays <= 0)
                {
                    firstDate = x.AttendanceDay;
                }

                if ((x.AttendanceDay - lastDate).TotalDays >= 0)
                {
                    lastDate = x.AttendanceDay;
                }

                x.AttendanceDay = TimeZoneInfo.ConvertTimeFromUtc(x.AttendanceDay, istTimeZome);
            });


            List<AttendenceDetail> finalAttendanceSet = new List<AttendenceDetail>();
            var attendanceSet = this.GetAttendanceByUserId(new AttendenceDetail { UserId = firstItem.UserId, UserTypeId = firstItem.UserTypeId, AttendenceFromDay = firstDate, AttendenceToDay = lastDate });
            if (attendanceSet != null && attendanceSet.Tables.Count > 0)
            {
                if (attendanceSet.Tables[0].Rows.Count != 0)
                {
                    var attendaceList = Converter.ToList<AttendenceDetail>(attendanceSet.Tables[0]);
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
                else
                {
                    finalAttendanceSet = attendenceDetail;
                }

                var fileInfo = (from n in finalAttendanceSet.AsEnumerable()
                                select new
                                {
                                    AttendanceId = n.AttendanceId,
                                    UserId = n.UserId,
                                    UserTypeId = n.UserTypeId,
                                    AttendanceDay = n.AttendanceDay,
                                    Hours = n.Hours,
                                    IsHoliday = n.IsHoliday,
                                    IsWeekEnd = n.IsWeekEnd,
                                    AttendanceStatus = n.AttendenceStatus,
                                    UserComments = n.UserComments,
                                    SubmittedOn = DateTime.Now,
                                    SubmitterUserId = n.UserId
                                });

                DataTable table = Converter.ToDataTable(fileInfo);
                var dataSet = new DataSet();
                dataSet.Tables.Add(table);
                var result = _db.BatchInsert(ApplicationConstants.InserUpdateAttendance, dataSet, true);
                if (result > 0)
                    return "Insertd/Update successfully";
            }

            return "Fail to insert/update";
        }
    }
}
