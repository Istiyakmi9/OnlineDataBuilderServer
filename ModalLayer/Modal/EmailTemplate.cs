using System;
using System.Collections.Generic;

namespace ModalLayer.Modal
{
    public class EmailTemplate
    {
        public int EmailTemplateId { set; get; }
        public string TemplateName { set; get; }
        public string SubjectLine { set; get; }
        public string Salutation { set; get; }
        public string EmailClosingStatement { set; get; }
        public string BodyContent { set; get; }
        public string EmailNote { set; get; }
        public string SignatureDetail { set; get; }
        public string ContactNo { set; get; }
        public string Fax { set; get; }
        public string Footer { set; get; }
        public string EmailTitle { get; set; } = string.Empty;
        public List<string> Emails { set; get; }
        public long AdminId { set; get; }
        public DateTime UpdatedOn { set; get; }
        public int Total { set; get; }
        public int Index { get; set; }
        public int CompanyId { get; set; }
        public int FileId { get; set; }
        public string FilePath { get; set; }
        public string Description { get; set; }
        public string EmailSubject { get; set; } = string.Empty;

    }
}