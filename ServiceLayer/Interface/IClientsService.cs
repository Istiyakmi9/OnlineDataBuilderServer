using ModalLayer.Modal;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.Interface
{
    public interface IClientsService
    {
        Task<string> RegisterClient(Organization client);
        Organization GetClientDetailById(long ClientId, bool IsActive);
        List<Organization> GetClients(FilterModel filterModel);
    }
}
