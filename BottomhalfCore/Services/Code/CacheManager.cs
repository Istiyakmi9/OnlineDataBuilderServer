using System;
using System.Data;
using BottomhalfCore.Services.Interface;

namespace BottomhalfCore.Services.Code
{
    public class CacheManager : ICacheManager
    {
        private readonly Cache _cache;
        public CacheManager()
        {
            _cache = Cache.GetInstance();
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
            return _cache.Get(key);
        }

        public void ReLoad(Func<DataSet> procFunc)
        {
            _cache.ReLoad(procFunc);
        }
    }
}