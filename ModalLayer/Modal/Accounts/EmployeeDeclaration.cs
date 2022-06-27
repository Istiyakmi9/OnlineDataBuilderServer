using System.Collections.Generic;

namespace ModalLayer.Modal.Accounts
{
    public class EmployeeDeclaration
    {
        public long EmployeeDeclarationId { set; get; }
        public long EmployeeId { set; get; }
        public string DocumentPath { set; get; }
        public string DeclarationDetail { set; get; }
        public string ComponentId { set; get; }
        public decimal DeclaredValue { set; get; }
        public string Email { set; get; }
        public decimal TotalAmount { set; get; }
        public decimal TaxNeedToPay { set; get; }
        public decimal TaxPaid { set; get; }
        public List<SalaryComponents> SalaryComponentItems { set; get; }
        public List<SalaryComponents> ExemptionDeclaration { set; get; }
        public List<SalaryComponents> OtherDeclaration { set; get; }
        public List<SalaryComponents> TaxSavingAlloance { set; get; }
        public List<Files> FileDetails { set; get; }
        public EmployeeSalaryDetail SalaryDetail { set; get; }
        public Dictionary<string, List<string>> Sections { set; get; }
        public List<DeclarationReport> Declarations { set; get; } = new List<DeclarationReport>();
    }

    public class DeclarationReport
    {
        public string DeclarationName { set; get; }
        public List<string> Declarations { set; get; }
        public decimal TotalAmountDeclared { set; get; }
        public int NumberOfProofSubmitted { set; get; }
        public decimal RejectedAmount { set; get; }
        public decimal AcceptedAmount { set; get; }
    }
}
