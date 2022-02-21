using BottomhalfCore.DatabaseLayer.Common.Code;
using ServiceLayer.Interface;
using System;
using System.Data;

namespace ServiceLayer.Code
{
    public class CommonService : ICommonService
    {
        private readonly IDb _db;
        private readonly IAuthenticationService _authenticationService;

        public CommonService(IDb db, IAuthenticationService authenticationService)
        {
            _db = db;
            _authenticationService = authenticationService;
        }

        public DataSet LoadApplicationData()
        {
            string AdminUid = _authenticationService.ReadJwtToken();
            if (!string.IsNullOrEmpty(AdminUid))
            {
                var AdminId = Convert.ToInt64(AdminUid);
                if (AdminId > 0)
                {
                    DbParam[] dbParams = new DbParam[]
                    {
                        new DbParam(AdminId, typeof(long), "_AdminId")
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
            }
            return null;
        }
    }
}
