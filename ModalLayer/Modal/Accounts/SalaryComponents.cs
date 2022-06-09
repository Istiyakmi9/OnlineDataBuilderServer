namespace ModalLayer.Modal.Accounts
{
    public class SalaryComponents : SalaryCommon
    {
        public string ComponentId { set; get; }
        public string TaxExempt { get; set; }
        public int SubComponentTypeId { get; set; }
        public decimal PercentageValue { set; get; }
        public decimal EmployeeContribution { set; get; }
        public decimal EmployerContribution { set; get; }
        public bool IncludeInPayslip { set; get; }
        public bool IsOpted { set; get; }
        public bool IsActive { set; get; }
    }

    public class SalaryCommon : CreationInfo
    {
        public bool CalculateInPercentage { set; get; }
        public string ComponentDescription { set; get; }
        public bool IsComponentEnabled { set; get; }
        public decimal MaxLimit { set; get; }
        public string Formula { set; get; }
    }
}
