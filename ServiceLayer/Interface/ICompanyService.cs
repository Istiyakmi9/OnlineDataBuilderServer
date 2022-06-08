using ModalLayer.Modal;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceLayer.Interface
{
    public interface ICompanyService
    {
        List<Organization> GetAllCompany();
    }
}
