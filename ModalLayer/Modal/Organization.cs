using System;
using System.Collections.Generic;
using System.Text;

namespace ModalLayer.Modal
{
    public class Organization
    {
        public int Index { set; get; }
        public long Id { set; get; }
        public long ClientId { set; get; }
        public string ClientName { set; get; }
        public string MobileNo { set; get; }
        public string PrimaryPhoneNo { set; get; }
        public string SecondaryPhoneNo { set; get; }
        public string Email { set; get; }
        public string Fax { set; get; }
        public string FirstAddress { set; get; }
        public string SecondAddress { set; get; }
        public string ThirdAddress { set; get; }
        public string ForthAddress { set; get; }
        public int Pincode { set; get; }
        public string City { set; get; }
        public string State { set; get; }
        public string Country { set; get; }
        public string GSTNO { set; get; }
        public string AccountNo { set; get; }
        public string BankName { set; get; }
        public string BranchName { set; get; }
        public string IFSC { set; get; }
        public string PanNo { set; get; }
        public long AdminId { set; get; }
        public bool IsActive { set; get; }
        public int Total { get; set; }
    }
}
