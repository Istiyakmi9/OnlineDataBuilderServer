using BottomhalfCore.DatabaseLayer.Common.Code;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Threading.Tasks;

namespace BottomhalfCore.DatabaseLayer.Common.Code
{
    public interface IDb
    {
        DataSet FetchResult(string ProcedureName);
        /*===========================================  GetDataSet =============================================================*/
        DataSet GetDataset(string ProcedureName, DbParam[] param);
        DataSet GetDataset(string ProcedureName);
        int BatchInsert(string ProcedureName, DataTable table, Boolean IsOutparam);
        Task<int> BatchInsertAsync(string ProcedureName, DataSet TableSet, Boolean IsOutparam);
        DataSet GetDataset(string ProcedureName, DbParam[] param, bool OutParam, ref string ProcessingStatus);
        Object ExecuteSingle(string ProcedureName, DbParam[] param, bool OutParam);
        string ExecuteNonQuery(string ProcedureName, DbParam[] param, bool OutParam);
        void UserDefineTypeBulkInsert(DataSet dataset, string ProcedureName, bool OutParam);
        string XmlBatchInsertUpdate(string ProcedureName, string XmlData, bool OutParam);
        string InsertUpdateBatchRecord(string ProcedureName, DataTable table, Boolean OutParam = false);
        string SqlBulkInsert(DataTable table, string TableName);
        DataSet CommonadBuilderBulkInsertUpdate(string SelectQuery, string TableName);
        void StartTransaction(IsolationLevel isolationLevel);
        void Commit();
        void RollBack();

        /*=========================================  Generic type =====================================*/

        string Execute<T>(string ProcedureName, dynamic instance, bool OutParam);
        T Get<T>(string ProcedureName, T instance, bool OutParam = false) where T : new();
        T Get<T>(string ProcedureName, bool OutParam = false) where T : new();
        List<T> GetList<T>(string ProcedureName, bool OutParam = false) where T : new();
        T Get<T>(string ProcedureName, dynamic Parameters, bool OutParam = false) where T : new();
        List<T> GetList<T>(string ProcedureName, dynamic Parameters, bool OutParam = false) where T : new();
        DataSet Get(string ProcedureName, object Parameters, bool OutParam = false);
    }
}
