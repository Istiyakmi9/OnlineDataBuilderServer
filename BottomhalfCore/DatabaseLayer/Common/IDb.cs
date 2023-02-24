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
        string ExecuteNonQuery(string ProcedureName, DbParam[] param, bool OutParam);
        void StartTransaction(IsolationLevel isolationLevel);
        void Commit();
        void RollBack();

        /*=========================================  Generic type =====================================*/

        Task<int> BulkExecuteAsync<T>(string ProcedureName, List<T> Parameters, bool IsOutParam = false);
        string Execute<T>(string ProcedureName, dynamic instance, bool OutParam);
        DbResult Execute(string ProcedureName, dynamic Parameters, bool OutParam = false);
        Task<DbResult> ExecuteAsync(string ProcedureName, dynamic instance, bool OutParam = false);
        T Get<T>(string ProcedureName, dynamic Parameters = null, bool OutParam = false) where T : new();
        (T, Q) GetMulti<T, Q>(string ProcedureName, dynamic Parameters = null, bool OutParam = false)
            where T : new()
            where Q : new();
        (T, Q, R) GetMulti<T, Q, R>(string ProcedureName, dynamic Parameters = null, bool OutParam = false)
            where T : new()
            where Q : new()
            where R : new();
        List<T> GetList<T>(string ProcedureName, dynamic Parameters = null, bool OutParam = false) where T : new();
        (List<T>, List<R>) GetList<T, R>(string ProcedureName, dynamic Parameters, bool OutParam = false)
            where T : new()
            where R : new();
        (List<T>, List<R>, List<Q>) GetList<T, R, Q>(string ProcedureName, dynamic Parameters, bool OutParam = false)
            where T : new()
            where R : new()
            where Q : new();
        DataSet FetchDataSet(string ProcedureName, dynamic Parameters = null, bool OutParam = false);
        Task<DataSet> GetDataSetAsync(string ProcedureName, dynamic Parameters = null, bool OutParam = false);
        DataSet GetDataSet(string ProcedureName, dynamic Parameters = null, bool OutParam = false);

        int BatchInset(string ProcedureName, DataTable table);

        // --------------------new -----------------------------

        (T, Q) Get<T, Q>(string ProcedureName, dynamic Parameters = null, bool OutParam = false)
            where T : new()
            where Q : new();
    }
}
