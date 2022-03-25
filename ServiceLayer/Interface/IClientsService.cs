using ModalLayer.Modal;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace ServiceLayer.Interface
{
    public interface IClientsService
    {
        Task<string> RegisterClient(Organization client);
        Organization GetClientDetailById(long ClientId, bool IsActive);
        List<Organization> GetClients(FilterModel filterModel);
        DataSet DeactivateClient(Employee employee);
    }
}
