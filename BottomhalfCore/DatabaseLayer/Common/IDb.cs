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

        DataSet Get(string ProcedureName, object Parameters, bool OutParam = false);
        string Execute<T>(string ProcedureName, dynamic instance, bool OutParam);
        T Get<T>(string ProcedureName, T instance, bool OutParam = false) where T : new();
        T Get<T>(string ProcedureName, bool OutParam = false) where T : new();
        T Get<T>(string ProcedureName, dynamic Parameters, bool OutParam = false) where T : new();
        (T, Q) GetMulti<T, Q>(string ProcedureName, dynamic Parameters = null, bool OutParam = false)
            where T : new()
            where Q : new();
        (T, Q, R) GetMulti<T, Q, R>(string ProcedureName, dynamic Parameters = null, bool OutParam = false)
            where T : new()
            where Q : new()
            where R : new();
        T GetValue<T>(string ProcedureName, dynamic Parameters = null, bool OutParam = false) where T : new();

        List<T> GetList<T>(string ProcedureName, bool OutParam = false) where T : new();
        List<T> GetList<T>(string ProcedureName, dynamic Parameters, bool OutParam = false) where T : new();
        (List<T>, List<R>) GetList<T, R>(string ProcedureName, dynamic Parameters, bool OutParam = false)
            where T : new()
            where R : new();
        (List<T>, List<R>, List<Q>) GetList<T, R, Q>(string ProcedureName, dynamic Parameters, bool OutParam = false)
            where T : new()
            where R : new()
            where Q : new();
        List<T> GetListValue<T>(string ProcedureName, dynamic Parameters = null, bool OutParam = false) where T : new();
        DataSet FetchDataSet(string ProcedureName, dynamic Parameters, bool OutParam = false);
        Task<string> BatchUpdateAsync(string ProcedureName, DataTable table, Boolean OutParam = false);
    }
}
