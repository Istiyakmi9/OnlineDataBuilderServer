namespace ModalLayer.Modal
{
    public class CompanyNotification:CreationInfo
    {
        public long NotificationId {get; set;}
        public string Topic {get; set;}
        public int CompanyId {get; set;}
        public string BriefDetail {get; set;}
        public int DepartmentId {get; set;}
        public string CompleteDetail {get; set;}
        public int Total {get; set;}
        public int Index { get; set; }
        public long AdminId { get; set; }
    }
}
