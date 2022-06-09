using Microsoft.AspNetCore.Http;
using ModalLayer.Modal;
using ModalLayer.Modal.Accounts;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceLayer.Interface
{
    public interface ICompanyService
    {
        List<OrganizationSettings> GetAllCompany();
        List<OrganizationSettings> AddCompanyGroup(OrganizationSettings companyGroup);
        OrganizationSettings GetCompanyById(int companyId);
        OrganizationSettings UpdateCompanyDetails(OrganizationSettings companyInfo, IFormFileCollection fileCollection);

    }
}
