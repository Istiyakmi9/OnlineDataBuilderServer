using System;
using System.Collections.Generic;
using System.Text;

namespace ModalLayer.Modal
{
    public class Project
    {
        public long ProjectId {set; get;}
        public string ProjectName {set; get;}
        public string ProjectDescription {set; get;}
        public long ProjectManagerId {set; get;}
        public string TeamMemberIds { set; get; }
        public DateTime? ProjectStartedOn { set; get; }
        public DateTime? ProjectEndedOn { set; get; }
        public long ArchitectId { set; get; }
        public bool IsClientProject { set; get; }
        public long ClientId { set; get; }
        public string HomePageUrl { set; get; }
        public string PageIndexDetail { set; get; }
        public string KeywordDetail { set; get; }
        public string DocumentationDetail { set; get; }
    }
}
