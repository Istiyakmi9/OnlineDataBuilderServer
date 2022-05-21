using System;
using System.Data;

namespace ServiceLayer.Caching
{
    public interface ICacheManager
    {
        bool IsEmpty();
        DataTable Get(Table key);
        void Add(Table key, DataTable value);
        void Clean();
        void ReLoad(Func<DataTable> procFunc, Table tableName);
        DataSet LoadApplicationData();
    }
}