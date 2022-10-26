using BottomhalfCore.DatabaseLayer.MySql.Code;
using ModalLayer.Modal;
using System.Data;

namespace ServiceLayer.Caching
{
    public class CacheManager : ICacheManager
    {
        private static readonly object _lock = new object();
        private static CacheManager _cacheManager;

        private readonly Cache _cache = null;
        private readonly string _connectionString;
        private CacheManager(string connectionString)
        {
            _cache = Cache.GetInstance();
            _connectionString = connectionString;
            this.LoadApplicationData();
        }

        public static CacheManager GetInstance(string connectionString)
        {
            if (_cacheManager == null)
            {
                lock (_lock)
                {
                    if (_cacheManager == null)
                    {
                        _cacheManager = new CacheManager(connectionString);
                    }
                }
            }
            return _cacheManager;
        }

        public bool IsEmpty()
        {
            return _cache.IsEmpty();
        }

        public void Add(CacheTable key, DataTable value)
        {
            _cache.Add(key, value);
        }

        public void Clean()
        {
            _cache.Clean();
        }

        public DataTable Get(CacheTable key)
        {
            if (_cache.IsEmpty())
            {
                this.LoadApplicationData();
                if (_cache.IsEmpty())
                    throw new HiringBellException("Encounter some internal issue. Please login again or contact to your admin.");
            }
            return _cache.Get(key);
        }

        public void ReLoad(CacheTable tableName, DataTable table)
        {
            _cache.ReLoad(tableName, table);
        }

        public void LoadApplicationData(bool isReload = false)
        {
            if (IsEmpty() || isReload)
            {
                var _db = new Db(_connectionString);
                DataSet Result = _db.FetchDataSet("SP_ApplicationData_Get");
                if (Result.Tables.Count == 3)
                {
                    _cache.Clean();
                    _cache.Add(CacheTable.AccessLevel, Result.Tables[0]);
                    _cache.Add(CacheTable.LeavePlan, Result.Tables[1]);
                    _cache.Add(CacheTable.Company, Result.Tables[2]);
                }
            }
        }
    }
}