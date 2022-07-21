using System;
using System.Collections.Generic;
using System.Text;

namespace ModalLayer.Modal.Leaves
{
    public class LeavePlan : CreationInfo
    {
        public int LeavePlanId { set; get; }
        public int LeaveGroupId { set; get; }
        public string LeavePlanCode { set; get; }
        public string PlanName { set; get; }
        public string PlanDescription { set; get; }
        public int MaxLeaveLimit { set; get; }
        public bool ShowDescription { set; get; }
        public bool IsPaidLeave { set; get; }
        public bool IsSickLeave { set; get; }
        public bool IsStatutoryLeave { set; get; }
        public bool IsRestrictOnGender { set; get; }
        public bool? IsMale { set; get; }
        public bool IsRestrictOnMaritalStatus { set; get; }
        public bool? IsMarried { set; get; }
        public string Reasons { set; get; }
    }
}
