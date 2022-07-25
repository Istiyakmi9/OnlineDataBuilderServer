using BottomhalfCore.DatabaseLayer.Common.Code;
using ModalLayer.Modal;
using ModalLayer.Modal.Leaves;
using Newtonsoft.Json;
using ServiceLayer.Interface;

namespace ServiceLayer.Code
{
    public class ManageLeavePlanService : IManageLeavePlanService
    {
        private readonly IDb _db;

        public ManageLeavePlanService(IDb db)
        {
            _db = db;
        }

        public LeavePlanConfiguration UpdateLeaveAccrual(int leavePlanTypeId, LeaveAccrual leaveAccrual)
        {
            _db.Execute<LeaveAccrual>("", leaveAccrual, true);
            return this.GetLeaveConfigurationDetail(leavePlanTypeId);
        }

        public LeavePlanConfiguration GetLeaveConfigurationDetail(int leavePlanTypeId)
        {
            LeavePlanConfiguration leavePlanConfiguration = new LeavePlanConfiguration();
            LeavePlanType leavePlanType = _db.Get<LeavePlanType>("sp_leave_plans_type_getbyId", new { LeavePlanTypeId = leavePlanTypeId });

            if (leavePlanType != null && !string.IsNullOrEmpty(leavePlanType.PlanConfigurationDetail))
                leavePlanConfiguration = JsonConvert.DeserializeObject<LeavePlanConfiguration>(leavePlanType.PlanConfigurationDetail);

            return leavePlanConfiguration;
        }

        public LeavePlanConfiguration UpdateLeaveDetail(int leavePlanTypeId, LeaveDetail leaveDetail)
        {
            LeavePlanConfiguration leavePlanConfiguration = new LeavePlanConfiguration();
            LeavePlanType leavePlanType = _db.Get<LeavePlanType>("sp_leave_plans_type_getbyId", new { LeavePlanTypeId = leavePlanTypeId });

            if (leavePlanType == null)
                throw new HiringBellException("Invalid plan type id. No record found.");

            if (leavePlanType != null && !string.IsNullOrEmpty(leavePlanType.PlanConfigurationDetail))
            {
                leavePlanConfiguration = JsonConvert.DeserializeObject<LeavePlanConfiguration>(leavePlanType.PlanConfigurationDetail);
                leavePlanConfiguration.leaveDetail = leaveDetail;
            }

            var result = _db.Execute<LeaveDetail>("sp_leave_detail_insupd", new
            {
                leaveDetail.LeaveDetailId,
                leaveDetail.LeavePlanTypeId,
                leaveDetail.IsLeaveDaysLimit,
                leaveDetail.LeaveLimit,
                leaveDetail.CanApplyExtraLeave,
                leaveDetail.ExtraLeaveLimit,
                leaveDetail.IsNoLeaveAfterDate,
                leaveDetail.LeaveNotAllocatedIfJoinAfter,
                leaveDetail.CanManagerAwardCausalLeave,
                leaveDetail.CanCompoffCreditedByManager,
                leaveDetail.CanCompoffAllocatedAutomatically,
                LeavePlanConfiguration = JsonConvert.SerializeObject(leavePlanConfiguration)
            }, true);

            if (!ApplicationConstants.IsExecuted(result))
                throw new HiringBellException("Fail to insert or update leave plan detail.");

            return leavePlanConfiguration;
        }
    }
}
