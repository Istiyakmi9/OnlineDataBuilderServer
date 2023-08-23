using ModalLayer;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace BottomhalfCore.DatabaseLayer.Common.Code
{
    public interface IDb
    {
        void SetupConnectionString(string ConnectionString);

        /*===========================================  GetDataSet =============================================================*/
        Task<int> ExecuteListAsync(string ProcedureName, List<dynamic> Parameters, bool IsOutParam = false);

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

        // --------------------new -----------------------------

        (T, Q) Get<T, Q>(string ProcedureName, dynamic Parameters = null, bool OutParam = false)
            where T : new()
            where Q : new();

        Task<string> BatchInsetUpdate(string firstProcedure, dynamic parameters, string tableName, List<object> secondQuery);
        Task<string> BatchInsetUpdate(string firstProcedure, dynamic parameters, List<object> secondQuery);
        //Task<string> BatchInsetUpdate(string tableName, List<object> queryData, bool isDirectCall = false);
        Task<string> BatchInsetUpdate<T>(List<object> data);
    }
}
