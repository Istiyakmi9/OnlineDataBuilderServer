using BottomhalfCore.DatabaseLayer.MySql.Code;
using Microsoft.Extensions.Options;
using ModalLayer.Modal;
using System.Collections.Generic;

namespace ServiceLayer.Code
{
    public class AppUtilityService
    {
        private Dictionary<string, DbConfigModal> _dbConfig;

        public AppUtilityService(IOptions<Dictionary<string, DbConfigModal>> options)
        {
            _dbConfig = options.Value;
        }

        public void ConfigureDatabase(string CompanyCode)
        {
            DbConfigModal dbConfig = null;
            _dbConfig.TryGetValue(CompanyCode.ToUpper(), out dbConfig);
            if (dbConfig == null)
            {
                throw HiringBellException.ThrowBadRequest($"No connection string found for compnay code: {CompanyCode}");
            }

            var cs = @$"server={dbConfig.Server};port={dbConfig.Port};database={dbConfig.Database};User Id={dbConfig.User_Id};password={dbConfig.Password};Connection Timeout={dbConfig.Connection_Timeout};Connection Lifetime={dbConfig.Connection_Lifetime};Min Pool Size={dbConfig.Min_Pool_Size};Max Pool Size={dbConfig.Max_Pool_Size};Pooling={dbConfig.Pooling};";
            Db.SetupConnectionString(cs);
        }
    }
}
