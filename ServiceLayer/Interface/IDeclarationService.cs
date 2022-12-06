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
        Task<EmployeeDeclaration> HouseRentDeclarationService(long EmployeeDeclarationId, HousingDeclartion DeclarationDetail, IFormFileCollection FileCollection, List<Files> fileDetail);
        Task<EmployeeDeclaration> GetEmployeeDeclarationDetail(long EmployeeId, bool reCalculateFlag = false);
        Task<EmployeeSalaryDetail> CalculateSalaryDetail(long EmployeeId, EmployeeDeclaration employeeDeclaration, bool reCalculateFlag = false);
        Task<string> UpdateTaxDetailsService(long EmployeeId, int PresentMonth, int PresentYear);
        string SwitchEmployeeTaxRegimeService(EmployeeDeclaration employeeDeclaration);
        Task<EmployeeDeclaration> DeleteDeclarationValueService(long DeclarationId, string ComponentId);
        Task<EmployeeDeclaration> DeleteDeclaredHRAService(long DeclarationId);
        Task<EmployeeDeclaration> DeleteDeclarationFileService(long DeclarationId, int FileId, string ComponentId);
        Task<EmployeeSalaryDetail> CalculateSalaryNDeclaration(EmployeeCalculation empCal, bool reCalculateFlag);
    }
}
