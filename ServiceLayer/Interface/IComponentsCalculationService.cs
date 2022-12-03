using ModalLayer.Modal;
using ModalLayer.Modal.Accounts;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceLayer.Interface
{
    public interface IComponentsCalculationService
    {
        decimal StandardDeductionComponent(EmployeeCalculation empCal);
        void ProfessionalTaxComponent(EmployeeDeclaration employeeDeclaration, SalaryGroup salaryGroup);
        void EmployerProvidentFund(EmployeeDeclaration employeeDeclaration, SalaryGroup salaryGroup);
        void TaxRegimeCalculation(EmployeeDeclaration employeeDeclaration, decimal grossIncome, List<TaxRegime> taxRegimeSlabs);
        void HRAComponent(EmployeeDeclaration employeeDeclaration, List<CalculatedSalaryBreakupDetail> calculatedSalaryBreakupDetails);
        void BuildTaxDetail(long EmployeeId, EmployeeDeclaration employeeDeclaration, EmployeeSalaryDetail salaryBreakup);
        decimal OneAndHalfLakhsComponent(EmployeeDeclaration employeeDeclaration);
        decimal OtherDeclarationComponent(EmployeeDeclaration employeeDeclaration);
        decimal TaxSavingComponent(EmployeeDeclaration employeeDeclaration);
        decimal HousePropertyComponent(EmployeeDeclaration employeeDeclaration);
    }
}
