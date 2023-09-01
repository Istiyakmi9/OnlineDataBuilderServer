using System;
using System.Collections.Generic;

namespace ModalLayer.Modal.HtmlTemplateModel
{
    public class AttendanceRequestModal
    {
        public List<string> ToAddress { set; get; }
        public string RequestType { set; get; }
        public string ActionType { set; get; }
        public DateTime FromDate { set; get; }
        public DateTime ToDate { set; get; }
        public string DeveloperName { set; get; }
        public string ManagerName { set; get; }
        public int DayCount { set; get; }
        public string CompanyName { get; set; }
        public string Message { set; get; }
    }
}
