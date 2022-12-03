using ModalLayer.Modal.Accounts;
using System.Collections.Generic;

namespace ModalLayer.Modal
{
    public class EmployeeCalculation
    {
        public decimal CTC { set; get; }
        public long EmployeeId { set; get; }
        public Employee employee { set; get; }
        public EmployeeDeclaration employeeDeclaration { set; get; }
        public EmployeeSalaryDetail employeeSalaryDetail { set; get; }
        public EmployeeEmailMobileCheck emailMobileCheck { set; get; }
        public List<SalaryComponents> salaryComponents { set; get; }
        public SalaryGroup salaryGroup { set; get; }
        public CompanySetting companySetting { set; get; }
    }
}
