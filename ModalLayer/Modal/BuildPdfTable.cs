using System;
using System.Collections.Generic;
using System.Text;

namespace ModalLayer.Modal
{
    public class FileLocationDetail
    {
        public string RootPath { set; get; }
        public string Location { set; get; }
        public string AppLocation { set; get; }
        public string DocumentFolder { set; get; }
        public string UserFolder { set; get; }
        public string BillFolder { set; get; }
        public List<string> HtmlTemplaePath { set; get; }
        public string User { set; get; }
        public string BillsPath { set; get; }
        public string StaffingBillTemplate { set; get; }
        public string PaysliplTemplate { set; get; }
        public string StaffingBillPdfTemplate { set; get; }
        public string resumeTemplate { set; get; }
        public List<string> resumePath { set; get; }
        public string LogoPath { set; get; }
        public string CompanyFiles { set; get; } = "CompanyFiles";
        public string ConnectionString { set; get; }
    }
}
