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
        OrganizationSettings GetCompanyById(int companyId);
        OrganizationSettings UpdateCompanyDetails(OrganizationSettings companyInfo, IFormFileCollection fileCollection);

    }
}
