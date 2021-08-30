using ModalLayer.Modal;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.Interface
{
    public interface IClientsService
    {
        Task<string> RegisterClient(Client client);
        Client GetClientDetailById(long ClientId, bool IsActive);
        List<Client> GetClients(FilterModel filterModel);
    }
}
