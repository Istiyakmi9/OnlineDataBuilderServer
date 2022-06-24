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
        public List<SalaryComponents> SalaryComponentItems { set; get; }
        public List<SalaryComponents> ExemptionDeclaration { set; get; }
        public List<SalaryComponents> OtherDeclaration { set; get; }
        public List<SalaryComponents> TaxSavingAlloance { set; get; }
        public List<Files> FileDetails { set; get; }
        public EmployeeSalaryDetail SalaryDetail { set; get; }
        public Dictionary<string, List<string>> Sections { set; get; }
    }
}
