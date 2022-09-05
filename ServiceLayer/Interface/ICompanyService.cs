using Microsoft.AspNetCore.Http;
using ModalLayer.Modal.Accounts;
using System.Collections.Generic;

namespace ServiceLayer.Interface
{
    public interface ICompanyService
    {
        List<OrganizationDetail> GetAllCompany();
        List<OrganizationDetail> AddCompanyGroup(OrganizationDetail companyGroup);
        List<OrganizationDetail> UpdateCompanyGroup(OrganizationDetail companyGroup, int companyId);
        dynamic GetCompanyById(int companyId);
        dynamic GetOrganizationDetailService();
        OrganizationDetail InsertUpdateOrganizationDetailService(OrganizationDetail companyInfo, IFormFileCollection fileCollection);
        BankDetail InsertUpdateCompanyAccounts(BankDetail bankDetail);
        BankDetail GetCompanyBankDetail(int OrganizationId, int CompanyId);

    }
}
