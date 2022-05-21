using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace BottomhalfCore.Services.Code
{
    public static class Converter
    {
        public static decimal TwoDecimalValue(decimal num)
        {
            string strNum = num.ToString();
            if (strNum.IndexOf(".") == -1)
                return num;
            else
                return (decimal)Math.Floor(num * 100) / 100;
        }

        public static List<T> ToList<T>(this DataTable table) where T : new()
        {
            IList<PropertyInfo> availableProperties = typeof(T).GetProperties().ToList();
            IList<PropertyInfo> properties = new List<PropertyInfo>();
            List<T> result = new List<T>();

            DataColumnCollection columns = table.Columns;
            string name = null;
            int index = 0;
            while (index < availableProperties.Count)
            {
                name = availableProperties.ElementAt(index).Name;
                if (columns.Contains(name))
                    properties.Add(availableProperties[index]);
                index++;
            }

            foreach (var row in table.Rows)
            {
                var item = CreateItemFromRow<T>((DataRow)row, properties);
                result.Add(item);
            }

            return result;
        }

        public static T ToType<T>(this DataTable table) where T : new()
        {
            IList<PropertyInfo> availableProperties = typeof(T).GetProperties().ToList();
            IList<PropertyInfo> properties = new List<PropertyInfo>();
            T result = new T();

            DataColumnCollection columns = table.Columns;
            string name = null;
            int index = 0;
            while (index < availableProperties.Count)
            {
                name = availableProperties.ElementAt(index).Name;
                if (columns.Contains(name))
                    properties.Add(availableProperties[index]);
                index++;
            }

            var row = table.Rows[0];
            result = CreateItemFromRow<T>((DataRow)row, properties);
            return result;
        }

        private static T CreateItemFromRow<T>(DataRow row, IList<PropertyInfo> properties) where T : new()
        {
            T item = new T();
            DateTime? now = null;
            foreach (var property in properties)
            {
                try
                {
                    if (property.PropertyType == typeof(System.DayOfWeek))
                    {
                        DayOfWeek day = (DayOfWeek)Enum.Parse(typeof(DayOfWeek), row[property.Name].ToString());
                        property.SetValue(item, day, null);
                    }
                    else
                    {
                        if (row[property.Name] == DBNull.Value)
                            property.SetValue(item, null, null);
                        else
                        {
                            switch (property.PropertyType.FullName)
                            {
                                case "System.Boolean":
                                    if (row[property.Name].ToString() == "1")
                                        property.SetValue(item, true, null);
                                    else
                                        property.SetValue(item, false, null);
                                    break;
                                case "System.Guid":
                                    property.SetValue(item, Guid.Parse(row[property.Name].ToString()), null);
                                    break;
                                case "System.DateTime":
                                    property.SetValue(item, Convert.ToDateTime(row[property.Name].ToString()), null);
                                    break;
                                case "System.Int32":
                                    property.SetValue(item, Convert.ToInt32(row[property.Name].ToString()), null);
                                    break;
                                default:
                                    property.SetValue(item, row[property.Name], null);
                                    break;
                            }
                        }
                    }
                }
                finally { }
            }
            return item;
        }

        public static DataSet ToDataSet<T>(IList<T> list)
        {
            Type elementType = typeof(T);
            DataSet ds = new DataSet();
            DataTable t = new DataTable();
            ds.Tables.Add(t);

            //add a column to table for each public property on T
            foreach (var propInfo in elementType.GetProperties())
            {
                Type ColType = Nullable.GetUnderlyingType(propInfo.PropertyType) ?? propInfo.PropertyType;
                t.Columns.Add(propInfo.Name, ColType);
            }

            //go through each property on T and add each value to the table
            foreach (T item in list)
            {
                DataRow row = t.NewRow();

                foreach (var propInfo in elementType.GetProperties())
                {
                    row[propInfo.Name] = propInfo.GetValue(item, null) ?? DBNull.Value;
                }

                t.Rows.Add(row);
            }

            return ds;
        }

        public static DataTable ToDataTable<T>(IEnumerable<T> Linqlist)
        {
            DataTable dt = new DataTable();
            PropertyInfo[] columns = null;

            if (Linqlist == null) return dt;

            foreach (T Record in Linqlist)
            {

                if (columns == null)
                {
                    columns = ((Type)Record.GetType()).GetProperties();
                    foreach (PropertyInfo GetProperty in columns)
                    {
                        Type colType = GetProperty.PropertyType;

                        if ((colType.IsGenericType) && (colType.GetGenericTypeDefinition()
                               == typeof(Nullable<>)))
                        {
                            colType = colType.GetGenericArguments()[0];
                        }

                        dt.Columns.Add(new DataColumn(GetProperty.Name, colType));
                    }
                }

                DataRow dr = dt.NewRow();

                foreach (PropertyInfo pinfo in columns)
                {
                    dr[pinfo.Name] = pinfo.GetValue(Record, null) == null ? DBNull.Value : pinfo.GetValue
                           (Record, null);
                }

                dt.Rows.Add(dr);
            }
            return dt;
        }
    }
}
