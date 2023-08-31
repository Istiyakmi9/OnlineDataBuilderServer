using System.Collections.Generic;

namespace ModalLayer.Modal.HtmlTemplateModel
{
    public class BillingTemplateModel
    {
        public List<string> ToAddress { set; get; }
        public int Year { get; set; }
        public string Month { get; set; }
        public string DeveloperName { get; set; }
        public List<FileDetail> FileDetails { set; get; }
    }
}
