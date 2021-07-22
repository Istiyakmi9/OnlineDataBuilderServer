using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;
using System.Linq;

namespace BottomhalfCore.Services.Code
{
    public static class Converter
    {
        public static List<T> ToList<T>(this DataTable table) where T : new()
        {
            IList<PropertyInfo> properties = typeof(T).GetProperties().ToList();
            List<T> result = new List<T>();

            foreach (var row in table.Rows)
            {
                var item = CreateItemFromRow<T>((DataRow)row, properties);
                result.Add(item);
            }

            return result;
        }

        private static T CreateItemFromRow<T>(DataRow row, IList<PropertyInfo> properties) where T : new()
        {
            T item = new T();
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
    }
}
