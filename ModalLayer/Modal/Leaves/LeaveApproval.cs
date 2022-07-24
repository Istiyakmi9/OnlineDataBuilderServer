using System;
using System.Collections.Generic;
using System.Text;

namespace ModalLayer.Modal.Leaves
{
    public class LeaveApproval
    {
        public int LeaveApprovalId { get; set; }
        public int LeavePlanId { get; set; }
        public bool IsLeaveRequiredApproval { get; set; }
        public int ApprovalLevels { get; set; }
        public bool IsReportingManageIsDefaultForAction { get; set; }
        public bool CanHigherRankPersonsIsAvailForAction { get; set; }
        public bool IsReqioredAllLevelApproval { get; set; }
        public int NoOfApprovalForConfirmation { get; set; }
    }
}
