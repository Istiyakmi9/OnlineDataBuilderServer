using BottomhalfCore.DatabaseLayer.Common.Code;
using ModalLayer.Modal;
using ServiceLayer.Interface;
using System;
using System.Data;
using TimeZoneConverter;

namespace ServiceLayer.Code
{
    public class DashboardService: IDashboardService
    {
        private readonly IDb _db;

        public DashboardService(IDb db)
        {
            _db = db;
        }

        public DataSet GetEmployeeDeatils(AttendenceDetail userDetails)
        {
            TimeZoneInfo istTimeZome = TZConvert.GetTimeZoneInfo("India Standard Time");
            userDetails.AttendenceFromDay = TimeZoneInfo.ConvertTimeFromUtc((DateTime)userDetails.AttendenceFromDay, istTimeZome);
            userDetails.AttendenceToDay = TimeZoneInfo.ConvertTimeFromUtc((DateTime)userDetails.AttendenceToDay, istTimeZome);

            DbParam[] dbParams = new DbParam[]
            {
                new DbParam(userDetails.UserId, typeof(int), "_userId"),
                new DbParam(userDetails.EmployeeUid, typeof(int), "_userTypeId"),
                new DbParam(userDetails.AttendenceFromDay, typeof(DateTime), "_attendanceFromDay"),
                new DbParam(userDetails.AttendenceToDay, typeof(DateTime), "_attendanceToDay"),
            };

            var Result = _db.GetDataset("sp_attendance_get", dbParams);
            if (Result != null && Result.Tables.Count == 1)
            {
                Result.Tables[0].TableName = "Attendance";
                return Result;
            }

            return null;
        }

        
    }
}
