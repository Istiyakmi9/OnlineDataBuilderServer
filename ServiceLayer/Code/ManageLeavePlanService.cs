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
                leaveDetail.CanCompoffAllocatedAutomatically,
                leaveDetail.CanCompoffCreditedByManager,
                LeavePlanConfiguration = JsonConvert.SerializeObject(leavePlanConfiguration)
            }, true);

            if (!ApplicationConstants.IsExecuted(result))
                throw new HiringBellException("Fail to insert or update leave plan detail.");

            return leavePlanConfiguration;
        }

        public LeavePlanConfiguration UpdateLeaveAccrualService(int leavePlanTypeId, LeaveAccrual leaveAccrual)
        {
            LeavePlanConfiguration leavePlanConfiguration = new LeavePlanConfiguration();
            LeavePlanType leavePlanType = _db.Get<LeavePlanType>("sp_leave_plans_type_getbyId", new { LeavePlanTypeId = leavePlanTypeId });

            if (leavePlanType == null)
                throw new HiringBellException("Invalid plan type id. No record found.");

            if (leavePlanType != null && !string.IsNullOrEmpty(leavePlanType.PlanConfigurationDetail))
            {
                leavePlanConfiguration = JsonConvert.DeserializeObject<LeavePlanConfiguration>(leavePlanType.PlanConfigurationDetail);
                leavePlanConfiguration.leaveAccrual = leaveAccrual;
            }

            var result = _db.Execute<LeaveAccrual>("sp_leave_accrual_InsUpdate", new
            {
                leaveAccrual.LeaveAccrualId,
                leaveAccrual.LeavePlanTypeId,
                leaveAccrual.CanApplyEntireLeave,
                leaveAccrual.IsLeaveAccruedPatternAvail,
                leaveAccrual.LeaveDistributionSequence,
                leaveAccrual.LeaveDistributionAppliedFrom,
                leaveAccrual.IsAllowLeavesForJoinigMonth,
                leaveAccrual.IsAllowLeavesProbationPeriod,
                leaveAccrual.BreakMonthLeaveAllocationId,
                leaveAccrual.IsNoLeaveOnProbationPeriod,
                leaveAccrual.IsVaryOnProbationOrExprience,
                leaveAccrual.IsImpactedOnWorkDaysEveryMonth,
                leaveAccrual.WeekOffAsAbsentIfAttendaceLessThen,
                leaveAccrual.HolidayAsAbsentIfAttendaceLessThen,
                leaveAccrual.CanApplyForFutureDate,
                leaveAccrual.ExtraLeaveBeyondAccruedBalance,
                leaveAccrual.NoOfDaysForExtraLeave,
                leaveAccrual.AllowOnlyIfAccrueBalanceIsAlleast,
                leaveAccrual.NotAllowIfAlreadyOnLeaveMoreThan,
                leaveAccrual.RoundOffLeaveBalance,
                leaveAccrual.ToNearestHalfDay,
                leaveAccrual.ToNearestFullDay,
                leaveAccrual.ToNextAvailableHalfDay,
                leaveAccrual.ToNextAvailableFullDay,
                leaveAccrual.ToPreviousHalfDay,
                leaveAccrual.DoesLeaveExpireAfterSomeTime,
                leaveAccrual.AfterHowManyDays,
                LeavePlanConfiguration = JsonConvert.SerializeObject(leavePlanConfiguration)
            }, true);

            if (!ApplicationConstants.IsExecuted(result))
                throw new HiringBellException("Fail to insert or update leave plan detail.");

            return leavePlanConfiguration;
        }
        
    }
}
