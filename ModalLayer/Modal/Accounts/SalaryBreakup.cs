using System;
using System.Collections.Generic;
using System.Text;

namespace ModalLayer.Modal.Accounts
{
    public class SalaryBreakup
    {
        public long EmployeeId { get; set; }
        public decimal CTC { get; set; }
        public decimal GrossIncome { get; set; }
        public decimal NetSalary { get; set; }
        public int GroupId { get; set; }
        public string CompleteSalaryDetail { get; set; }
        public long AdminId { get; set; }
    }

    public class  CompleteSalaryBreakup
    {
        public decimal BasicMonthly { get; set; }
        public decimal BasicAnnually { get; set; }
        public decimal ConveyanceMonthly { get; set; }
        public decimal ConveyanceAnnually { get; set; }
        public decimal HRAMonthly { get; set; }
        public decimal HRAAnnually { get; set; }
        public decimal MedicalMonthly { get; set; }
        public decimal MedicalAnnually { get; set; }
        public decimal CarRunningMonthly { get; set; }
        public decimal CarRunningAnnually { get; set; }
        public decimal InternetMonthly { get; set; }
        public decimal InternetAnnually { get; set; }
        public decimal TravelMonthly { get; set; }
        public decimal TravelAnnually { get; set; }
        public decimal ShiftMonthly { get; set; }
        public decimal ShiftAnnually { get; set; }
        public decimal SpecialMonthly { get; set; }
        public decimal SpecialAnnually { get; set; }
        public decimal GrossMonthly { get; set; }
        public decimal GrossAnnually { get; set; }
        public decimal InsuranceMonthly { get; set; }
        public decimal InsuranceAnnually { get; set; }
        public decimal PFMonthly { get; set; }
        public decimal PFAnnually { get; set; }
        public decimal GratuityMonthly { get; set; }
        public decimal GratuityAnnually { get; set; }
        public decimal CTCMonthly { get; set; }
        public decimal CTCAnnually { get; set; }
    }
}
