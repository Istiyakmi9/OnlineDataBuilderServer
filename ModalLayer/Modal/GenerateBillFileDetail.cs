using System.Collections.Generic;

namespace ModalLayer.Modal
{
    public class GenerateBillFileDetail
    {
        public long ClientId { get; set; }
        public long SenderId { get; set; }
        public long EmployeeId { set; get; }
        public string FileExtension { get; set; }
        public long FileId { get; set; }
        public int UserTypeId { get; set; }
        public int ForMonth { get; set; }
        public string MonthName { get; set; }
        public int ForYear { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string EmailTitle { get; set; } = string.Empty;
        public string EmailSubject { get; set; } = string.Empty;
        public List<string> Emails { get; set; }
        public EmailTemplate EmailTemplateDetail { set; get; }
    }
}
