﻿using System;

namespace ModalLayer.Modal
{
    public class Employee : AssignedClients
    {
        public string FirstName { set; get; }
        public string LastName { set; get; }
        public string Mobile { set; get; }
        public string Email { set; get; }
        public string SecondaryMobile { set; get; }
        public string FatherName { set; get; }
        public string MotherName { set; get; }
        public string SpouseName { set; get; }
        public string State { set; get; }
        public bool Gender { set; get; }
        public string City { set; get; }
        public int Pincode { set; get; }
        public string Address { set; get; }
        public string PANNo { set; get; }
        public string AadharNo { set; get; }
        public string AccountNumber { set; get; }
        public string BankName { set; get; }
        public string BranchName { set; get; }
        public string IFSCCode { set; get; }
        public string Domain { set; get; }
        public string Specification { set; get; }
        public float ExprienceInYear { set; get; }
        public string LastCompanyName { set; get; }
        public int Index { set; get; }
        public bool IsActive { set; get; }
        public int Total { set; get; }
        public DateTime? DOB { get; set; }
        public DateTime CreatedOn { get; set; }
    }

    public class AssignedClients
    {
        public long EmployeeUid { set; get; }
        public long EmployeeMappedClientsUid { get; set; }
        public long ClientUid { set; get; }
        public string ClientName { set; get; }
        public float ActualPackage { set; get; }
        public float FinalPackage { set; get; }
        public float TakeHomeByCandidate { set; get; }
        public bool IsPermanent { set; get; }
        public long FileId { set; get; }
        public int BillingHours { set; get; } = 0;
        public int WorkingDaysPerWeek { set; get; } = 0;
        public DateTime DOJ { set; get; }
        public DateTime? DOL { set; get; } = null;
    }
}
