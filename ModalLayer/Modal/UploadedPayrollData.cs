﻿using System;
using System.Collections.Generic;

namespace ModalLayer.Modal
{
    public class UploadedPayrollData
    {
        public UploadedPayrollData()
        {
            Investments = new Dictionary<string, decimal>();
        }

        public long EmployeeId { set; get; }
        public string EmployeeName { set; get; }
        public DateTime DOJ { set; get; }
        public string PAN { set; get; }
        public string Address { set; get; }
        public decimal CTC { set; get; }
        public string Email { set; get; }
        public string Mobile { set; get; }
        public bool Status { set; get; }
        public string Regime { set; get; }
        public Dictionary<string, decimal> Investments { set; get; }
    }
}
