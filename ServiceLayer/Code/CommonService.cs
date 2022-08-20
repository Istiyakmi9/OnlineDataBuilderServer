using BottomhalfCore.DatabaseLayer.Common.Code;
using ServiceLayer.Caching;
using ServiceLayer.Interface;
using System.Data;

namespace ServiceLayer.Code
{
    public class CommonService : ICommonService
    {
        private readonly IDb _db;
        private readonly ICacheManager _cacheManager;

        public CommonService(IDb db, ICacheManager cacheManager)
        {
            _db = db;
            _cacheManager = cacheManager;
        }

        public DataTable LoadEmployeeData()
        {
            DataTable employeeTable = _cacheManager.Get(ServiceLayer.Caching.Table.Employee);
            employeeTable.TableName = "Employee";
            return employeeTable;
        }

        public bool IsEmptyJson(string json)
        {
            if (!string.IsNullOrEmpty(json))
            {
                if (json == "" || json == "{}" || json == "[]")
                    return true;
            }
            else
                return true;

            return false;
        }
    }
}
