using Microsoft.AspNetCore.Http;
using ModalLayer.Modal;
using ModalLayer.Modal.Accounts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServiceLayer.Interface
{
    public interface IDeclarationService
    {
        EmployeeDeclaration GetDeclarationByEmployee(long EmployeeId);
        EmployeeDeclaration GetDeclarationById(long EmployeeDeclarationId);
        Task<EmployeeDeclaration> UpdateDeclarationDetail(long EmployeeDeclarationId, EmployeeDeclaration employeeDeclaration, IFormFileCollection FileCollection, List<Files> fileDetail);
        EmployeeDeclaration HousingPropertyDeclarationService(long EmployeeDeclarationId, HousingDeclartion DeclarationDetail, IFormFileCollection FileCollection, List<Files> fileDetail);
        EmployeeDeclaration GetEmployeeDeclarationDetailById(long EmployeeId);
        EmployeeSalaryDetail CalculateSalaryDetail(long EmployeeId, EmployeeDeclaration employeeDeclaration, decimal CTC = 0);
    }
}
