using ModalLayer.Modal;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Asn1.X509.Qualified;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace BottomhalfCore.DatabaseLayer.MySql.Code
{
    public class Utility
    {
        private readonly HelperService _helperService;
        public Utility()
        {
            _helperService = HelperService.Instance();
        }

        public void AddParametersWithValue(object instance, List<PropertyInfo> properties, MySqlCommand cmd, bool isOutPutPramam = false)
        {
            if (instance != null)
            {
                cmd.Parameters.Clear();

                dynamic fieldValue = null;
                PropertyInfo prop = null;
                foreach (PropertyInfo p in properties)
                {
                    prop = instance.GetType().GetProperty(p.Name);
                    if (prop != null)
                    {
                        fieldValue = prop.GetValue(instance, null);
                        if (fieldValue != null)
                        {
                            if (p.PropertyType == typeof(DateTime) || p.PropertyType == typeof(Nullable<DateTime>))
                            {
                                if (Convert.ToDateTime(fieldValue).Year == 1)
                                    cmd.Parameters.Add($"_{p.Name}", MySqlDbType.DateTime).Value = Convert.ToDateTime("1/1/1976");
                                else
                                    cmd.Parameters.Add($"_{p.Name}", MySqlDbType.DateTime).Value = fieldValue.ToString("yyyy-MM-dd HH:mm:ss.fff");
                            }
                            else if (p.PropertyType == typeof(bool) || p.PropertyType == typeof(bool?))
                            {
                                if (fieldValue != null)
                                    cmd.Parameters.Add($"_{p.Name}", MySqlDbType.Bit).Value = Convert.ToBoolean(fieldValue);
                                else
                                    cmd.Parameters.Add($"_{p.Name}", MySqlDbType.Bit).Value = null;
                            }
                            else
                                cmd.Parameters.Add($"_{p.Name}", MySqlDbType.VarChar).Value = fieldValue;
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue($"_{p.Name}", DBNull.Value);
                        }
                    }
                }
            }

            if (isOutPutPramam)
            {
                cmd.Parameters.Add("_ProcessingResult", MySqlDbType.VarChar, 100).Direction = ParameterDirection.Output;
            }
        }

        public void Addarameters(List<PropertyInfo> properties, MySqlCommand cmd, bool isOutPutPramam = false)
        {
            foreach (PropertyInfo p in properties)
            {
                var param = _helperService.GetDataType(p.PropertyType);
                if (param == null)
                    throw HiringBellException.ThrowBadRequest($"{p.Name}: parameter not found.");

                cmd.Parameters.Add($"_{p.Name}", param.DbType);
            }

            if (isOutPutPramam)
            {
                cmd.Parameters.Add("_ProcessingResult", MySqlDbType.VarChar, 100).Direction = ParameterDirection.Output;
            }
        }

        public void BindParametersValue(object instance, List<PropertyInfo> properties, MySqlCommand cmd, bool isOutPutPramam = false)
        {
            foreach (PropertyInfo p in properties)
            {
                var param = _helperService.GetDataType(p.PropertyType);
                if (param != null)
                {
                    var prop = instance.GetType().GetProperty(p.Name);
                    if (prop != null)
                    {
                        var type = Type.GetType(param.TypeQualifiedName);
                        var value = prop.GetValue(instance, null);
                        if (value != null)
                            cmd.Parameters[$"_{p.Name}"].Value = Convert.ChangeType(prop.GetValue(instance, null), type);
                        else
                            cmd.Parameters[$"_{p.Name}"].Value = DBNull.Value;
                    }
                }
            }

            if (isOutPutPramam)
            {
                cmd.Parameters.Add("_ProcessingResult", MySqlDbType.VarChar, 100).Direction = ParameterDirection.Output;
            }
        }

        public void BindParametersWithValue(object instance, List<PropertyInfo> properties, MySqlCommand cmd, bool isOutPutPramam = false)
        {
            foreach (PropertyInfo p in properties)
            {
                var param = _helperService.GetDataType(p.PropertyType);
                var prop = instance.GetType().GetProperty(p.Name);
                if (prop != null && param != null)
                {
                    var type = Type.GetType(param.TypeQualifiedName);
                    var value = prop.GetValue(instance, null);
                    if (value != null)
                        cmd.Parameters.Add($"_{p.Name}", param.DbType).Value = Convert.ChangeType(value, type);
                    else
                        cmd.Parameters.AddWithValue($"_{p.Name}", DBNull.Value);
                }
                else
                {
                    cmd.Parameters.AddWithValue($"_{p.Name}", DBNull.Value);
                }
            }

            if (isOutPutPramam)
            {
                cmd.Parameters.Add("_ProcessingResult", MySqlDbType.VarChar, 100).Direction = ParameterDirection.Output;
            }
        }

        public void MultiRowParametersWithValue(object instance, List<PropertyInfo> properties, MySqlCommand cmd, string affectedValue)
        {
            foreach (PropertyInfo p in properties)
            {
                var param = _helperService.GetDataType(p.PropertyType);
                var prop = instance.GetType().GetProperty(p.Name);
                if (prop != null && param != null)
                {
                    var type = Type.GetType(param.TypeQualifiedName);
                    var value = prop.GetValue(instance, null);
                    if (value != null)
                    {
                        if (value.ToString() == ApplicationConstants.LastInsertedKey)
                            cmd.Parameters.Add($"_{p.Name}", param.DbType).Value = Convert.ChangeType(affectedValue, type);
                        else
                            cmd.Parameters.Add($"_{p.Name}", param.DbType).Value = Convert.ChangeType(value, type);
                    }
                    else
                        cmd.Parameters.AddWithValue($"_{p.Name}", DBNull.Value);
                }
                else
                {
                    cmd.Parameters.AddWithValue($"_{p.Name}", DBNull.Value);
                }
            }
        }

        public static object DefaultOrValue(Type PropertyType, object originalValue)
        {
            object value = DBNull.Value;
            if (PropertyType == typeof(DateTime) || PropertyType == typeof(Nullable<DateTime>))
            {
                var datetime = Convert.ToDateTime(originalValue);
                if (datetime.Year == 1)
                {
                    value = Convert.ToDateTime("1/1/1976").ToString("yyyy-MM-dd HH:mm:ss.fff");
                }
                else
                {
                    value = Convert.ToDateTime(originalValue).ToString("yyyy-MM-dd HH:mm:ss.fff");
                }
            }
            else if (PropertyType == typeof(int) || PropertyType == typeof(Nullable<int>))
            {
                value = Convert.ToInt32(originalValue);
            }
            else if (PropertyType == typeof(long) || PropertyType == typeof(Nullable<long>))
            {

            }
            else if (PropertyType == typeof(double) || PropertyType == typeof(Nullable<double>))
            {

            }
            else if (PropertyType == typeof(decimal) || PropertyType == typeof(Nullable<decimal>))
            {

            }
            else if (PropertyType == typeof(bool) || PropertyType == typeof(Nullable<bool>))
            {

            }
            else if (PropertyType == typeof(short) || PropertyType == typeof(Nullable<short>))
            {

            }
            else
            {

            }

            return value;
        }
    }
}
