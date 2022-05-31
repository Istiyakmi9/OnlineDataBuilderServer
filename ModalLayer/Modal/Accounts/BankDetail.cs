using System;

namespace ModalLayer.Modal.Accounts
{
    public class BankDetail
    {
        public long BankAccountId { set; get; }
        public long OrganizationId { set; get; }
        public long UserId { set; get; }
        public string AccountNumber { set; get; }
        public string BankName { set; get; }
        public string Branch { set; get; }
        public string BranchCode { set; get; }
        public string IFSCCode { set; get; }
        public bool IsUser { set; get; }
        public string PANNumber { get; set; }
        public string GSTINNumber { get; set; }
        public string TradeLiecenceNumber { get; set; }
        public DateTime OpeningDate { set; get; } = DateTime.Now;
    }
}
