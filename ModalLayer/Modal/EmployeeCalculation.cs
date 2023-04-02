using ModalLayer.Modal.Accounts;
using System;
using System.Collections.Generic;

namespace ModalLayer.Modal
{
    public class EmployeeCalculation
    {
        public decimal CTC { set; get; }
        public decimal TaxableCTC { set; get; } // this is equals to the Gross i.e. CTC - (Tax Exampted components)
        public long EmployeeId { set; get; }
        public DateTime Doj { set; get; }
        public bool IsFirstYearDeclaration { set; get; }
        public DateTime PayrollStartDate { set; get; }
        public Employee employee { set; get; }
        public decimal expectedAnnualGrossIncome { set; get; }
        public EmployeeDeclaration employeeDeclaration { set; get; }
        public EmployeeSalaryDetail employeeSalaryDetail { set; get; }
        public EmployeeEmailMobileCheck emailMobileCheck { set; get; }
        public List<SalaryComponents> salaryComponents { set; get; }
        public SalaryGroup salaryGroup { set; get; }
        public CompanySetting companySetting { set; get; }
        public List<SurChargeSlab> surchargeSlabs { set; get; }
        public List<PTaxSlab> ptaxSlab { set; get; }
    }

    public class PayrollCommonData
    {
        public List<SalaryComponents> salaryComponents { set; get; }
        public List<SalaryGroup> salaryGroups { set; get; }
        public List<Payroll> payrolls { set; get; }
        public List<SurChargeSlab> surchargeSlabs { set; get; }
        public List<PTaxSlab> ptaxSlab { set; get; }
        public TimeZoneInfo timeZone { set; get; }
        public DateTime presentDate { set; get; }
        public DateTime utcPresentDate { set; get; }
    }
}
