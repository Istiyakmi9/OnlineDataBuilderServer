using System;

namespace ModalLayer
{
    public class EmailRequestModal
    {
        public string DeveloperName { set; get; }
        public string RequestType { set; get; }
        public string ActionType { set; get; }
        public string Message { set; get; }
        public DateTime FromDate { set; get; }
        public DateTime ToDate { set; get; }
        public string ManagerName { set; get; }
        public double TotalNumberOfDays { set; get; }
        public int TemplateId { set; get; }
    }
}
