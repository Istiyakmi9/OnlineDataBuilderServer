using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BottomhalfCore.DatabaseLayer.MySql.Code
{
    public class HelperService
    {
        private static HelperService _instance;
        private static readonly object _lock = new object();
        public static List<DataType> DbTypes;

        private HelperService()
        {
            InitDataType();
        }

        private void InitDataType()
        {
            HelperService.DbTypes = new List<DataType>();
            DbTypes.Add(new DataType { TypeQualifiedName = typeof(DateTime).AssemblyQualifiedName, DbType = MySqlDbType.DateTime });
            DbTypes.Add(new DataType { TypeQualifiedName = typeof(System.String).AssemblyQualifiedName, DbType = MySqlDbType.VarChar });
            DbTypes.Add(new DataType { TypeQualifiedName = typeof(System.Int16).AssemblyQualifiedName, DbType = MySqlDbType.Int16 });
            DbTypes.Add(new DataType { TypeQualifiedName = typeof(System.Int32).AssemblyQualifiedName, DbType = MySqlDbType.Int32 });
            DbTypes.Add(new DataType { TypeQualifiedName = typeof(System.Double).AssemblyQualifiedName, DbType = MySqlDbType.Float });
            DbTypes.Add(new DataType { TypeQualifiedName = typeof(System.Int64).AssemblyQualifiedName, DbType = MySqlDbType.Int64 });
            DbTypes.Add(new DataType { TypeQualifiedName = typeof(System.Char).AssemblyQualifiedName, DbType = MySqlDbType.VarChar });
            DbTypes.Add(new DataType { TypeQualifiedName = typeof(System.Decimal).AssemblyQualifiedName, DbType = MySqlDbType.Decimal });
            DbTypes.Add(new DataType { TypeQualifiedName = typeof(System.Boolean).AssemblyQualifiedName, DbType = MySqlDbType.Bit });
            DbTypes.Add(new DataType { TypeQualifiedName = typeof(System.Single).AssemblyQualifiedName, DbType = MySqlDbType.Decimal });
        }

        public DataType GetDataType(Type type)
        {
            Type paramType = type;
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                paramType = Nullable.GetUnderlyingType(type);
            return HelperService.DbTypes.FirstOrDefault(x => x.TypeQualifiedName == paramType.AssemblyQualifiedName);
        }

        public class DataType
        {
            public string TypeQualifiedName { set; get; }
            public MySqlDbType DbType { set; get; }
        }

        public static HelperService Instance()
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new HelperService();
                    }

                }
            }
            return _instance;
        }
    }
}
