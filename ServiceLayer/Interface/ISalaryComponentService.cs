using ModalLayer.Modal.Accounts;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceLayer.Interface
{
    public interface ISalaryComponentService
    {
        SalaryComponents GetSalaryComponentByIdService();
        List<SalaryComponents> GetSalaryComponentsDetailService();
    }
}
