using BottomhalfCore.DatabaseLayer.Common.Code;
using BottomhalfCore.Services.Code;
using ModalLayer;
using ModalLayer.Modal;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static ApplicationConstants;

namespace BottomhalfCore.DatabaseLayer.MySql.Code
{
    public class Db : IDb
    {
        private MySqlConnection con = null;
        private MySqlCommand cmd = null;
        private MySqlTransaction transaction = null;
        private bool IsTransactionStarted = false;
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


        #region Common

        public MySqlCommand AddCommandParameter(MySqlCommand cmd, DbParam[] param)
        {
            var date = DateTime.Now;
            var defaultDate = Convert.ToDateTime("1976-01-01");
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
                                if (p.Value != null && Convert.ToDateTime(p.Value).Year != 1)
                                {
                                    date = Convert.ToDateTime(p.Value);
                                    date = DateTime.SpecifyKind(date, DateTimeKind.Utc);
                                    cmd.Parameters.Add(p.ParamName, MySqlDbType.DateTime).Value = date;
                                }
                                else
                                {
                                    cmd.Parameters.Add(p.ParamName, MySqlDbType.DateTime).Value = defaultDate;
                                }
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
            string TypeName = string.Empty;
            DateTime date = DateTime.Now;
            DateTime defaultDate = Convert.ToDateTime("1976-01-01");
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
                        props.ForEach(x =>
                        {
                            if (fieldNames.Contains(x.Name))
                            {
                                if (dataReader[x.Name] != DBNull.Value)
                                {
                                    try
                                    {
                                        if (x.PropertyType.IsGenericType)
                                            TypeName = x.PropertyType.GenericTypeArguments.First().Name;
                                        else
                                            TypeName = x.PropertyType.Name;

                                        switch (TypeName)
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
                                            case nameof(DateTime):
                                                if (dataReader[x.Name].ToString() != null)
                                                {
                                                    date = Convert.ToDateTime(dataReader[x.Name].ToString());
                                                    date = DateTime.SpecifyKind(date, DateTimeKind.Utc);
                                                    x.SetValue(t, date);
                                                }
                                                else
                                                {
                                                    x.SetValue(t, defaultDate);
                                                }
                                                break;
                                            default:
                                                x.SetValue(t, dataReader[x.Name]);
                                                break;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
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
                throw ex;
            }
            catch (Exception ex)
            {
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

        private string CreateUpdateQuery(object row, List<PropertyInfo> properties)
        {
            int i = 0;
            string delimiter = "";
            StringBuilder update = new StringBuilder();
            while (i < properties.Count())
            {
                if (i != 0)
                {
                    if (i > 1)
                        delimiter = ",";

                    update.Append($"{delimiter} {properties[i].Name} = values({properties[i].Name})");
                }

                i++;
            }

            return update.ToString();
        }


        #region MULTI TABLE INSERT UPDATE

        private string PrepareQuery(List<object> rows, string affectedValue, string tableName = null, long lastKey = -1)
        {
            int i = 0;
            var first = rows.First();
            var properties = first.GetType().GetProperties().ToList();
            StringBuilder query = new StringBuilder();
            if (tableName != null)
                query.AppendLine($"insert into {tableName} values ");

            string primaryKey = string.Empty;
            if (lastKey == -1)
                primaryKey = "@id + ";
            else
                primaryKey = $"{lastKey.ToString()} + ";

            string delimiter = string.Empty;

            var update = CreateUpdateQuery(first, properties);

            rows.ForEach(x =>
            {
                if (i != 0)
                    delimiter = ",";

                query.AppendLine(CreateQueryRow(affectedValue, x, properties, delimiter, $"{primaryKey} {(i + 1)}"));
                i++;
            });

            return string.Concat(query.ToString(), "On duplicate key update ", update.ToString());
        }

        private string CreateQueryRow(string affectedValue, object x, List<PropertyInfo> properties, string rowDelimiter, string autoIncrementValue)
        {
            string delimiter = "";
            object value = null;
            StringBuilder builder = new StringBuilder();

            builder.Append($"{rowDelimiter}(");

            int i = 0;
            PropertyInfo prop = null;
            while (i < properties.Count)
            {
                if (i != 0)
                {
                    delimiter = ",";
                }
                else
                {
                    builder.Append(autoIncrementValue);
                    i++;
                    continue;
                }

                prop = x.GetType().GetProperty(properties[i].Name);
                value = prop.GetValue(x, null);
                if (value.ToString() == ApplicationConstants.LastInsertedKey)
                {
                    value = Convert.ChangeType(affectedValue, prop.PropertyType);
                    builder.Append($"{delimiter} '{value}'");
                }
                else if (value.ToString() == ApplicationConstants.LastInsertedNumericKey)
                {
                    value = Convert.ChangeType(affectedValue, prop.PropertyType);
                    builder.Append(delimiter + value);
                }
                else
                {
                    if (prop.PropertyType == typeof(string) || prop.PropertyType == typeof(DateTime))
                        builder.Append($"{delimiter} '{value}'");
                    else
                        builder.Append(delimiter + value);
                }

                i++;
            }

            builder.Append(")");
            return builder.ToString();
        }

        public async Task<int> ConsicutiveBatchInset(string firstProcedure, dynamic parameters, string secondProcedure, List<object> secondQuery)
        {
            try
            {
                int rowsAffected = 0;
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    using (MySqlTransaction transaction = connection.BeginTransaction())
                    {
                        using (MySqlCommand command = new MySqlCommand())
                        {
                            Utility util = new Utility();

                            command.CommandType = CommandType.StoredProcedure;
                            command.CommandText = firstProcedure;
                            command.Connection = connection;

                            object userType = parameters;
                            var properties = userType.GetType().GetProperties().ToList();
                            util.BindParametersWithValue(parameters, properties, command, true);
                            int pRowAffected = await command.ExecuteNonQueryAsync();
                            if (pRowAffected > 0)
                            {
                                var statusMessage = command.Parameters["_ProcessingResult"].Value.ToString();
                                if (statusMessage != "0" && statusMessage != "")
                                {
                                    string pKey = DbProcedure.getKey(secondProcedure);
                                    command.Parameters.Clear();
                                    command.CommandType = CommandType.Text;
                                    command.CommandText = $"select {pKey} from {secondProcedure} order by {pKey} desc limit 1;";
                                    var lastKey = await command.ExecuteScalarAsync();
                                    long lastValue = 0;
                                    if (lastKey == null)
                                        lastValue = 0;
                                    else
                                        lastValue = Convert.ToInt64(lastKey);

                                    var rows = PrepareQuery(secondQuery, statusMessage, secondProcedure, lastValue);
                                    command.Parameters.Clear();
                                    command.CommandType = CommandType.Text;
                                    command.CommandText = rows;

                                    rowsAffected = await command.ExecuteNonQueryAsync();
                                    if (rowsAffected > 0)
                                        await transaction.CommitAsync();
                                    else
                                        await transaction.RollbackAsync();
                                }
                                else
                                {
                                    await transaction.RollbackAsync();
                                }
                            }
                        }
                    }
                }

                return rowsAffected;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<int> BatchInsetUpdate(string firstProcedure, dynamic parameters, string secondProcedure, List<object> secondQuery)
        {
            return await NativeBatchInsetUpdateMulti(firstProcedure, parameters, secondProcedure, secondQuery);
        }

        private async Task<int> NativeBatchInsetUpdateMulti(string firstProcedure, dynamic parameters, string secondProcedure, List<object> secondQuery)
        {
            try
            {
                int rowsAffected = 0;
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    using (MySqlTransaction transaction = connection.BeginTransaction())
                    {
                        using (MySqlCommand command = new MySqlCommand())
                        {
                            Utility util = new Utility();

                            command.CommandType = CommandType.StoredProcedure;
                            command.CommandText = firstProcedure;
                            command.Connection = connection;

                            object userType = parameters;
                            var properties = userType.GetType().GetProperties().ToList();
                            util.BindParametersWithValue(parameters, properties, command, true);
                            int pRowsAffected = await command.ExecuteNonQueryAsync();
                            if (pRowsAffected > 0)
                            {
                                var statusMessage = command.Parameters["_ProcessingResult"].Value.ToString();
                                if (statusMessage != "0" && statusMessage != "")
                                {
                                    string pKey = DbProcedure.getKey(secondProcedure);
                                    var rows = PrepareQuery(secondQuery, statusMessage);
                                    command.Parameters.Clear();
                                    command.CommandType = CommandType.StoredProcedure;
                                    command.CommandText = "sp_dynamic_query_ins_upd";

                                    command.Parameters.AddWithValue("_TableName", secondProcedure);
                                    command.Parameters.AddWithValue("_PrimaryKey", pKey);
                                    command.Parameters.AddWithValue("_Rows", rows);

                                    rowsAffected = await command.ExecuteNonQueryAsync();
                                    if (rowsAffected > 0)
                                        await transaction.CommitAsync();
                                    else
                                        await transaction.RollbackAsync();
                                }
                                else
                                {
                                    await transaction.RollbackAsync();
                                }
                            }
                        }
                    }
                }

                return rowsAffected;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion


        #region BULK INSERT UPDATE SIGLE TABLE

        private string PrepareQuerySingle(List<object> rows, string tableName)
        {
            int i = 0;
            var first = rows.First();
            var properties = first.GetType().GetProperties().ToList();
            StringBuilder query = new StringBuilder();
            if (tableName != null)
                query.AppendLine($"insert into {tableName} values ");

            string primaryKey = "@id + ";
            string delimiter = string.Empty;

            var update = CreateUpdateQuery(first, properties);

            rows.ForEach(x =>
            {
                if (i != 0)
                    delimiter = ",";

                query.AppendLine(CreateQuerySingleTableRows(x, properties, delimiter, $"{primaryKey} {(i + 1)}"));
                i++;
            });

            return string.Concat(query.ToString(), "On duplicate key update ", update.ToString());
        }

        private string CreateQuerySingleTableRows(object x, List<PropertyInfo> properties, string rowDelimiter, string autoIncrementValue)
        {
            string delimiter = "";
            object value = null;
            StringBuilder builder = new StringBuilder();

            builder.Append($"{rowDelimiter}(");

            int i = 0;
            PropertyInfo prop = null;
            while (i < properties.Count)
            {
                if (i != 0)
                {
                    delimiter = ",";
                }
                else
                {
                    builder.Append(autoIncrementValue);
                    i++;
                    continue;
                }

                prop = x.GetType().GetProperty(properties[i].Name);
                value = prop.GetValue(x, null);
                if (prop.PropertyType == typeof(string) || prop.PropertyType == typeof(DateTime))
                    builder.Append($"{delimiter} '{value}'");
                else
                    builder.Append(delimiter + value);

                i++;
            }

            builder.Append(")");
            return builder.ToString();
        }

        public async Task<int> BatchInsetUpdate(string procedureName, List<object> data)
        {
            return await NativeBatchInsetUpdate(procedureName, data);
        }

        public async Task<int> NativeBatchInsetUpdate(string procedureName, List<object> secondQuery)
        {
            int rowsAffected = 0;
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                using (MySqlTransaction transaction = connection.BeginTransaction())
                {
                    using (MySqlCommand command = new MySqlCommand())
                    {
                        try
                        {
                            Utility util = new Utility();
                            string pKey = DbProcedure.getKey(procedureName);
                            var rows = PrepareQuerySingle(secondQuery, procedureName);
                            command.Parameters.Clear();
                            command.CommandType = CommandType.StoredProcedure;
                            command.CommandText = "sp_dynamic_query_ins_upd";
                            command.Connection = connection;

                            command.Parameters.AddWithValue("_TableName", procedureName);
                            command.Parameters.AddWithValue("_PrimaryKey", pKey);
                            command.Parameters.AddWithValue("_Rows", rows);

                            rowsAffected = await command.ExecuteNonQueryAsync();
                            if (rowsAffected > 0)
                                await transaction.CommitAsync();
                            else
                                await transaction.RollbackAsync();
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    }
                }

                return rowsAffected;
            }
        }

        #endregion


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
                                object userType = Parameters;
                                var properties = userType.GetType().GetProperties().ToList();
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

        public DataSet GetDataSet(string ProcedureName, dynamic Parameters = null, bool OutParam = false)
        {
            Task<DataSet> taskResult = GetDataSetAsync(ProcedureName, Parameters, OutParam);
            return taskResult.Result;
        }

        public async Task<DataSet> GetDataSetAsync(string ProcedureName, dynamic Parameters = null, bool OutParam = false)
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

        private async Task<DbResult> ExecuteSingleAsync(string ProcedureName, dynamic Parameters, bool IsOutParam = false)
        {
            DbResult dbResult = new DbResult();
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
                            dbResult.rowsEffected = command.ExecuteNonQuery();
                            if (IsOutParam)
                                dbResult.statusMessage = command.Parameters["_ProcessingResult"].Value.ToString();
                            else
                                dbResult.statusMessage = dbResult.rowsEffected.ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return await Task.FromResult(dbResult);
        }

        public async Task<int> BulkExecuteAsync<T>(string ProcedureName, List<T> Parameters, bool IsOutParam = false)
        {
            return await ExecuteListAsync(ProcedureName, Parameters, IsOutParam);
        }

        public async Task<int> ExecuteListAsync<T>(string ProcedureName, List<T> Parameters, bool IsOutParam = false)
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

                            command.Parameters.Clear();
                            object userType = Parameters.First();
                            var properties = userType.GetType().GetProperties().ToList();
                            util.Addarameters(properties, command, IsOutParam);

                            command.CommandType = CommandType.StoredProcedure;
                            command.CommandText = ProcedureName;
                            connection.Open();
                            command.Connection = connection;

                            int i = 0;
                            while (i < Parameters.Count)
                            {
                                dynamic data = Parameters.ElementAt(i);
                                util.BindParametersValue(data, properties, command, false);
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

        public DbResult Execute(string ProcedureName, dynamic Parameters, bool OutParam = false)
        {
            var dbResult = ExecuteSingleAsync(ProcedureName, Parameters, OutParam);
            return dbResult.Result;
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

        public Task<int> ExecuteListAsync(string ProcedureName, List<dynamic> Parameters, bool IsOutParam = false)
        {
            throw new NotImplementedException();
        }
    }
}
