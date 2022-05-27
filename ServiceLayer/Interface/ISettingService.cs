using ModalLayer.Modal.Accounts;
using System;
using System.Collections.Generic;

namespace ServiceLayer.Interface
{
    public interface ISettingService
    {
        string AddUpdateComponentService(SalaryComponents salaryComponents);
        dynamic GetSalaryComponentService();
        string PfEsiSetting(SalaryComponents PfSetting, SalaryComponents EsiSetting, PfEsiSetting PfesiSetting);
    }
}
