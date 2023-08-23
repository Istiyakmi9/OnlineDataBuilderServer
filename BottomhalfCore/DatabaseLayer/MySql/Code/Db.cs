using BottomhalfCore.DatabaseLayer.Common.Code;
using BottomhalfCore.Services.Code;
using ModalLayer;
using ModalLayer.MarkerInterface;
using ModalLayer.Modal;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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
        private string _connectionString;

        public Db() { }

        public void SetupConnectionString(string ConnectionString)
        {
            _connectionString = ConnectionString;
        }

        /*===========================================  Bulk Insert Update =====================================================*/


        #region Common

        public MySqlCommand AddCommandParameter(MySqlCommand cmd, DbParam[] param)
        {
            var date = DateTime.Now;
            var defaultDate = Convert.ToDateTime("1970-01-01");
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
            catch (Exception ex)
            {
                throw ex;
            }

            return genericReaderData;
        }

        private string CreateUpdateQuery(List<PropertyInfo> properties)
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

            var update = CreateUpdateQuery(properties);

            rows.ForEach(x =>
            {
                if (i != 0)
                    delimiter = ",";

                query.AppendLine(CreateQueryRow(affectedValue, x, properties, delimiter, $"{primaryKey} {(i + 1)}"));
                i++;
            });

            return string.Concat(query.ToString(), "On duplicate key update ", update.ToString());
        }

        private string PrepareQuery(OperationDetail operationDetail, List<object> rows, string affectedValue)
        {
            int i = 0;
            var properties = operationDetail.props;
            StringBuilder query = new StringBuilder();
            // query.AppendLine($"insert into {operationDetail.tableName} values ");
            string primaryKey = "@id + ";

            string delimiter = string.Empty;

            var update = CreateUpdateQuery(properties);

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
                prop = x.GetType().GetProperty(properties[i].Name);
                value = prop.GetValue(x, null);

                if (i != 0)
                {
                    delimiter = ",";
                }
                else
                {
                    if (value.ToString() == "0")
                    {
                        builder.Append(autoIncrementValue);
                        i++;
                        continue;
                    }
                }

                if (value != null)
                {
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
                        if (prop.PropertyType == typeof(string))
                            builder.Append($"{delimiter} '{value}'");
                        else if (prop.PropertyType == typeof(DateTime) || prop.PropertyType == typeof(DateTime?))
                        {
                            var dt = Convert.ToDateTime(value);
                            builder.Append($"{delimiter} '{dt.ToString("yyyy-MM-dd HH:mm:ss.fff")}'");
                        }
                        else
                            builder.Append(delimiter + value);
                    }
                }
                else
                {
                    builder.Append($"{delimiter} null");
                }

                i++;
            }

            builder.Append(")");
            return builder.ToString();
        }

        private OperationDetail SetTargetDataPropertyInfoList(List<object> data, bool findKeys = false)
        {
            OperationDetail operationDetail = new OperationDetail();
            if (data == null || data.Count == 0)
                throw HiringBellException.ThrowBadRequest("[SetTargetDataPropertyInfoList]: Target data is null or empty. BatchInserUpdate second query data.");

            var type = data.First().GetType();
            operationDetail.props = type.GetProperties().ToList();

            if (operationDetail.props == null || operationDetail.props.Count == 0)
                throw HiringBellException.ThrowBadRequest("[SetTargetDataPropertyInfoList]: Target data is null or empty. BatchInserUpdate second query data.");

            if (findKeys)
            {
                var tableAttr = type.GetCustomAttribute<Table>(false);
                if (tableAttr == null)
                    throw HiringBellException.ThrowBadRequest("[SetTargetDataPropertyInfoList]: Second table name not found. Db layer exception.");

                operationDetail.tableName = tableAttr._name;
                foreach (var prop in operationDetail.props)
                {
                    var primaryAttr = prop.GetCustomAttribute<Primary>(false);
                    if (primaryAttr != null)
                    {
                        operationDetail.primaryKey = primaryAttr.GetName();
                        break;
                    }
                }

                if (string.IsNullOrEmpty(operationDetail.primaryKey))
                    throw HiringBellException.ThrowBadRequest("[GetTableDetail]: Second table primary key not found. Db layer exception.");
            }

            return operationDetail;
        }

        public async Task<string> BatchInsetUpdate(string firstProcedure, dynamic parameters, List<object> secondQuery)
        {
            OperationDetail operationDetail = SetTargetDataPropertyInfoList(secondQuery, true);
            return await NativeBatchInsetUpdateMulti(firstProcedure, parameters, operationDetail, secondQuery);
        }

        public async Task<string> BatchInsetUpdate(string firstProcedure, dynamic parameters, string tableName, List<object> secondQuery)
        {
            OperationDetail operationDetail = SetTargetDataPropertyInfoList(secondQuery);
            operationDetail.tableName = tableName;
            operationDetail.primaryKey = DbProcedure.getKey(operationDetail.tableName);
            return await NativeBatchInsetUpdateMulti(firstProcedure, parameters, operationDetail, secondQuery);
        }

        private async Task<string> NativeBatchInsetUpdateMulti(string firstProcedure, dynamic parameters, OperationDetail operationDetail, List<object> secondQuery)
        {
            try
            {
                string statusMessage = null;
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    using (MySqlTransaction transaction = connection.BeginTransaction())
                    {
                        try
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
                                statusMessage = command.Parameters["_ProcessingResult"].Value.ToString();
                                if (pRowsAffected > 0 || (statusMessage != "0" && !string.IsNullOrEmpty(statusMessage)))
                                {
                                    if (statusMessage != "0" && statusMessage != "")
                                    {
                                        string pKey = operationDetail.primaryKey;
                                        command.Parameters.Clear();
                                        command.CommandType = CommandType.StoredProcedure;
                                        command.CommandText = "sp_dynamic_query_ins_upd";

                                        command.Parameters.Add("_TableName", MySqlDbType.VarChar, 50).Value = operationDetail.tableName;
                                        command.Parameters.Add("_PrimaryKey", MySqlDbType.VarChar, 50).Value = pKey;
                                        command.Parameters.Add("_Rows", MySqlDbType.VarChar, 50).Value = PrepareQuery(operationDetail, secondQuery, statusMessage);
                                        command.Parameters.Add("_ProcessingResult", MySqlDbType.VarChar, 100).Direction = ParameterDirection.Output;

                                        int rowsAffected = await command.ExecuteNonQueryAsync();
                                        if (rowsAffected > 0)
                                        {
                                            statusMessage = command.Parameters["_ProcessingResult"].Value.ToString();
                                            await transaction.CommitAsync();
                                        }
                                        else
                                        {
                                            await transaction.RollbackAsync();
                                        }
                                    }
                                    else
                                    {
                                        await transaction.RollbackAsync();
                                    }
                                }
                            }
                        }
                        catch
                        {
                            await transaction.RollbackAsync();
                            throw;
                        }
                    }
                }

                return statusMessage;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion


        #region BULK INSERT UPDATE SIGLE TABLE

        private string PrepareQuerySingle(List<object> rows, OperationDetail operationDetail)
        {
            StringBuilder query = new StringBuilder();
            string delimiter = string.Empty;
            var update = CreateUpdateQuery(operationDetail.props);

            int i = 0;
            rows.ForEach(x =>
            {
                if (i != 0)
                    delimiter = ",";

                query.AppendLine(BuildInsertQuery(x, operationDetail, i));
                i++;
            });

            return string.Concat(query.ToString(), "On duplicate key update ", update.ToString());
        }

        private string BuildInsertQuery(object x, OperationDetail operationDetail, int rowIndex)
        {
            List<PropertyInfo> properties = operationDetail.props;
            string delimiter = "";
            object value = null;
            StringBuilder builder = new StringBuilder();

            if (rowIndex != 0)
                builder.Append($",(");
            else
                builder.Append($"(");

            int i = 0;
            PropertyInfo prop = null;
            while (i < properties.Count)
            {
                prop = x.GetType().GetProperty(properties[i].Name);
                value = prop.GetValue(x, null);

                if (prop.Name == operationDetail.primaryKey)
                {
                    if (value == null && value.ToString() == "" || value.ToString() == "0")
                    {
                        builder.Append($"@id + {rowIndex + 1},");
                        i++;
                        continue;
                    }
                }

                if (value != null)
                {
                    if (prop.PropertyType == typeof(DateTime) || prop.PropertyType == typeof(Nullable<DateTime>))
                    {
                        var datetime = Convert.ToDateTime(value);
                        if (datetime.Year == 1)
                            value = Convert.ToDateTime("1/1/1976").ToString("yyyy-MM-dd HH:mm:ss.fff");
                        else
                            value = Convert.ToDateTime(datetime).ToString("yyyy-MM-dd HH:mm:ss.fff");

                        builder.Append($"{delimiter} '{value}'");
                    }
                    else if (prop.PropertyType == typeof(string))
                    {
                        builder.Append($"{delimiter} '{value}'");
                    }
                    else
                    {
                        builder.Append(delimiter + value);
                    }
                }
                else
                {
                    builder.Append($"{delimiter} null");
                }


                delimiter = ",";
                i++;
            }

            builder.Append(")");
            return builder.ToString();
        }


        private static OperationDetail FindTableKeyDetail(List<object> data, Type tableType)
        {
            if (tableType == null)
                throw new HiringBellException("Table model should not be null.");

            OperationDetail operationDetail = new OperationDetail
            {
                tableName = null,
                primaryKey = null,
                props = data.First().GetType().GetProperties().ToList()
            };

            var tableAttr = tableType.GetCustomAttribute<TableAttribute>(false);
            if (tableAttr == null)
                throw new HiringBellException("Table name is not defined.");

            operationDetail.tableName = tableAttr.Name;
            PropertyInfo[] props = tableType.GetProperties();

            int i = 0;
            while (i < props.Length)
            {
                var prop = props.ElementAt(i);
                var keyAttribute = prop.GetCustomAttribute<KeyAttribute>(false);
                if (keyAttribute == null)
                {
                    throw new HiringBellException("Primary key is not defined.");
                }
                else
                {
                    operationDetail.primaryKey = prop.Name;
                    break;
                }

                i++;
            }

            return operationDetail;
        }

        public async Task<string> BatchInsetUpdate<T>(List<object> data)
        {
            Type tableType = typeof(T);
            OperationDetail operationDetail = FindTableKeyDetail(data, tableType);
            operationDetail.isInsertOperation = true;
            return await NativeInsetUpdate(data, operationDetail);
        }

        public async Task<string> NativeInsetUpdate(List<object> secondQuery, OperationDetail operationDetail)
        {
            string statusMessage = null;
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
                            string pKey = operationDetail.primaryKey;
                            command.Parameters.Clear();
                            command.Connection = connection;

                            command.CommandType = CommandType.StoredProcedure;
                            command.CommandText = "sp_dynamic_query_ins_upd";

                            command.Parameters.Add("_TableName", MySqlDbType.VarChar, 50).Value = operationDetail.tableName;
                            command.Parameters.Add("_PrimaryKey", MySqlDbType.VarChar, 50).Value = pKey;
                            command.Parameters.Add("_Rows", MySqlDbType.VarChar, 50).Value = PrepareQuerySingle(secondQuery, operationDetail);
                            command.Parameters.Add("_ProcessingResult", MySqlDbType.VarChar, 100).Direction = ParameterDirection.Output;

                            int rowsAffected = await command.ExecuteNonQueryAsync();
                            if (rowsAffected > 0)
                            {
                                statusMessage = rowsAffected.ToString();
                                await transaction.CommitAsync();
                            }
                            else
                            {
                                await transaction.RollbackAsync();
                            }
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    }
                }

                return statusMessage;
            }
        }


        //public async Task<string> BatchInsetUpdate(string tableName, List<object> data, bool isDirectCall = false)
        //{
        //    OperationDetail operationDetail = new OperationDetail
        //    {
        //        tableName = tableName,
        //        primaryKey = DbProcedure.getKey(tableName),
        //        props = data.First().GetType().GetProperties().ToList()
        //    };

        //    return await NativeBatchInsetUpdate(data, operationDetail, isDirectCall);
        //}

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

    public class OperationDetail
    {
        public List<PropertyInfo> props { set; get; }
        public string tableName { set; get; }
        public string primaryKey { set; get; }
        public bool isInsertOperation { set; get; }
        public string latestKeyQuery { set; get; }
    }
}
