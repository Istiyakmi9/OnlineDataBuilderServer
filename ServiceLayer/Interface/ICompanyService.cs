using Microsoft.AspNetCore.Http;
using ModalLayer.Modal;
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
        List<BankDetail> InsertUpdateCompanyAccounts(BankDetail bankDetail);
        List<BankDetail> GetCompanyBankDetail(FilterModel filterModel);
        OrganizationDetail InsertUpdateCompanyDetailService(OrganizationDetail companyInfo, IFormFileCollection fileCollection);
    }
}
