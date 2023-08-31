using System;
using System.Collections.Generic;
using System.Text;

namespace ModalLayer.Modal.HtmlTemplateModel
{
    public class LeaveApprovalTemplateModel
    {
        public List<string> ToAddress { set; get; }
        public string RequestType { set; get; }
        public string ActionType { set; get; }
        public DateTime FromDate { set; get; }
        public DateTime ToDate { set; get; }
        public string DeveloperName { set; get; }
        public string ManagerName { set; get; }
        public int DayCount { set; get; }
    }
}
