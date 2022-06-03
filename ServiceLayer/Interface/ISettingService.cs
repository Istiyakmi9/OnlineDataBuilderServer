using ModalLayer.Modal.Accounts;
using System;
using System.Collections.Generic;
using System.Data;

namespace ServiceLayer.Interface
{
    public interface ISettingService
    {
        string AddUpdateComponentService(SalaryComponents salaryComponents);
        dynamic GetSalaryComponentService();
        OrganizationSettings InsertUpdateCompanyDetailService(OrganizationSettings organizationSettings);
        BankDetail UpdateCompanyAccountsService(BankDetail bankDetail);
        string PfEsiSetting(SalaryComponents PfSetting, SalaryComponents EsiSetting, PfEsiSetting PfesiSetting);
        List<OrganizationSettings> GetOrganizationInfo();
        BankDetail GetOrganizationBankDetailInfoService(int organizationId);
        Payroll InsertUpdatePayrollSetting(Payroll payroll);
        string InsertUpdateSalaryStructure(List<SalaryStructure> salaryStructure);
    }
}
