﻿using Microsoft.AspNetCore.Http;
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
        Task<EmployeeDeclaration> HousingPropertyDeclarationService(long EmployeeDeclarationId, HousingDeclartion DeclarationDetail, IFormFileCollection FileCollection, List<Files> fileDetail);
        EmployeeDeclaration GetEmployeeDeclarationDetail(long EmployeeId, bool reCalculateFlag = false);
        EmployeeSalaryDetail CalculateSalaryDetail(long EmployeeId, EmployeeDeclaration employeeDeclaration, decimal CTC = 0, bool reCalculateFlag = false);
        Task<string> UpdateTaxDetailsService(long EmployeeId, int PresentMonth, int PresentYear);
        string SwitchEmployeeTaxRegimeService(EmployeeDeclaration employeeDeclaration);
        string DeleteDeclarationValueService(long EmployeeId, string ComponentId);
        Task<string> DeleteDeclarationFileService(long DeclarationId, int FileId, string ComponentId);

    }
}
