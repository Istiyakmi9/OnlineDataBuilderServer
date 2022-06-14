using ModalLayer.Modal.Accounts;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceLayer.Interface
{
    public interface IDeclarationService
    {
        EmployeeDeclaration GetDeclarationByEmployee(long EmployeeId);
        EmployeeDeclaration GetDeclarationById(long EmployeeDeclarationId);
        string UpdateDeclarationDetail(long EmployeeDeclarationId, EmployeeDeclaration employeeDeclaration);
    }
}
