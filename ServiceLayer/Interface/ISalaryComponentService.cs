using ModalLayer.Modal.Accounts;
using System.Collections.Generic;

namespace ServiceLayer.Interface
{
    public interface ISalaryComponentService
    {
        SalaryComponents GetSalaryComponentByIdService();
        List<SalaryComponents> GetSalaryComponentsDetailService();
    }
}
