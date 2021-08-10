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
        public int noOfWorkingDay { get; set; }
        public int packageAmount { get; set; }
        public double grandTotalAmount { get; set; }
        public string companyName { get; set; }
        public string companyAddress { get; set; }
        public string senderName { get; set; }
        public string senderAddress { get; set; }
    }
}
