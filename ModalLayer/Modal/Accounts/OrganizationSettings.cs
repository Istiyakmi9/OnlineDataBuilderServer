using System;

namespace ModalLayer.Modal.Accounts
{
    public class OrganizationSettings
    {
        public int OrganizationId { set; get; }
        public int CompanyId { set; get; }
        public string OrganizationName { set; get; }
        public string CompanyName { set; get; }
        public string CompanyDetail { get; set; }
        public int SectorType { set; get; }
        public string Country { set; get; }
        public string State { set; get; }
        public string City { set; get; }
        public string FullAddress { set; get; }
        public string PANNumber { set; get; }
        public string TradeLicenseNumber { set; get; }
        public string GSTINNumber { set; get; }
        public string LegalDocumentPath { set; get; }
        public string LegalEntity { set; get; }
        public string LegalNameOfCompany { set; get; }
        public string TypeOfBusiness { set; get; }
        public DateTime InCorporationDate { set; get; }
        public DateTime UpdatedOn { set; get; }
        public string MobileNo { get; set; }
        public string Email { get; set; }
        public string PrimaryPhoneNo { get; set; }
        public string SecondaryPhoneNo { get; set; }
        public string Fax { get; set; }
        public string FirstAddress { get; set; }
        public string SecondAddress { get; set; }
        public string ThirdAddress { get; set; }
        public string FourthAddress { get; set; }
        public int Pincode { get; set; }
        public long FileId { set; get; }
    }
}
