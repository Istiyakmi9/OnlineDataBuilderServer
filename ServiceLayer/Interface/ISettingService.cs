using ModalLayer.Modal.Accounts;
using System.Collections.Generic;

namespace ServiceLayer.Interface
{
    public interface ISettingService
    {
        string AddUpdateComponentService(SalaryComponents salaryComponents);
        List<SalaryComponents> GetSalaryComponentService();
    }
}
