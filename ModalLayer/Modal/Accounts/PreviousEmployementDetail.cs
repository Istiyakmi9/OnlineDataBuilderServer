using System.Collections.Generic;

namespace ModalLayer.Modal.Accounts
{
    public class PreviousEmployementDetail:CreationInfo
    {
        public long PreviousEmpDetailId { get; set; }
        public int EmployeeId { get; set; }
        public string Month { get; set; }
        public int MonthNumber { get; set; }
        public decimal Gross { get; set; }
        public decimal Basic { get; set; }
        public decimal HouseRent { get; set; }
        public decimal EmployeePR { get; set; }
        public decimal ESI { get; set; }
        public decimal LWF { get; set; }
        public decimal LWFEmp { get; set; }
        public decimal Professional { get; set; }
        public decimal IncomeTax { get; set; }
        public decimal OtherTax { get; set; }
        public decimal OtherTaxable { get; set; }
        public int Year { get; set; }
    }

}
