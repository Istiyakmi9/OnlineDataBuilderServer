using Microsoft.AspNetCore.Http;
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
        OrganizationSettings InsertUpdateCompanyDetailService(OrganizationSettings organizationSettings, IFormFileCollection fileCollection);
        BankDetail UpdateCompanyAccountsService(BankDetail bankDetail);
        string PfEsiSetting(SalaryComponents PfSetting, SalaryComponents EsiSetting, PfEsiSetting PfesiSetting);
        List<OrganizationSettings> GetOrganizationInfo();
        BankDetail GetOrganizationBankDetailInfoService(int organizationId);
        string InsertUpdatePayrollSetting(Payroll payroll);
        Payroll GetPayrollSetting(int companyId);
        string InsertUpdateSalaryStructure(List<SalaryStructure> salaryStructure);
        string UpdateSalaryComponentDetailService(string componentId, SalaryComponents component);
    }
}
