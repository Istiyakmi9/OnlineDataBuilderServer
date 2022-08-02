using System;
using System.Collections.Concurrent;
using System.Data;

namespace ServiceLayer.Caching
{
    public enum Table
    {
        Employee = 1,
        Client =2,
        MappedOrganization = 3,
        EmployeeRoles = 4,
        Companies = 5
    }

    public class Cache
    {
        private readonly ConcurrentDictionary<Table, DataTable> _table;
        private static readonly object _lock = new object();
        private static Cache _cache = null;

        private Cache()
        {
            _table = new ConcurrentDictionary<Table, DataTable>();
        }

        public bool IsEmpty()
        {
            return _table.Count == 0 ? true : false;
        }

        public static Cache GetInstance()
        {
            if (_cache == null)
            {
                lock (_lock)
                {
                    if (_cache == null)
                    {
                        _cache = new Cache();
                    }
                }
            }
            return _cache;
        }

        public DataTable Get(Table key)
        {
            DataTable value = null;
            if (_table.ContainsKey(key))
            {
                _table.TryGetValue(key, out value);
                return value;
            }
            return value;
        }

        public void Add(Table key, DataTable value)
        {
            DataTable oldValue = null;
            if (_table.ContainsKey(key))
            {
                _table.TryGetValue(key, out oldValue);
                _table.TryUpdate(key, value, oldValue);
            }
            else
            {
                _table.TryAdd(key, value);
            }
        }

        public void Clean()
        {
            _table.Clear();
        }

        public void ReLoad(Func<DataTable> procFunc, Table tableName)
        {
            DataTable result = procFunc.Invoke();
            if (result != null)
            {
                switch (tableName)
                {
                    case Table.MappedOrganization:
                        _table.TryRemove(Table.MappedOrganization, out DataTable oldOrganizationSet);
                        break;
                    case Table.Employee:
                        _table.TryRemove(Table.Employee, out DataTable oldSet);
                        _table.TryAdd(Table.Employee, result);
                        break;
                }
            }
        }
    }
}