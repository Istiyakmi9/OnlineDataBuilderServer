using System;

namespace ModalLayer.Modal
{
    public class Employee : AssignedClients
    {
        public string FirstName { set; get; }
        public string LastName { set; get; }
        public string Mobile { set; get; }
        public int CompanyId { set; get; }
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
        public string ProfessionalDetail_Json { get; set; }
        public string ClientJson { set; get; }
        public long EmpProfDetailUid { set; get; }
        public decimal ExperienceInYear { set; get; }
        public int DesignationId { set; get; }
        public int AccessLevelId { set; get; }
        public int UserTypeId { set; get; } = 2;
    }
}
