using System;
using System.Collections.Generic;

namespace ModalLayer.Modal.HtmlTemplateModel
{
    public class TimesheetSubmittedTemplateModel
    {
        public List<string> ToAddress { set; get; }
        public string RequestType { set; get; }
        public string ActionType { set; get; }
        public DateTime FromDate { set; get; }
        public DateTime ToDate { set; get; }
        public string DeveloperName { set; get; }
        public string Message { set; get; }
        public int DayCount { set; get; }
    }
}
