﻿using ModalLayer.Modal.Accounts;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceLayer.Interface
{
    public interface IComponentsCalculationService
    {
        decimal StandardDeductionComponent(EmployeeDeclaration employeeDeclaration);
        void ProfessionalTaxComponent(EmployeeDeclaration employeeDeclaration, SalaryGroup salaryGroup);
        void EmployerProvidentFund(EmployeeDeclaration employeeDeclaration, SalaryGroup salaryGroup);
        decimal OneAndHalfLakhsComponent(EmployeeDeclaration employeeDeclaration);
        void OldTaxRegimeCalculation(EmployeeDeclaration employeeDeclaration, decimal grossIncome);
        void NewTaxRegimeCalculation(EmployeeDeclaration employeeDeclaration, decimal grossIncome);
        void HRAComponent(EmployeeDeclaration employeeDeclaration, List<CalculatedSalaryBreakupDetail> calculatedSalaryBreakupDetails);
        void BuildTaxDetail(long EmployeeId, EmployeeDeclaration employeeDeclaration, EmployeeSalaryDetail salaryBreakup);
    }
}