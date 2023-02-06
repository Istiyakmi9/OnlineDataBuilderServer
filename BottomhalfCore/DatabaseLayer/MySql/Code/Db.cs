using BottomhalfCore.DatabaseLayer.Common.Code;
using ModalLayer;
using ModalLayer.Modal;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace BottomhalfCore.DatabaseLayer.MySql.Code
{
    public class Db : IDb
    {
        private MySqlConnection con = null;
        private MySqlCommand cmd = null;
        private MySqlDataAdapter da = null;
        private MySqlCommandBuilder builder = null;
        private MySqlTransaction transaction = null;
        private MySqlDataReader reader = null;
        private bool IsTransactionStarted = false;
        private DataSet ds = null;
        private readonly string _connectionString;
        private static readonly object _lock = new object();

        public Db(string ConnectionString)
        {
            _connectionString = ConnectionString;
            con = new MySqlConnection();
            cmd = new MySqlCommand();
        }

        private async Task<MySqlConnection> OpenConnectionSecurelyAsync()
        {
            if (cmd != null)
                cmd.Parameters.Clear();

            var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            return connection;
        }

        private MySqlConnection OpenConnectionSecurely()
        {
            if (cmd != null)
                cmd.Parameters.Clear();

            var connection = new MySqlConnection(_connectionString);
            connection.Open();
            return connection;
        }

        private MySqlConnection GetConnectionAsync()
        {
            if (cmd != null)
                cmd.Parameters.Clear();

            var connection = new MySqlConnection(_connectionString);
            return connection;
        }

        private MySqlConnection GetConnection()
        {
            if (cmd != null)
                cmd.Parameters.Clear();

            var connection = new MySqlConnection(_connectionString);
            return connection;
        }

        private void HandleSqlException(Exception ex)
        {
            if (this.IsTransactionStarted && this.transaction != null)
            {
                this.IsTransactionStarted = false;
                this.transaction.Rollback();
            }

            con = cmd.Connection;
            if (this.con.State == ConnectionState.Open || this.con.State == ConnectionState.Broken)
                this.con.Close();
        }

        public void StartTransaction(IsolationLevel isolationLevel)
        {
            //try
            //{
            //    this.IsTransactionStarted = true;
            //    if (this.con.State == ConnectionState.Open || this.con.State == ConnectionState.Broken)
            //        this.con.Close();

            //    this.con.Open();
            //    transaction = this.con.BeginTransaction(isolationLevel);
            //}
            //catch (Exception ex)
            //{
            //    if (con.State == ConnectionState.Broken || con.State == ConnectionState.Open)
            //        con.Close();

            //    throw new HiringBellException("Fail to perform Database operation.", ex);
            //}
        }

        public void Commit()
        {
            //try
            //{
            //    this.IsTransactionStarted = false;
            //    this.transaction.Commit();
            //}
            //catch (Exception ex)
            //{
            //    throw new HiringBellException("Fail to perform Database operation.", ex);
            //}
            //finally
            //{
            //    if (con.State == ConnectionState.Broken || con.State == ConnectionState.Open)
            //        con.Close();
            //}
        }

        public void RollBack()
        {
            //try
            //{
            //    if (this.IsTransactionStarted && this.transaction != null)
            //    {
            //        this.IsTransactionStarted = false;
            //        this.transaction.Rollback();
            //    }
            //}
            //catch (Exception ex)
            //{
            //    throw new HiringBellException("Fail to perform Database operation.", ex);
            //}
            //finally
            //{
            //    if (con.State == ConnectionState.Broken || con.State == ConnectionState.Open)
            //        con.Close();
            //}
        }

        private async Task CloseSecurelyAsync()
        {
            con = cmd.Connection;
            if (!this.IsTransactionStarted && (con.State == ConnectionState.Open || con.State == ConnectionState.Broken))
                await con.CloseAsync();
        }

        private void CloseSecurely()
        {
            con = cmd.Connection;
            if (!this.IsTransactionStarted && (con.State == ConnectionState.Open || con.State == ConnectionState.Broken))
                con.Close();
        }

        /*===========================================  GetDataSet =============================================================*/

        #region GetDataSet



        public DataSet GetDataset(string ProcedureName, DbParam[] param)
        {
            try
            {
                ds = null;
                cmd = new MySqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = ProcedureName;
                cmd.Connection = GetConnection();
                cmd = AddCommandParameter(cmd, param);
                da = new MySqlDataAdapter();
                da.SelectCommand = cmd;
                ds = new DataSet();
                da.Fill(ds);
                if (ds.Tables.Count > 0)
                {
                    return ds;
                }
            }
            catch (MySqlException ex)
            {
                HandleSqlException(ex);
                throw ex;
            }
            catch (Exception ex)
            {
                HandleSqlException(ex);
                throw ex;
            }

            return ds;
        }


        public string ExecuteNonQuery(string ProcedureName, DbParam[] param, bool OutParam)
        {
            string state = "";
            string fileId = DateTime.Now.Ticks.ToString();
            try
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = ProcedureName;
                cmd.Connection = OpenConnectionSecurely();
                cmd = AddCommandParameter(cmd, param);
                if (OutParam)
                {
                    cmd.Parameters.Add("_ProcessingResult", MySqlDbType.VarChar, 100).Direction = ParameterDirection.Output;
                }

                var result = cmd.ExecuteNonQuery();
                if (OutParam)
                    state = cmd.Parameters["_ProcessingResult"].Value.ToString();
                else
                    state = result.ToString();
                return state;
            }
            catch (Exception ex)
            {
                HandleSqlException(ex);
                throw ex;
            }
            finally
            {
                CloseSecurely();
            }
        }

        #endregion







        /*===========================================  Bulk Insert Update =====================================================*/

        #region Bulk Insert Update

        private async Task<DbResult> BatchInsertUpdateCoreAsync(string ProcedureName, DataTable table, Boolean IsOutparam)
        {
            string statusMessage = string.Empty;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = ProcedureName;
            cmd.UpdatedRowSource = UpdateRowSource.None;
            cmd.Connection = await OpenConnectionSecurelyAsync();

            int Initial = 0;
            Type ColumnType = null;
            string ColumnValue = string.Empty;
            foreach (DataColumn column in table.Columns)
            {
                ColumnType = table.Rows[Initial][column].GetType();
                if (ColumnType == typeof(System.String))
                    cmd.Parameters.Add("_" + column.ColumnName, MySqlDbType.VarChar, ColumnValue.Length, column.ColumnName);
                else if (ColumnType == typeof(System.Int16))
                    cmd.Parameters.Add("_" + column.ColumnName, MySqlDbType.Int16, 2, column.ColumnName);
                else if (ColumnType == typeof(System.Int32))
                    cmd.Parameters.Add("_" + column.ColumnName, MySqlDbType.Int32, 4, column.ColumnName);
                else if (ColumnType == typeof(System.Double) || ColumnType == typeof(System.Single))
                    cmd.Parameters.Add("_" + column.ColumnName, MySqlDbType.Decimal, 8, column.ColumnName);
                else if (ColumnType == typeof(System.Int64) || ColumnType == typeof(System.Decimal))
                    cmd.Parameters.Add("_" + column.ColumnName, MySqlDbType.Int64, 8, column.ColumnName);
                else if (ColumnType == typeof(System.Char))
                    cmd.Parameters.Add("_" + column.ColumnName, MySqlDbType.VarChar, 1, column.ColumnName);
                else if (ColumnType == typeof(System.Boolean))
                    cmd.Parameters.Add("_" + column.ColumnName, MySqlDbType.Bit, 1, column.ColumnName);
                else if (ColumnType == typeof(System.DateTime))
                    cmd.Parameters.Add("_" + column.ColumnName, MySqlDbType.DateTime, 8, column.ColumnName);
                else
                    cmd.Parameters.Add("_" + column.ColumnName, MySqlDbType.VarChar, ColumnValue.Length, column.ColumnName);
            }

            if (IsOutparam)
                cmd.Parameters.Add("_ProcessingResult", MySqlDbType.VarChar, ColumnValue.Length, "ProcessingResult").Direction = ParameterDirection.Output;

            MySqlDataAdapter da = new MySqlDataAdapter();
            da.InsertCommand = cmd;
            da.UpdateBatchSize = 4;
            var state = await da.UpdateAsync(table);
            return DbResult.Build(state, statusMessage);
        }

        public int BatchInsert(string ProcedureName, DataTable table, Boolean IsOutparam)
        {
            int state = -1;
            try
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = ProcedureName;
                cmd.UpdatedRowSource = UpdateRowSource.None;
                cmd.Connection = OpenConnectionSecurely();

                int Initial = 0;
                Type ColumnType = null;
                string ColumnValue = string.Empty;
                foreach (DataColumn column in table.Columns)
                {
                    ColumnType = table.Rows[Initial][column].GetType();
                    if (ColumnType == typeof(System.String))
                        cmd.Parameters.Add("_" + column.ColumnName, MySqlDbType.VarChar, ColumnValue.Length, column.ColumnName);
                    else if (ColumnType == typeof(System.Int16))
                        cmd.Parameters.Add("_" + column.ColumnName, MySqlDbType.Int16, 2, column.ColumnName);
                    else if (ColumnType == typeof(System.Int32))
                        cmd.Parameters.Add("_" + column.ColumnName, MySqlDbType.Int32, 4, column.ColumnName);
                    else if (ColumnType == typeof(System.Double) || ColumnType == typeof(System.Single))
                        cmd.Parameters.Add("_" + column.ColumnName, MySqlDbType.Decimal, 8, column.ColumnName);
                    else if (ColumnType == typeof(System.Int64) || ColumnType == typeof(System.Decimal))
                        cmd.Parameters.Add("_" + column.ColumnName, MySqlDbType.Int64, 8, column.ColumnName);
                    else if (ColumnType == typeof(System.Char))
                        cmd.Parameters.Add("_" + column.ColumnName, MySqlDbType.VarChar, 1, column.ColumnName);
                    else if (ColumnType == typeof(System.Boolean))
                        cmd.Parameters.Add("_" + column.ColumnName, MySqlDbType.Bit, 1, column.ColumnName);
                    else if (ColumnType == typeof(System.DateTime))
                        cmd.Parameters.Add("_" + column.ColumnName, MySqlDbType.DateTime, 8, column.ColumnName);
                    else
                        cmd.Parameters.Add("_" + column.ColumnName, MySqlDbType.VarChar, ColumnValue.Length, column.ColumnName);
                }

                if (IsOutparam)
                    cmd.Parameters.Add("_ProcessingResult", MySqlDbType.VarChar, ColumnValue.Length, "ProcessingResult").Direction = ParameterDirection.Output;

                MySqlDataAdapter da = new MySqlDataAdapter();
                da.InsertCommand = cmd;
                da.UpdateBatchSize = 4;
                if (con.State != ConnectionState.Open)
                    con.Open();
                state = da.Update(table);
            }
            catch (Exception ex)
            {
                HandleSqlException(ex);
                throw ex;
            }
            finally
            {
                CloseSecurely();
            }

            return state;
        }

        public async Task<DbResult> BatchInsertUpdateAsync(string ProcedureName, DataTable table, Boolean IsOutparam)
        {
            try
            {
                return await this.BatchInsertUpdateCoreAsync(ProcedureName, table, IsOutparam);
            }
            catch (Exception ex)
            {
                HandleSqlException(ex);
                throw ex;
            }
            finally
            {
                await CloseSecurelyAsync();
            }
        }













        public class DbTypeDetail
        {
            public MySqlDbType DbType { set; get; }
            public int Size { set; get; }
        }


        private DbTypeDetail GetType(Type ColumnType)
        {
            if (ColumnType == typeof(System.String))
                return new DbTypeDetail { DbType = MySqlDbType.VarChar, Size = 250 };
            else if (ColumnType == typeof(System.Int64))
                return new DbTypeDetail { DbType = MySqlDbType.Int64, Size = 8 };
            else if (ColumnType == typeof(System.Int32))
                return new DbTypeDetail { DbType = MySqlDbType.Int32, Size = 4 };
            else if (ColumnType == typeof(System.Char))
                return new DbTypeDetail { DbType = MySqlDbType.VarChar, Size = 1 };
            else if (ColumnType == typeof(System.DateTime))
                return new DbTypeDetail { DbType = MySqlDbType.DateTime, Size = 10 };
            else if (ColumnType == typeof(System.Double))
                return new DbTypeDetail { DbType = MySqlDbType.Int64, Size = 8 };
            else if (ColumnType == typeof(System.Boolean))
                return new DbTypeDetail { DbType = MySqlDbType.Bit, Size = 1 };
            else
                return new DbTypeDetail { DbType = MySqlDbType.VarChar, Size = 250 };
        }

        public string InsertUpdateBatchRecord(string ProcedureName, DataTable table, Boolean OutParam = false)
        {
            try
            {
                string state = "";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = ProcedureName;
                cmd.UpdatedRowSource = UpdateRowSource.None;
                cmd.Connection = OpenConnectionSecurely();

                foreach (DataColumn column in table.Columns)
                {
                    var DbType = this.GetType(column.DataType);
                    cmd.Parameters.Add("_" + column.ColumnName, DbType.DbType, DbType.Size, column.ColumnName);
                }

                if (OutParam)
                    cmd.Parameters.Add("_ProcessingResult", MySqlDbType.Int32).Direction = ParameterDirection.Output;

                da = new MySqlDataAdapter();
                da.InsertCommand = cmd;
                da.UpdateBatchSize = 2;
                con.Open();
                int Count = da.Update(table);
                if (OutParam)
                    state = cmd.Parameters["_ProcessingResult"].Value.ToString();
                else if (Count > 0)
                    state = "Successfull";
                return state;
            }
            catch (Exception ex)
            {
                HandleSqlException(ex);
                throw ex;
            }
            finally
            {
                CloseSecurely();
            }
        }

        #endregion

        #region Common

        public MySqlCommand AddCommandParameter(MySqlCommand cmd, DbParam[] param)
        {
            cmd.Parameters.Clear();
            if (param != null)
            {
                foreach (DbParam p in param)
                {
                    if (p.IsTypeDefined)
                    {
                        if (p.Value != null)
                        {
                            if (p.Type == typeof(System.String))
                                cmd.Parameters.Add(p.ParamName, MySqlDbType.VarChar, p.Size).Value = Convert.ToString(p.Value);
                            else if (p.Type == typeof(System.Int16))
                                cmd.Parameters.Add(p.ParamName, MySqlDbType.Int16).Value = Convert.ToInt16(p.Value);
                            else if (p.Type == typeof(System.Int32))
                                cmd.Parameters.Add(p.ParamName, MySqlDbType.Int32).Value = Convert.ToInt32(p.Value);
                            else if (p.Type == typeof(System.Double))
                                cmd.Parameters.Add(p.ParamName, MySqlDbType.Float).Value = Convert.ToDouble(p.Value);
                            else if (p.Type == typeof(System.Int64))
                                cmd.Parameters.Add(p.ParamName, MySqlDbType.Int64).Value = Convert.ToInt64(p.Value);
                            else if (p.Type == typeof(System.Char))
                                cmd.Parameters.Add(p.ParamName, MySqlDbType.VarChar, 1).Value = Convert.ToChar(p.Value);
                            else if (p.Type == typeof(System.Decimal))
                                cmd.Parameters.Add(p.ParamName, MySqlDbType.Decimal).Value = Convert.ToDecimal(p.Value);
                            else if (p.Type == typeof(System.DBNull))
                                cmd.Parameters.Add(p.ParamName, MySqlDbType.Decimal).Value = Convert.DBNull;
                            else if (p.Type == typeof(System.Boolean))
                                cmd.Parameters.Add(p.ParamName, MySqlDbType.Bit).Value = Convert.ToBoolean(p.Value);
                            else if (p.Type == typeof(System.Single))
                                cmd.Parameters.Add(p.ParamName, MySqlDbType.Bit).Value = Convert.ToSingle(p.Value);
                            else if (p.Type == typeof(System.DateTime))
                            {
                                if (Convert.ToDateTime(p.Value).Year == 1)
                                    cmd.Parameters.Add(p.ParamName, MySqlDbType.DateTime).Value = Convert.ToDateTime("1/1/1976");
                                else
                                    cmd.Parameters.Add(p.ParamName, MySqlDbType.DateTime).Value = Convert.ToDateTime(p.Value);
                            }
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue(p.ParamName, DBNull.Value);
                        }
                    }
                    else
                    {
                        cmd.Parameters.Add(p.ParamName, p.Value);
                    }
                }
            }
            return cmd;
        }











        #endregion


        // ------------------------  database code with lock and thread safty ------------------------------

        private List<T> ReadAndConvertToType<T>(MySqlDataReader dataReader) where T : new()
        {
            List<T> items = new List<T>();
            try
            {
                List<PropertyInfo> props = typeof(T).GetProperties().ToList();
                if (dataReader.HasRows)
                {
                    var fieldNames = Enumerable.Range(0, dataReader.FieldCount).Select(i => dataReader.GetName(i)).ToArray();
                    while (dataReader.Read())
                    {
                        T t = new T();
                        Parallel.ForEach(props, x =>
                        {
                            if (fieldNames.Contains(x.Name))
                            {
                                if (dataReader[x.Name] != DBNull.Value)
                                {
                                    try
                                    {
                                        if (x.PropertyType.IsGenericType)
                                        {
                                            switch (x.PropertyType.GenericTypeArguments.First().Name)
                                            {
                                                case nameof(Boolean):
                                                    x.SetValue(t, Convert.ToBoolean(dataReader[x.Name]));
                                                    break;
                                                case nameof(Int32):
                                                    x.SetValue(t, Convert.ToInt32(dataReader[x.Name]));
                                                    break;
                                                case nameof(Int64):
                                                    x.SetValue(t, Convert.ToInt64(dataReader[x.Name]));
                                                    break;
                                                case nameof(Decimal):
                                                    x.SetValue(t, Convert.ToDecimal(dataReader[x.Name]));
                                                    break;
                                                default:
                                                    x.SetValue(t, dataReader[x.Name]);
                                                    break;
                                            }
                                        }
                                        else
                                        {
                                            switch (x.PropertyType.Name)
                                            {
                                                case nameof(Boolean):
                                                    x.SetValue(t, Convert.ToBoolean(dataReader[x.Name]));
                                                    break;
                                                case nameof(Int32):
                                                    x.SetValue(t, Convert.ToInt32(dataReader[x.Name]));
                                                    break;
                                                case nameof(Int64):
                                                    x.SetValue(t, Convert.ToInt64(dataReader[x.Name]));
                                                    break;
                                                case nameof(Decimal):
                                                    x.SetValue(t, Convert.ToDecimal(dataReader[x.Name]));
                                                    break;
                                                default:
                                                    x.SetValue(t, dataReader[x.Name]);
                                                    break;
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        HandleSqlException(ex);
                                        throw ex;
                                    }
                                }
                            }
                        });

                        items.Add(t);
                    }
                }
            }
            catch (MySqlException ex)
            {
                HandleSqlException(ex);
                throw ex;
            }
            catch (Exception ex)
            {
                HandleSqlException(ex);
                throw ex;
            }

            return items;
        }

        public (T, Q) GetMulti<T, Q>(string ProcedureName, dynamic Parameters = null, bool OutParam = false)
            where T : new()
            where Q : new()
        {
            return Get<T, Q>(ProcedureName, Parameters, OutParam);
        }

        public (T, Q, R) GetMulti<T, Q, R>(string ProcedureName, dynamic Parameters = null, bool OutParam = false)
            where T : new()
            where Q : new()
            where R : new()
        {
            throw new NotImplementedException();
        }

        public (List<T>, List<Q>) GetList<T, Q>(string ProcedureName, dynamic Parameters, bool OutParam = false)
            where T : new()
            where Q : new()
        {
            try
            {
                List<T> firstInstance = default(List<T>);
                List<Q> secondInstance = default(List<Q>);

                GenericReaderData genericReaderData = NativeGet<T, Q>(ProcedureName, Parameters, OutParam);

                if (genericReaderData.ResultSet.Count != 2)
                    throw HiringBellException.ThrowBadRequest("Fail to get the result. Got database error");

                if (genericReaderData.ResultSet[0] != null)
                    firstInstance = (genericReaderData.ResultSet[0] as List<T>);

                if (genericReaderData.ResultSet[1] != null)
                    secondInstance = (genericReaderData.ResultSet[1] as List<Q>);

                return (firstInstance, secondInstance);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public (List<T>, List<R>, List<Q>) GetList<T, R, Q>(string ProcedureName, dynamic Parameters, bool OutParam = false)
            where T : new()
            where R : new()
            where Q : new()
        {
            throw new NotImplementedException();
        }

        public DataSet FetchDataSet(string ProcedureName, dynamic Parameters = null, bool OutParam = false)
        {
            return GetDataSet(ProcedureName, Parameters, OutParam);
        }

        public (T, Q) Get<T, Q>(string ProcedureName, dynamic Parameters = null, bool OutParam = false)
            where T : new()
            where Q : new()
        {
            try
            {
                T firstInstance = default(T);
                Q secondInstance = default(Q);

                GenericReaderData genericReaderData = NativeGet<T, Q>(ProcedureName, Parameters, OutParam);

                if (genericReaderData.ResultSet.Count != 2)
                    throw HiringBellException.ThrowBadRequest("Fail to get the result. Got database error");

                if (genericReaderData.ResultSet[0] != null)
                    firstInstance = (genericReaderData.ResultSet[0] as List<T>).SingleOrDefault();

                if (genericReaderData.ResultSet[1] != null)
                    secondInstance = (genericReaderData.ResultSet[1] as List<Q>).SingleOrDefault();

                return (firstInstance, secondInstance);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public class GenericReaderData
        {
            public List<dynamic> ResultSet { set; get; } = new List<dynamic>();
        }

        private GenericReaderData NativeGet<T, Q>(string ProcedureName, dynamic Parameters = null, bool OutParam = false)
            where T : new()
            where Q : new()
        {
            GenericReaderData genericReaderData = new GenericReaderData();
            try
            {
                object userType = Parameters;
                List<PropertyInfo> properties = userType.GetType().GetProperties().ToList();

                lock (_lock)
                {
                    using (var connection = new MySqlConnection(_connectionString))
                    {
                        using (MySqlCommand command = new MySqlCommand())
                        {
                            Utility util = new Utility();

                            command.CommandType = CommandType.StoredProcedure;
                            command.CommandText = ProcedureName;
                            connection.Open();
                            command.Connection = connection;

                            if (Parameters != null)
                            {
                                util.BindParametersWithValue(Parameters, properties, command, OutParam);
                            }

                            var dataReader = command.ExecuteReader();
                            var firstResult = this.ReadAndConvertToType<T>(dataReader);
                            genericReaderData.ResultSet.Add(firstResult);

                            if (!dataReader.NextResult())
                                throw new HiringBellException("[DB Query] getting error while trying to read data.");

                            var secondResult = this.ReadAndConvertToType<Q>(dataReader);
                            genericReaderData.ResultSet.Add(secondResult);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return genericReaderData;
        }

        //public List<T> GetList<T>(string ProcedureName, bool OutParam = false) where T : new()
        //{
        //    List<T> result = null;
        //    try
        //    {
        //        ds = null;
        //        cmd.CommandType = CommandType.StoredProcedure;
        //        cmd.CommandText = ProcedureName;
        //        cmd.Connection = OpenConnectionSecurely();

        //        if (OutParam)
        //        {
        //            cmd.Parameters.Add("_ProcessingResult", MySqlDbType.VarChar, 100).Direction = ParameterDirection.Output;
        //        }

        //        reader = cmd.ExecuteReader();
        //        result = this.ReadAndConvertToType<T>(reader);
        //    }
        //    catch (MySqlException ex)
        //    {
        //        HandleSqlException(ex);
        //        throw ex;
        //    }
        //    catch (Exception ex)
        //    {
        //        HandleSqlException(ex);
        //        throw ex;
        //    }
        //    finally
        //    {
        //        CloseSecurely();
        //    }

        //    return result;
        //}

        public List<T> GetList<T>(string ProcedureName, dynamic Parameters = null, bool OutParam = false) where T : new()
        {
            List<T> result = this.NativeGetList<T>(ProcedureName, Parameters, OutParam);
            if (result == null)
                throw HiringBellException.ThrowBadRequest("Fail to get data.");

            return result;
        }

        private List<T> NativeGetList<T>(string ProcedureName, dynamic Parameters = null, bool OutParam = false) where T : new()
        {
            List<T> result = null;
            object userType = Parameters;
            var properties = userType.GetType().GetProperties().ToList();

            try
            {
                lock (_lock)
                {
                    using (var connection = new MySqlConnection(_connectionString))
                    {
                        using (MySqlCommand command = new MySqlCommand())
                        {
                            Utility util = new Utility();

                            command.CommandType = CommandType.StoredProcedure;
                            command.CommandText = ProcedureName;
                            connection.Open();
                            command.Connection = connection;

                            if (Parameters != null)
                            {
                                util.BindParametersWithValue(Parameters, properties, command, OutParam);
                            }
                            var dataReader = command.ExecuteReader();
                            result = this.ReadAndConvertToType<T>(dataReader);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return result;
        }

        public T Get<T>(string ProcedureName, dynamic Parameters = null, bool OutParam = false) where T : new()
        {
            T data = default(T);
            if (Parameters == null)
                Parameters = new { };

            List<T> result = this.NativeGetList<T>(ProcedureName, Parameters, OutParam);
            if (result != null)
            {
                if (result.Count > 0)
                    data = result.FirstOrDefault();
            }

            return data;
        }

        public async Task<DataSet> GetDataSet(string ProcedureName, dynamic Parameters = null, bool OutParam = false)
        {
            DataSet dataSet = new DataSet();
            try
            {
                lock (_lock)
                {
                    using (var connection = new MySqlConnection(_connectionString))
                    {
                        using (MySqlCommand command = new MySqlCommand())
                        {
                            Utility util = new Utility();
                            if (Parameters != null)
                            {
                                object userType = Parameters;
                                var properties = userType.GetType().GetProperties().ToList();
                                util.BindParametersWithValue(Parameters, properties, command);
                            }

                            command.CommandType = CommandType.StoredProcedure;
                            command.CommandText = ProcedureName;
                            command.Connection = connection;
                            using (MySqlDataAdapter dataAdapter = new MySqlDataAdapter())
                            {
                                connection.Open();
                                dataAdapter.SelectCommand = command;
                                dataAdapter.Fill(dataSet);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return await Task.FromResult(dataSet);
        }

        private async Task<int> ExecuteSingleAsync(string ProcedureName, dynamic Parameters, bool IsOutParam = false)
        {
            int rowsAffected = 0;
            try
            {
                lock (_lock)
                {
                    using (var connection = new MySqlConnection(_connectionString))
                    {
                        using (MySqlCommand command = new MySqlCommand())
                        {
                            Utility util = new Utility();
                            if (Parameters == null)
                            {
                                throw HiringBellException.ThrowBadRequest("Passed parameter is null. Please supply proper collection of data.");
                            }

                            object userType = Parameters;
                            var properties = userType.GetType().GetProperties().ToList();
                            command.CommandType = CommandType.StoredProcedure;
                            command.CommandText = ProcedureName;
                            connection.Open();
                            command.Connection = connection;

                            util.BindParametersWithValue(Parameters, properties, command, IsOutParam);
                            rowsAffected = command.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return await Task.FromResult(rowsAffected);
        }

        public async Task<int> ExecuteListAsync(string ProcedureName, List<dynamic> Parameters, bool IsOutParam = false)
        {
            int rowsAffected = 0;
            try
            {
                lock (_lock)
                {
                    using (var connection = new MySqlConnection(_connectionString))
                    {
                        using (MySqlCommand command = new MySqlCommand())
                        {
                            Utility util = new Utility();
                            if (Parameters == null)
                            {
                                throw HiringBellException.ThrowBadRequest("Passed parameter is null. Please supply proper collection of data.");
                            }

                            object userType = Parameters.First();
                            var properties = userType.GetType().GetProperties().ToList();
                            util.Addarameters(properties, command);

                            command.CommandType = CommandType.StoredProcedure;
                            command.CommandText = ProcedureName;
                            connection.Open();
                            command.Connection = connection;

                            int i = 0;
                            while (i < Parameters.Count)
                            {
                                dynamic data = Parameters.ElementAt(i);
                                util.BindParametersValue(data, properties, command, IsOutParam);
                                rowsAffected += command.ExecuteNonQuery();
                                i++;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return await Task.FromResult(rowsAffected);
        }

        public async Task<DbResult> ExecuteAsync(string ProcedureName, dynamic Parameters, bool OutParam = false)
        {
            return await ExecuteSingleAsync(ProcedureName, Parameters, OutParam);
        }

        public string Execute<T>(string ProcedureName, dynamic Parameters, bool IsOutParam)
        {
            string state = "";
            int rowsAffected = 0;
            try
            {
                lock (_lock)
                {
                    using (var connection = new MySqlConnection(_connectionString))
                    {
                        using (MySqlCommand command = new MySqlCommand())
                        {
                            Utility util = new Utility();
                            if (Parameters == null)
                            {
                                throw HiringBellException.ThrowBadRequest("Passed parameter is null. Please supply proper collection of data.");
                            }

                            object userType = Parameters;
                            var properties = userType.GetType().GetProperties().ToList();
                            command.CommandType = CommandType.StoredProcedure;
                            command.CommandText = ProcedureName;
                            connection.Open();
                            command.Connection = connection;

                            util.BindParametersWithValue(Parameters, properties, command, IsOutParam);
                            rowsAffected = command.ExecuteNonQuery();
                            if (IsOutParam)
                                state = command.Parameters["_ProcessingResult"].Value.ToString();
                            else
                                state = rowsAffected.ToString();
                            return state;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
