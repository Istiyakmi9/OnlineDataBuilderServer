using Microsoft.AspNetCore.Http;
using ModalLayer.Modal.Accounts;
using System.Collections.Generic;

namespace ServiceLayer.Interface
{
    public interface ICompanyService
    {
        List<OrganizationSettings> GetAllCompany();
        List<OrganizationSettings> AddCompanyGroup(OrganizationSettings companyGroup);
        List<OrganizationSettings> UpdateCompanyGroup(OrganizationSettings companyGroup, int companyId);
        dynamic GetCompanyById(int companyId);
        OrganizationSettings UpdateCompanyDetails(OrganizationSettings companyInfo, IFormFileCollection fileCollection);
        BankDetail InsertUpdateCompanyAccounts(BankDetail bankDetail);
        BankDetail GetCompanyBankDetail(int OrganizationId, int CompanyId);

    }
}
