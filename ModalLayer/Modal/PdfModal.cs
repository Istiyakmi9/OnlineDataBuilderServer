using System;
using System.Collections.Generic;
using System.Text;

namespace ModalLayer.Modal
{
    public class PdfModal
    {
        public string header { get; set; }
        public string billForMonth { get; set; }
        public string billNo { get; set; }
        public DateTime dateOfBilling { get; set; }
        public int cGST { get; set; }
        public int sGST { get; set; }
        public int iGST { get; set; }
        public float cGstAmount { get; set; }
        public float sGstAmount { get; set; }
        public float iGstAmount { get; set; }
        public int noOfWorkingDay { get; set; }
        public int packageAmount { get; set; }
        public double grandTotalAmount { get; set; }
        public string senderCompanyName { get; set; }
        public string receiverFirstAddress { get; set; }
        public string receiverCompanyName { get; set; }
        public string developerName { set; get; }
        public string receiverSecondAddress { get; set; }
        public string senderFirstAddress { get; set; }
        public string senderSecondAddress { get; set; }
        public string senderPrimaryContactNo { get; set; }
        public string senderEmail { get; set; }
        public string senderGSTNo { get; set; }
        public string receiverGSTNo { get; set; }
        public string receiverPrimaryContactNo { get; set; }
        public string receiverEmail { get; set; }

    }
}
