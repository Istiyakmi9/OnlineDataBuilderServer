namespace ModalLayer.Modal.Accounts
{
    public class SalaryComponents : CreationInfo
    {
        public string ComponentId { set; get; }
        public string ComponentDescription { set; get; }
        public bool CalculateInPercentage { set; get; }
        public decimal PercentageValue { set; get; }
        public decimal Amount { set; get; }
        public decimal EmployeeContribution { set; get; }
        public decimal EmployerContribution { set; get; }
        public bool IncludeInPayslip { set; get; }
        public bool IsDeductions { set; get; }
        public bool IsOpted { set; get; }
        public bool IsActive { set; get; }
    }
}
