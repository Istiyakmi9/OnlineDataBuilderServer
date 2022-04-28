using System;
using System.Data;
using BottomhalfCore.Services.Code;

namespace BottomhalfCore.Services.Interface
{
    public interface ICacheManager
    {
        bool IsEmpty();
        DataTable Get(Table key);
        void Add(Table key, DataTable value);
        void Clean();
        void ReLoad(Func<DataSet> procFunc);
    }
}