using BottomhalfCore.DatabaseLayer.Common.Code;
using ModalLayer;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Threading.Tasks;

namespace BottomhalfCore.DatabaseLayer.Common.Code
{
    public interface IDb
    {
        /*===========================================  GetDataSet =============================================================*/
        Task<int> ExecuteListAsync(string ProcedureName, List<dynamic> Parameters, bool IsOutParam = false);
        DataSet GetDataset(string ProcedureName, DbParam[] param);
        int BatchInsert(string ProcedureName, DataTable table, Boolean IsOutparam);
        Task<DbResult> BatchInsertUpdateAsync(string ProcedureName, DataTable table, Boolean IsOutparam);
        Object ExecuteSingle(string ProcedureName, DbParam[] param, bool OutParam);
        string ExecuteNonQuery(string ProcedureName, DbParam[] param, bool OutParam);
        void UserDefineTypeBulkInsert(DataSet dataset, string ProcedureName, bool OutParam);
        string InsertUpdateBatchRecord(string ProcedureName, DataTable table, Boolean OutParam = false);
        DataSet CommonadBuilderBulkInsertUpdate(string SelectQuery, string TableName);
        void StartTransaction(IsolationLevel isolationLevel);
        void Commit();
        void RollBack();

        /*=========================================  Generic type =====================================*/

        string Execute<T>(string ProcedureName, dynamic instance, bool OutParam);
        Task<DbResult> ExecuteAsync(string ProcedureName, dynamic instance, bool OutParam = false);
        T Get<T>(string ProcedureName, dynamic Parameters = null, bool OutParam = false) where T : new();
        (T, Q) GetMulti<T, Q>(string ProcedureName, dynamic Parameters = null, bool OutParam = false)
            where T : new()
            where Q : new();
        (T, Q, R) GetMulti<T, Q, R>(string ProcedureName, dynamic Parameters = null, bool OutParam = false)
            where T : new()
            where Q : new()
            where R : new();
        List<T> GetList<T>(string ProcedureName, bool OutParam = false) where T : new();
        List<T> GetList<T>(string ProcedureName, dynamic Parameters, bool OutParam = false) where T : new();
        (List<T>, List<R>) GetList<T, R>(string ProcedureName, dynamic Parameters, bool OutParam = false)
            where T : new()
            where R : new();
        (List<T>, List<R>, List<Q>) GetList<T, R, Q>(string ProcedureName, dynamic Parameters, bool OutParam = false)
            where T : new()
            where R : new()
            where Q : new();
        DataSet FetchDataSet(string ProcedureName, dynamic Parameters = null, bool OutParam = false);
        Task<DataSet> GetDataSet(string ProcedureName, dynamic Parameters = null, bool OutParam = false);
    }
}
