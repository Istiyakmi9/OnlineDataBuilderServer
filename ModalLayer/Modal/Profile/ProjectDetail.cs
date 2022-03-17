using System;
using System.Collections.Generic;
using System.Text;

namespace ModalLayer.Modal.Profile
{
    public class ProjectDetail
    {
        public string ProjectTitle { get; set; }
        public string ProjectTag { get; set; }
        public int ProjectWorkingYear { get; set; }
        public int ProjectWorkingMonth { get; set; }
        public int ProjectWorkedYear { get; set; }
        public int ProjectWorkedMonth { get; set; }
        public string IsProjectInCompleted { get; set; }
        public string IsProjectInProgress { get; set; }
        public string ClientName { get; set; }
        public string ProjectDetails { get; set; }
    }
}
