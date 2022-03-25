using BottomhalfCore.DatabaseLayer.Common.Code;
using BottomhalfCore.Services.Code;
using ModalLayer.Modal;
using ServiceLayer.Interface;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace ServiceLayer.Code
{
    public class ClientsService : IClientsService
    {
        private readonly IDb _db;
        private readonly CommonFilterService _commonFilterService;
        private readonly CurrentSession _currentSession;
        public ClientsService(IDb db, CommonFilterService commonFilterService, CurrentSession currentSession)
        {
            _db = db;
            _commonFilterService = commonFilterService;
            _currentSession = currentSession;
        }
        public List<Organization> GetClients(FilterModel filterModel)
        {
            List<Organization> client = _commonFilterService.GetResult<Organization>(filterModel, "SP_Clients_Get");
            return client;
        }

        public Organization GetClientDetailById(long ClientId, bool IsActive)
        {
            Organization client = default;
            DbParam[] param = new DbParam[]
            {
                new DbParam(ClientId, typeof(long), "_ClientId"),
                new DbParam(IsActive, typeof(bool), "_IsActive")
            };

            var resultSet = _db.GetDataset("SP_Client_ById", param);
            if (resultSet.Tables.Count > 0 && resultSet.Tables[0].Rows.Count > 0)
            {
                var emps = Converter.ToList<Organization>(resultSet.Tables[0]);
                if (emps != null && emps.Count > 0)
                    client = emps[0];
            }
            return client;
        }

        public async Task<string> RegisterClient(Organization client)
        {
            return await Task.Run(() =>
            {
                string status = "expired";
                DbParam[] param = new DbParam[]
                {
                    new DbParam(client.ClientId, typeof(long), "_ClientId"),
                    new DbParam(client.ClientName, typeof(string), "_ClientName"),
                    new DbParam(client.PrimaryPhoneNo, typeof(string), "_PrimaryPhoneNo"),
                    new DbParam(client.SecondaryPhoneNo, typeof(string), "_SecondaryPhoneNo"),
                    new DbParam(client.MobileNo, typeof(string), "_MobileNo"),
                    new DbParam(client.Email, typeof(string), "_Email"),
                    new DbParam(client.Fax, typeof(string), "_Fax"),
                    new DbParam(client.GSTNO, typeof(string), "_GSTNO"),
                    new DbParam(client.PanNo, typeof(string), "_PanNo"),
                    new DbParam(client.Pincode, typeof(int), "_Pincode"),
                    new DbParam(client.Country, typeof(string), "_Country"),
                    new DbParam(client.State, typeof(string), "_State"),
                    new DbParam(client.City, typeof(string), "_City"),
                    new DbParam(client.FirstAddress, typeof(string), "_FirstAddress"),
                    new DbParam(client.SecondAddress, typeof(string), "_SecondAddress"),
                    new DbParam(client.ThirdAddress, typeof(string), "_ThirdAddress"),
                    new DbParam(client.ForthAddress, typeof(string), "_ForthAddress"),
                    new DbParam(client.IFSC, typeof(string), "_IFSC"),
                    new DbParam(client.AccountNo, typeof(string), "_AccountNo"),
                    new DbParam(client.BankName, typeof(string), "_BankName"),
                    new DbParam(client.BranchName, typeof(string), "_BranchName"),
                    new DbParam(_currentSession.CurrentUserDetail.UserId, typeof(long), "_AdminId")
                };

                status = _db.ExecuteNonQuery("SP_Client_IntUpd", param, true);
                return status;
            });
        }

        public DataSet DeactivateClient(Employee employee)
        {
            if (employee == null || employee.EmployeeUid <= 0)
            {
                throw new HiringBellException("Invalid client detail submitted.");
            }

            DbParam[] param = new DbParam[]
            {
                new DbParam(employee.EmployeeMappedClientsUid, typeof(long), "_ClientMappedId"),
                new DbParam(employee.EmployeeUid, typeof(long), "_UserId")
            };

            var resultSet = _db.GetDataset("sp_deactivateOrganization_delandgetall", param);
            return resultSet;
        }
    }
}
