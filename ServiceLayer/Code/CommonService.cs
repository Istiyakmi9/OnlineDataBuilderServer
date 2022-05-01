using BottomhalfCore.DatabaseLayer.Common.Code;
using ModalLayer.Modal;
using ServiceLayer.Interface;
using System.Data;
using BottomhalfCore.Services.Interface;

namespace ServiceLayer.Code
{
    public class CommonService : ICommonService
    {
        private readonly IDb _db;
        private readonly CurrentSession _currentSession;
        private readonly ICacheManager _cacheManager;

        public CommonService(IDb db, CurrentSession currentSession, ICacheManager cacheManager)
        {
            _db = db;
            _currentSession = currentSession;
            _cacheManager = cacheManager;
        }

        public DataSet LoadApplicationData()
        {
            DbParam[] dbParams = new DbParam[]
            {
                new DbParam(_currentSession.CurrentUserDetail.UserId, typeof(long), "_AdminId")
            };

            var Result = _db.GetDataset("SP_ApplicationLevelDropdown_Get", dbParams);
            if (Result.Tables.Count == 3)
            {
                Result.Tables[0].TableName = "clients";
                Result.Tables[1].TableName = "employees";
                Result.Tables[2].TableName = "allocatedClients";
            }
            return Result;
        }

        public DataTable LoadEmployeeData()
        {
            DataTable employeeTable = _cacheManager.Get(BottomhalfCore.Services.Code.Table.Employee);
            employeeTable.TableName = "Employee";
            return employeeTable;
        }
    }
}
