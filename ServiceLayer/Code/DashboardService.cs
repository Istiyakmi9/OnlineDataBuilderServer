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

        public DataSet GetSystemDashboardService(AttendenceDetail userDetails)
        {
            DbParam[] dbParams = new DbParam[]
            {
                new DbParam(userDetails.UserId, typeof(int), "_userId"),
                new DbParam(userDetails.EmployeeUid, typeof(int), "_employeeUid"),
                new DbParam(userDetails.AttendenceFromDay, typeof(DateTime), "_fromDate"),
                new DbParam(userDetails.AttendenceToDay, typeof(DateTime), "_toDate"),
            };

            var Result = _db.GetDataset("sp_dashboard_get", dbParams);
            if (Result != null && Result.Tables.Count == 3)
            {
                Result.Tables[0].TableName = "BillDetail";
                Result.Tables[1].TableName = "GSTDetail";
                Result.Tables[2].TableName = "AttendaceDetail";
                return Result;
            }

            return Result;
        }

        
    }
}
