using System.Data;

namespace ServiceLayer.Interface
{
    public interface ICommonService
    {
        DataTable LoadEmployeeData();
    }
}
