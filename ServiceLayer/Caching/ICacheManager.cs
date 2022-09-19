using System;
using System.Data;

namespace ServiceLayer.Caching
{
    public interface ICacheManager
    {
        bool IsEmpty();
        DataTable Get(CacheTable key);
        void Add(CacheTable key, DataTable value);
        void Clean();
        void ReLoad(CacheTable tableName, DataTable table);
        void LoadApplicationData(bool isReload = false);
    }
}