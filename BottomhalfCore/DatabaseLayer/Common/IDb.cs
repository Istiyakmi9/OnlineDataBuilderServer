using BottomhalfCore.DatabaseLayer.Common.Code;
using System;
using System.Data;

namespace BottomhalfCore.DatabaseLayer.Common.Code
{
    public interface IDb
    {
        DataSet FetchResult(string ProcedureName);
        /*===========================================  GetDataSet =============================================================*/
        DataSet GetDataset(string ProcedureName, DbParam[] param);
        DataSet GetDataset(string ProcedureName);
        int BatchInsert(string ProcedureName, DataSet TableSet, Boolean IsOutparam);
        DataSet GetDataset(string ProcedureName, DbParam[] param, bool OutParam, ref string ProcessingStatus);
        Object ExecuteSingle(string ProcedureName, DbParam[] param, bool OutParam);
        string ExecuteNonQuery(string ProcedureName, DbParam[] param, bool OutParam);
        void UserDefineTypeBulkInsert(DataSet dataset, string ProcedureName, bool OutParam);
        string XmlBatchInsertUpdate(string ProcedureName, string XmlData, bool OutParam);
        string InsertUpdateBatchRecord(string ProcedureName, DataTable table, Boolean OutParam = false);
        string SqlBulkInsert(DataTable table, string TableName);
        DataSet CommonadBuilderBulkInsertUpdate(string SelectQuery, string TableName);
    }
}
