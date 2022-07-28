using BottomhalfCore.DatabaseLayer.Common.Code;
using ModalLayer.Modal;
using ModalLayer.Modal.Leaves;
using Newtonsoft.Json;
using ServiceLayer.Interface;
using System;
using System.Collections.Generic;

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

            if (leaveDetail.IsLeaveDaysLimit == false)
                leaveDetail.LeaveLimit = 0;

            if (leaveDetail.IsNoLeaveAfterDate == false)
                leaveDetail.LeaveNotAllocatedIfJoinAfter = 0;

            if (leaveDetail.CanApplyExtraLeave == false)
                leaveDetail.ExtraLeaveLimit = 0;

            if (leavePlanType != null && !string.IsNullOrEmpty(leavePlanType.PlanConfigurationDetail))
            {
                leavePlanConfiguration = JsonConvert.DeserializeObject<LeavePlanConfiguration>(leavePlanType.PlanConfigurationDetail);
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
                leaveDetail.CanCompoffCreditedByManager
            }, true);

            if (string.IsNullOrEmpty(result))
                throw new HiringBellException("Fail to insert or update leave plan detail.");
            else
            {
                leaveDetail.LeaveDetailId = Convert.ToInt32(result);
                leavePlanConfiguration.leaveDetail = leaveDetail;
                this.UpdateLeavePlanConfigurationDetail(leavePlanTypeId, leavePlanConfiguration);
            }

            return leavePlanConfiguration;
        }

        public LeavePlanConfiguration UpdateLeaveAccrualService(int leavePlanTypeId, LeaveAccrual leaveAccrual)
        {
            LeavePlanConfiguration leavePlanConfiguration = new LeavePlanConfiguration();
            LeavePlanType leavePlanType = _db.Get<LeavePlanType>("sp_leave_plans_type_getbyId", new { LeavePlanTypeId = leavePlanTypeId });

            if (leavePlanType == null)
                throw new HiringBellException("Invalid plan type id. No record found.");


            if (leavePlanType != null && !string.IsNullOrEmpty(leavePlanType.PlanConfigurationDetail))
                leavePlanConfiguration = JsonConvert.DeserializeObject<LeavePlanConfiguration>(leavePlanType.PlanConfigurationDetail);

            ValidateAndPreFillValue(leaveAccrual);

            var result = _db.Execute<LeaveAccrual>("sp_leave_accrual_InsUpdate", new
            {
                leaveAccrual.LeaveAccrualId,
                leaveAccrual.LeavePlanTypeId,
                leaveAccrual.CanApplyEntireLeave,
                leaveAccrual.IsLeaveAccruedPatternAvail,
                JoiningMonthLeaveDistribution = JsonConvert.SerializeObject(leaveAccrual.JoiningMonthLeaveDistribution),
                ExitMonthLeaveDistribution = JsonConvert.SerializeObject(leaveAccrual.ExitMonthLeaveDistribution),
                LeaveDistributionSequence = leaveAccrual.LeaveDistributionSequence,
                leaveAccrual.LeaveDistributionAppliedFrom,
                leaveAccrual.IsLeavesProratedForJoinigMonth,
                leaveAccrual.IsLeavesProratedOnProbation,
                leaveAccrual.IsNotAllowProratedOnProbation,
                leaveAccrual.IsNoLeaveOnProbationPeriod,
                leaveAccrual.IsVaryOnProbationOrExprience,
                leaveAccrual.IsAccrualStartsAfterJoining,
                leaveAccrual.IsAccrualStartsAfterProbationEnds,
                leaveAccrual.AccrualDaysAfterJoining,
                leaveAccrual.AccrualDaysAfterProbationEnds,
                AccrualProrateDetail = JsonConvert.SerializeObject(leaveAccrual.AccrualProrateDetail),
                leaveAccrual.IsImpactedOnWorkDaysEveryMonth,
                leaveAccrual.WeekOffAsAbsentIfAttendaceLessThen,
                leaveAccrual.HolidayAsAbsentIfAttendaceLessThen,
                leaveAccrual.CanApplyForFutureDate,
                leaveAccrual.IsExtraLeaveBeyondAccruedBalance,
                leaveAccrual.IsNoExtraLeaveBeyondAccruedBalance,
                leaveAccrual.NoOfDaysForExtraLeave,
                leaveAccrual.IsAccrueIfHavingLeaveBalance,
                leaveAccrual.AllowOnlyIfAccrueBalanceIsAlleast,
                leaveAccrual.IsAccrueIfOnOtherLeave,
                leaveAccrual.NotAllowIfAlreadyOnLeaveMoreThan,
                leaveAccrual.RoundOffLeaveBalance,
                leaveAccrual.ToNearestHalfDay,
                leaveAccrual.ToNearestFullDay,
                leaveAccrual.ToNextAvailableHalfDay,
                leaveAccrual.ToNextAvailableFullDay,
                leaveAccrual.ToPreviousHalfDay,
                leaveAccrual.DoesLeaveExpireAfterSomeTime,
                leaveAccrual.AfterHowManyDays
            }, true);

            if (string.IsNullOrEmpty(result))
                throw new HiringBellException("Fail to insert or update leave plan detail.");
            else
            {
                leaveAccrual.LeaveAccrualId = Convert.ToInt32(result);
                leavePlanConfiguration.leaveAccrual = leaveAccrual;
                this.UpdateLeavePlanConfigurationDetail(leavePlanTypeId, leavePlanConfiguration);
            }

            return leavePlanConfiguration;
        }

        private void ValidateAndPreFillValue(LeaveAccrual leaveAccrual)
        {
            if (!leaveAccrual.IsNotAllowProratedOnProbation)
                leaveAccrual.ExitMonthLeaveDistribution = new List<AllocateTimeBreakup>();

            if (leaveAccrual.IsLeavesProratedForJoinigMonth)
                leaveAccrual.JoiningMonthLeaveDistribution = new List<AllocateTimeBreakup>();

            if (leaveAccrual.CanApplyEntireLeave == false)
            {
                leaveAccrual.LeaveDistributionSequence = "";
                leaveAccrual.LeaveDistributionAppliedFrom = 0;
            }

            if (leaveAccrual.IsVaryOnProbationOrExprience == false)
            {
                leaveAccrual.IsAccrualStartsAfterJoining = false;
                leaveAccrual.AccrualDaysAfterJoining = 0;
                leaveAccrual.IsAccrualStartsAfterProbationEnds = false;
                leaveAccrual.AccrualDaysAfterProbationEnds = 0;
                leaveAccrual.AccrualProrateDetail = new List<AccrualProrate>();
            }

            if (leaveAccrual.IsAccrualStartsAfterJoining == true)
            {
                leaveAccrual.IsAccrualStartsAfterProbationEnds = false;
                leaveAccrual.AccrualDaysAfterProbationEnds = 0;
            }

            if (leaveAccrual.IsAccrualStartsAfterProbationEnds == true)
            {
                leaveAccrual.IsAccrualStartsAfterJoining = false;
                leaveAccrual.AccrualDaysAfterJoining = 0;
            }

            if (leaveAccrual.IsNoExtraLeaveBeyondAccruedBalance == true)
            {
                leaveAccrual.IsExtraLeaveBeyondAccruedBalance = false;
                leaveAccrual.NoOfDaysForExtraLeave = 0;
            }

            if (leaveAccrual.IsAccrueIfHavingLeaveBalance == false)
                leaveAccrual.AllowOnlyIfAccrueBalanceIsAlleast = 0;

            if (leaveAccrual.IsAccrueIfOnOtherLeave == false)
                leaveAccrual.NotAllowIfAlreadyOnLeaveMoreThan = 0;

            if (leaveAccrual.DoesLeaveExpireAfterSomeTime == false)
                leaveAccrual.AfterHowManyDays = 0;
        }

        public void UpdateLeavePlanConfigurationDetail(int leavePlanTypeId, LeavePlanConfiguration leavePlanConfiguration)
        {
            var result = _db.Execute<LeaveAccrual>("sp_leave_plan_upd_configuration", new
            {
                LeavePlanTypeId = leavePlanTypeId,
                LeavePlanConfiguration = JsonConvert.SerializeObject(leavePlanConfiguration)
            }, true);

            if (!ApplicationConstants.IsExecuted(result))
                throw new HiringBellException("Fail to insert or update leave plan detail.");
        }

        public LeavePlanConfiguration UpdateApplyForLeaveService(int leavePlanTypeId, LeaveApplyDetail leaveApplyDetail)
        {
            LeavePlanConfiguration leavePlanConfiguration = new LeavePlanConfiguration();
            LeavePlanType leavePlanType = _db.Get<LeavePlanType>("sp_leave_plans_type_getbyId", new { LeavePlanTypeId = leavePlanTypeId });

            if (leavePlanType == null)
                throw new HiringBellException("Invalid plan type id. No record found.");

            if (leavePlanType != null && !string.IsNullOrEmpty(leavePlanType.PlanConfigurationDetail))
                leavePlanConfiguration = JsonConvert.DeserializeObject<LeavePlanConfiguration>(leavePlanType.PlanConfigurationDetail);

            var result = _db.Execute<LeaveApplyDetail>("sp_leave_apply_detail_InsUpdate", new
            {
                leaveApplyDetail.LeaveApplyDetailId,
                leaveApplyDetail.LeavePlanTypeId,
                leaveApplyDetail.IsAllowForHalfDay,
                leaveApplyDetail.EmployeeCanSeeAndApplyCurrentPlanLeave,
                leaveApplyDetail.ApplyPriorBeforeLeaveDate,
                leaveApplyDetail.BackDateLeaveApplyNotBeyondDays,
                leaveApplyDetail.RestrictBackDateLeaveApplyAfter,
                leaveApplyDetail.CurrentLeaveRequiredComments,
                leaveApplyDetail.ProofRequiredIfDaysExceeds,
                leaveApplyDetail.NoOfDaysExceeded,
                RuleForLeaveInNotice = JsonConvert.SerializeObject(leaveApplyDetail.RuleForLeaveInNotice)
            }, true);

            if (string.IsNullOrEmpty(result))
                throw new HiringBellException("Fail to insert or update apply for leave detail.");
            else
            {
                leaveApplyDetail.LeaveApplyDetailId = Convert.ToInt32(result);
                leavePlanConfiguration.leaveApplyDetail = leaveApplyDetail;
                this.UpdateLeavePlanConfigurationDetail(leavePlanTypeId, leavePlanConfiguration);
            }

            return leavePlanConfiguration;
        }


        public LeavePlanConfiguration UpdateLeaveRestrictionService(int leavePlanTypeId, LeavePlanRestriction leavePlanRestriction)
        {
            LeavePlanConfiguration leavePlanConfiguration = new LeavePlanConfiguration();
            LeavePlanType leavePlanType = _db.Get<LeavePlanType>("sp_leave_plans_type_getbyId", new { LeavePlanTypeId = leavePlanTypeId });

            if (leavePlanType == null)
                throw new HiringBellException("Invalid plan type id. No record found.");

            if (leavePlanType != null && !string.IsNullOrEmpty(leavePlanType.PlanConfigurationDetail))
                leavePlanConfiguration = JsonConvert.DeserializeObject<LeavePlanConfiguration>(leavePlanType.PlanConfigurationDetail);

            var result = _db.Execute<LeaveApplyDetail>("sp_leave_plan_restriction_insupd", leavePlanRestriction, true);

            if (string.IsNullOrEmpty(result))
                throw new HiringBellException("Fail to insert or update apply for leave detail.");
            else
            {
                leavePlanRestriction.LeavePlanRestrictionId = Convert.ToInt32(result);
                leavePlanConfiguration.leavePlanRestriction = leavePlanRestriction;
                this.UpdateLeavePlanConfigurationDetail(leavePlanTypeId, leavePlanConfiguration);
            }

            return leavePlanConfiguration;
        }

        public LeavePlanConfiguration UpdateHolidayNWeekOffPlanService(int leavePlanTypeId, LeaveHolidaysAndWeekoff leaveHolidaysAndWeekoff)
        {
            LeavePlanConfiguration leavePlanConfiguration = new LeavePlanConfiguration();
            LeavePlanType leavePlanType = _db.Get<LeavePlanType>("sp_leave_plans_type_getbyId", new { LeavePlanTypeId = leavePlanTypeId });

            if (leavePlanType == null)
                throw new HiringBellException("Invalid plan type id. No record found.");

            leaveHolidaysAndWeekoff.LeavePlanTypeId = leavePlanTypeId;
            if (leavePlanType != null && !string.IsNullOrEmpty(leavePlanType.PlanConfigurationDetail))
                leavePlanConfiguration = JsonConvert.DeserializeObject<LeavePlanConfiguration>(leavePlanType.PlanConfigurationDetail);

            var result = _db.Execute<LeaveHolidaysAndWeekoff>("sp_leave_holidays_and_weekoff_insupd", leaveHolidaysAndWeekoff, true);

            if (string.IsNullOrEmpty(result))
                throw new HiringBellException("Fail to insert or update apply for leave detail.");
            else
            {
                leaveHolidaysAndWeekoff.LeaveHolidaysAndWeekOffId = Convert.ToInt32(result);
                leavePlanConfiguration.leaveHolidaysAndWeekoff = leaveHolidaysAndWeekoff;
                this.UpdateLeavePlanConfigurationDetail(leavePlanTypeId, leavePlanConfiguration);
            }

            return leavePlanConfiguration;
        }

        public LeavePlanConfiguration UpdateLeaveApprovalService(int leavePlanTypeId, LeaveApproval leaveApproval)
        {
            LeavePlanConfiguration leavePlanConfiguration = new LeavePlanConfiguration();
            LeavePlanType leavePlanType = _db.Get<LeavePlanType>("sp_leave_plans_type_getbyId", new { LeavePlanTypeId = leavePlanTypeId });

            if (leavePlanType == null)
                throw new HiringBellException("Invalid plan type id. No record found.");

            if (leaveApproval.IsLeaveRequiredApproval == false)
                leaveApproval.ApprovalChain = new List<ApprovalRoleDetail>();

            leaveApproval.LeavePlanTypeId = leavePlanTypeId;
            if (leavePlanType != null && !string.IsNullOrEmpty(leavePlanType.PlanConfigurationDetail))
                leavePlanConfiguration = JsonConvert.DeserializeObject<LeavePlanConfiguration>(leavePlanType.PlanConfigurationDetail);

            var result = _db.Execute<LeaveApproval>("sp_leave_approval_insupd", new
            {
                leaveApproval.LeaveApprovalId,
                leaveApproval.LeavePlanTypeId,
                leaveApproval.IsLeaveRequiredApproval,
                leaveApproval.ApprovalLevels,
                ApprovalChain = JsonConvert.SerializeObject(leaveApproval.ApprovalChain),
                leaveApproval.IsRequiredAllLevelApproval,
                leaveApproval.CanHigherRankPersonsIsAvailForAction,
                leaveApproval.IsPauseForApprovalNotification,
                leaveApproval.IsReportingManageIsDefaultForAction
            }, true);

            if (string.IsNullOrEmpty(result))
                throw new HiringBellException("Fail to insert or update apply for leave detail.");
            else
            {
                leaveApproval.LeaveApprovalId = Convert.ToInt32(result);
                leavePlanConfiguration.leaveApproval = leaveApproval;
                this.UpdateLeavePlanConfigurationDetail(leavePlanTypeId, leavePlanConfiguration);
            }

            return leavePlanConfiguration;
        }

        public LeavePlanConfiguration UpdateYearEndProcessingService(int leavePlanTypeId, LeaveEndYearProcessing leaveEndYearProcessing)
        {
            LeavePlanConfiguration leavePlanConfiguration = new LeavePlanConfiguration();
            LeavePlanType leavePlanType = _db.Get<LeavePlanType>("sp_leave_plans_type_getbyId", new { LeavePlanTypeId = leavePlanTypeId });

            if (leavePlanType == null)
                throw new HiringBellException("Invalid plan type id. No record found.");

            leaveEndYearProcessing.LeavePlanTypeId = leavePlanTypeId;
            if (leavePlanType != null && !string.IsNullOrEmpty(leavePlanType.PlanConfigurationDetail))
                leavePlanConfiguration = JsonConvert.DeserializeObject<LeavePlanConfiguration>(leavePlanType.PlanConfigurationDetail);

            var result = _db.Execute<LeaveEndYearProcessing>("sp_leave_endyear_processing_insupd", leaveEndYearProcessing, true);

            if (string.IsNullOrEmpty(result))
                throw new HiringBellException("Fail to insert or update apply for leave detail.");
            else
            {
                leaveEndYearProcessing.LeaveEndYearProcessingId = Convert.ToInt32(result);
                leavePlanConfiguration.leaveEndYearProcessing = leaveEndYearProcessing;
                this.UpdateLeavePlanConfigurationDetail(leavePlanTypeId, leavePlanConfiguration);
            }

            return leavePlanConfiguration;
        }
    }
}
