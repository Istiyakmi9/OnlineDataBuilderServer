using System;
using System.Collections.Generic;

namespace ModalLayer.Modal.HtmlTemplateModel
{
    public class OfferLetterTemplateModel
    {
        public List<string> ToAddress { set; get; }
        public string RequestType { set; get; }
        public string ActionType { set; get; }
        public DateTime FromDate { set; get; }
        public DateTime ToDate { set; get; }
        public string DeveloperName { set; get; }
        public string Message { set; get; }
        public int DayCount { set; get; }
        public List<FileDetail> FileDetails { set; get; }
    }
}
