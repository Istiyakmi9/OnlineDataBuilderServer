using System.Data;

namespace ServiceLayer.Interface
{
    public interface ICommonService
    {
        DataSet LoadApplicationData();
        DataTable LoadEmployeeData();
    }
}
