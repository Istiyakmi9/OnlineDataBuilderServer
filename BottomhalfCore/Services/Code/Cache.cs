using System;
using System.Collections.Concurrent;
using System.Data;

namespace BottomhalfCore.Services.Code
{
    public enum Table
    {
        User = 1,
        Employee = 2,
        Client = 3
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

        public void ReLoad(Func<DataSet> procFunc)
        {
            DataSet result = procFunc.Invoke();
            if (result != null && result.Tables.Count == 5)
            {
                _table.Clear();
                _table.TryAdd(Table.User, result.Tables[0]);
                _table.TryAdd(Table.Employee, result.Tables[3]);
                _table.TryAdd(Table.User, result.Tables[4]);
            }
        }
    }
}