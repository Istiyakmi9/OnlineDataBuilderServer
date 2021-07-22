using System.Collections.Generic;
using System.Data;

namespace BottomhalfCore.Services.Interface
{
    public interface IAutoMapper<T>
    {
        List<T> MapTo<T>(DataTable table);
        dynamic AutoMapToObjectList<T>(DataTable table);
        dynamic AutoMapToObject<T>(DataTable table);
    }
}
