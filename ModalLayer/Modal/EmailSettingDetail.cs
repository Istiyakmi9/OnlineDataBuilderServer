using System;

namespace ModalLayer.Modal
{
    public class EmailSettingDetail
    {
        public int EmailSettingDetailId { set; get; }
        public int CompanyId { set; get; }
        public string EmailAddress { set; get; }
        public string EmailName { set; get; }
        public string EmailHost { set; get; }
        public int PortNo { set; get; }
        public bool EnableSsl { set; get; }
        public string DeliveryMothod { set; get; }
        public bool UserDefaultCredentials { set; get; }
        public string Credentials { set; get; }
        public long UpdatedBy { set; get; }
        public DateTime UpdatedOn { set; get; }
    }
}
