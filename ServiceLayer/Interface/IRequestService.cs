using System.Data;

namespace ServiceLayer.Interface
{
    public interface IRequestService
    {
        DataSet FetchPendingRequests(int employeeId);
    }
}
