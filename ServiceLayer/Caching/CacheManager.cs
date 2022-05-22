using BottomhalfCore.DatabaseLayer.MySql.Code;
using Microsoft.Extensions.Configuration;
using ModalLayer.Modal;
using System;
using System.Data;

namespace ServiceLayer.Caching
{
    public class CacheManager : ICacheManager
    {
        private readonly Cache _cache;
        private readonly string _connectionString;
        public CacheManager(IConfiguration configuration)
        {
            _cache = Cache.GetInstance();
            _connectionString = configuration.GetConnectionString("OnlinedatabuilderDb");
        }

        public bool IsEmpty()
        {
            return _cache.IsEmpty();
        }

        public void Add(Table key, DataTable value)
        {
            _cache.Add(key, value);
        }

        public void Clean()
        {
            _cache.Clean();
        }

        public DataTable Get(Table key)
        {
            if (_cache.IsEmpty())
            {
                this.LoadApplicationData();
                if (_cache.IsEmpty())
                    throw new HiringBellException("Encounter some internal issue. Please login again or contact to your admin.");
            }
            return _cache.Get(key);
        }

        public void ReLoad(Func<DataTable> procFunc, Table tableName)
        {
            _cache.ReLoad(procFunc, tableName);
        }

        public DataSet LoadApplicationData()
        {
            var _db = new Db(_connectionString);
            var Result = _db.GetDataset("SP_ApplicationData_Get", null);
            if (Result.Tables.Count == 3)
            {
                Result.Tables[0].TableName = "clients";
                Result.Tables[1].TableName = "employees";
                Result.Tables[2].TableName = "allocatedClients";

                _cache.Clean();
                _cache.Add(Table.Client, Result.Tables[0]);
                _cache.Add(Table.Employee, Result.Tables[1]);
                _cache.Add(Table.MappedOrganization, Result.Tables[2]);
            }
            return Result;
        }
    }
}