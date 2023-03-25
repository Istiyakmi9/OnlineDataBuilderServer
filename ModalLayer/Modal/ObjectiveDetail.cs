using System;

namespace ModalLayer.Modal
{
    public class ObjectiveDetail
    {
        public long ObjectiveId { get; set; }
        public string Objective {get; set;}
        public bool ObjSeeType {get; set;}  // true = manager can see, false = everyone see
        public bool IsIncludeReview {get; set;}
        public string Tag {get; set;}
        public int ProgressMeassureType {get; set;}
        public int StartValue {get; set;}
        public int TargetValue {get; set;}
        public int MetricUnits {get; set;}
        public string Description { get; set; }
        public int ProgressCalculatedAs {get; set;}
        public DateTime TimeFrameStart {get; set;}
        public DateTime TimeFrmaeEnd { get; set; }
        public string ObjectiveType { get; set; }
        public int Total { get; set; }
        public long AdminId { get; set; }
        public int CompanyId { get; set; }
    }
}
