using System;
using System.Collections.Generic;
using System.Text;

namespace ModalLayer.Modal.Leaves
{
    public class LeavePlan
    {
        public string LeavePlanId { set; get; }
        public string LeaveGroupId { set; get; }
        public string PlanName { set; get; }
        public string PlanDescription { set; get; }
        public string MaxLeaveLimit { set; get; }
    }
}
