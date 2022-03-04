using System;
using System.Collections.Generic;
using System.Text;

namespace ModalLayer.Modal
{
    public class PdfModal : FileDetail
    {
        public string header { get; set; }
        public DateTime billingMonth { get; set; }
        public string billNo { get; set; }
        public long billId { get; set; }
        public DateTime dateOfBilling { get; set; }
        public float cGST { get; set; }
        public float sGST { get; set; }
        public float iGST { get; set; }
        public float cGstAmount { get; set; }
        public float sGstAmount { get; set; }
        public float iGstAmount { get; set; }
        public int workingDay { get; set; }
        public double packageAmount { get; set; }
        public double grandTotalAmount { get; set; }
        public string senderCompanyName { get; set; }
        public string receiverFirstAddress { get; set; }
        public string receiverCompanyName { get; set; }
        public int senderClientId { get; set; }
        public string developerName { set; get; }
        public string receiverSecondAddress { get; set; }
        public string receiverThirdAddress { set; get; }
        public string senderFirstAddress { get; set; }
        public float daysAbsent { set; get; }
        public string senderSecondAddress { get; set; }
        public string senderPrimaryContactNo { get; set; }
        public string senderEmail { get; set; }
        public string senderGSTNo { get; set; }
        public string receiverGSTNo { get; set; }
        public string receiverPrimaryContactNo { get; set; }
        public string receiverEmail { get; set; }
        public int UpdateSeqNo { set; get; }
    }
}
