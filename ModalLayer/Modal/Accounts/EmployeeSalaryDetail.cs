﻿namespace ModalLayer.Modal.Accounts
{
    public class EmployeeSalaryDetail
    {
        public long EmployeeId { set; get; }
        public decimal CTC { set; get; }
        public decimal GrossIncome { set; get; }
        public decimal NetSalary { set; get; }
        public string CompleteSalaryDetail { set; get; }
        public int GroupId { set; get; }
    }
}
