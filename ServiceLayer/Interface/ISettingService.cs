using Microsoft.AspNetCore.Http;
using ModalLayer.Modal.Accounts;
using System.Collections.Generic;

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
        List<SalaryComponents> ActivateCurrentComponentService(List<SalaryComponents> components);
        string UpdateGroupSalaryComponentDetailService(string componentId, int groupId,SalaryComponents component);
        List<SalaryComponents> EnableSalaryComponentDetailService(string componentId, SalaryComponents component);
        List<SalaryComponents> FetchComponentDetailByIdService(int componentTypeId);
        List<SalaryComponents> FetchActiveComponentService();
    }
}
