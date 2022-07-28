using System.Collections.Generic;

namespace ModalLayer.Modal.Leaves
{
    public class LeaveApproval
    {
        public int LeaveApprovalId { get; set; }
        public int LeavePlanTypeId { get; set; }
        public bool IsLeaveRequiredApproval { get; set; }
        public int ApprovalLevels { get; set; }
        public List<ApprovalRoleDetail> ApprovalChain { set; get; }
        public bool IsRequiredAllLevelApproval { get; set; }
        public bool CanHigherRankPersonsIsAvailForAction { get; set; }
        public bool IsPauseForApprovalNotification { get; set; }
        public bool IsReportingManageIsDefaultForAction { get; set; }
    }

    public class ApprovalRoleDetail
    {
        public int ApprovalRoleTypeId { set; get; }
        public bool IsSkipToNextLevel { set; get; }
        public decimal SkipToNextLevelAfterDays { set; get; }
    }
}
