using System.Collections.Generic;
using System.Data;

namespace BottomhalfCore.Services.Interface
{
    public interface IConvertToDataSet<T>
    {
        DataSet ToDataSet<T>(IList<T> list);
    }
}
