using System;

namespace ModalLayer.Modal.Accounts
{
    public class OrganizationSettings
    {
        public int OrganizationId { set; get; }
        public string OrganizationName { set; get; }
        public int SectorType { set; get; }
        public string Contry { set; get; }
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
    }
}
