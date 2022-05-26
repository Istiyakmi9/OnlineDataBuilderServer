using System;
using System.Collections.Generic;
using System.Text;

namespace ModalLayer.Modal.Accounts
{
    public class SalaryComponents : CreationInfo
    {
        public string ComponentId { set; get; }
        public string ComponentDescription { set; get; }
        public bool CalculateInPercentage { set; get; }
        public decimal PercentageValue { set; get; }
        public decimal Amount { set; get; }
        public bool EmployeeContribution { set; get; }
        public bool EmployerContribution { set; get; }
        public bool IncludeInPayslip { set; get; }
        public bool IsDeductions { set; get; }
        public bool IsOpted { set; get; }
    }
}
