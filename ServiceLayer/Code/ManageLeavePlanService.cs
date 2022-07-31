﻿using BottomhalfCore.DatabaseLayer.Common.Code;
using BottomhalfCore.Services.Code;
using ModalLayer.Modal;
using ModalLayer.Modal.Leaves;
using Newtonsoft.Json;
using ServiceLayer.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            if (leavePlanTypeId <= 0)
                throw new HiringBellException("Invalid plan selected");

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
            if (leavePlanTypeId <= 0)
                throw new HiringBellException("Invalid plan selected");

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

            if (leaveAccrual.IsLeaveAccruedPatternAvail == false)
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
            if (leavePlanTypeId <= 0)
                throw new HiringBellException("Invalid plan selected");

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
            if (leavePlanTypeId <= 0)
                throw new HiringBellException("Invalid plan selected");

            LeavePlanConfiguration leavePlanConfiguration = new LeavePlanConfiguration();
            LeavePlanType leavePlanType = _db.Get<LeavePlanType>("sp_leave_plans_type_getbyId", new { LeavePlanTypeId = leavePlanTypeId });

            if (leavePlanType == null)
                throw new HiringBellException("Invalid plan type id. No record found.");

            if (leavePlanType != null && !string.IsNullOrEmpty(leavePlanType.PlanConfigurationDetail))
                leavePlanConfiguration = JsonConvert.DeserializeObject<LeavePlanConfiguration>(leavePlanType.PlanConfigurationDetail);

            if (leaveApplyDetail.EmployeeCanSeeAndApplyCurrentPlanLeave == true)
                leaveApplyDetail.RuleForLeaveInNotice = new List<LeaveRuleInNotice>();

            if (leaveApplyDetail.ProofRequiredIfDaysExceeds == false)
                leaveApplyDetail.NoOfDaysExceeded = 0;

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
            if (leavePlanTypeId <= 0)
                throw new HiringBellException("Invalid plan selected");

            LeavePlanConfiguration leavePlanConfiguration = new LeavePlanConfiguration();
            LeavePlanType leavePlanType = _db.Get<LeavePlanType>("sp_leave_plans_type_getbyId", new { LeavePlanTypeId = leavePlanTypeId });

            if (leavePlanType == null)
                throw new HiringBellException("Invalid plan type id. No record found.");

            if (leavePlanType != null && !string.IsNullOrEmpty(leavePlanType.PlanConfigurationDetail))
                leavePlanConfiguration = JsonConvert.DeserializeObject<LeavePlanConfiguration>(leavePlanType.PlanConfigurationDetail);

            if (leavePlanRestriction.CanApplyAfterProbation == false)
                leavePlanRestriction.DaysAfterProbation = 0;

            if (leavePlanRestriction.CanApplyAfterJoining == false)
                leavePlanRestriction.DaysAfterJoining = 0;

            if (leavePlanRestriction.IsConsecutiveLeaveLimit == false)
                leavePlanRestriction.ConsecutiveDaysLimit = 0;

            if (leavePlanRestriction.IsLeaveInNoticeExtendsNoticePeriod == false)
                leavePlanRestriction.NoOfTimesNoticePeriodExtended = 0;

            if (leavePlanRestriction.IsCurrentPlanDepnedsOnOtherPlan == false)
                leavePlanRestriction.AssociatedPlanTypeId = 0;

            if (leavePlanRestriction.IsCheckOtherPlanTypeBalance == false)
                leavePlanRestriction.DependentPlanTypeId = 0;

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
            if (leavePlanTypeId <= 0)
                throw new HiringBellException("Invalid plan selected");

            LeavePlanConfiguration leavePlanConfiguration = new LeavePlanConfiguration();
            LeavePlanType leavePlanType = _db.Get<LeavePlanType>("sp_leave_plans_type_getbyId", new { LeavePlanTypeId = leavePlanTypeId });

            if (leavePlanType == null)
                throw new HiringBellException("Invalid plan type id. No record found.");

            leaveHolidaysAndWeekoff.LeavePlanTypeId = leavePlanTypeId;
            if (leavePlanType != null && !string.IsNullOrEmpty(leavePlanType.PlanConfigurationDetail))
                leavePlanConfiguration = JsonConvert.DeserializeObject<LeavePlanConfiguration>(leavePlanType.PlanConfigurationDetail);

            if (leaveHolidaysAndWeekoff.AdJoiningHolidayIsConsiderAsLeave == false)
            {
                leaveHolidaysAndWeekoff.ConsiderLeaveIfNumOfDays = 0;
                leaveHolidaysAndWeekoff.IfLeaveLieBetweenTwoHolidays = false;
                leaveHolidaysAndWeekoff.IfHolidayIsRightBeforLeave = false;
                leaveHolidaysAndWeekoff.IfHolidayIsRightAfterLeave = false;
                leaveHolidaysAndWeekoff.IfHolidayIsBetweenLeave = false;
                leaveHolidaysAndWeekoff.IfHolidayIsRightBeforeAfterOrInBetween = false;
            }

            if (leaveHolidaysAndWeekoff.AdjoiningWeekOffIsConsiderAsLeave == false)
            {
                leaveHolidaysAndWeekoff.ConsiderLeaveIfIncludeDays = 0;
                leaveHolidaysAndWeekoff.IfLeaveLieBetweenWeekOff = false;
                leaveHolidaysAndWeekoff.IfWeekOffIsRightBeforLeave = false;
                leaveHolidaysAndWeekoff.IfWeekOffIsRightAfterLeave = false;
                leaveHolidaysAndWeekoff.IfWeekOffIsBetweenLeave = false;
                leaveHolidaysAndWeekoff.IfWeekOffIsRightBeforeAfterOrInBetween = false;
            }

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
            if (leavePlanTypeId <= 0)
                throw new HiringBellException("Invalid plan selected");

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
            if (leavePlanTypeId <= 0)
                throw new HiringBellException("Invalid plan selected");

            LeavePlanConfiguration leavePlanConfiguration = new LeavePlanConfiguration();
            LeavePlanType leavePlanType = _db.Get<LeavePlanType>("sp_leave_plans_type_getbyId", new { LeavePlanTypeId = leavePlanTypeId });

            if (leavePlanType == null)
                throw new HiringBellException("Invalid plan type id. No record found.");

            leaveEndYearProcessing.LeavePlanTypeId = leavePlanTypeId;
            if (leavePlanType != null && !string.IsNullOrEmpty(leavePlanType.PlanConfigurationDetail))
                leavePlanConfiguration = JsonConvert.DeserializeObject<LeavePlanConfiguration>(leavePlanType.PlanConfigurationDetail);

            if (leaveEndYearProcessing.DoestCarryForwardExpired == false)
                leaveEndYearProcessing.ExpiredAfter = 0;

            if (leaveEndYearProcessing.PayNCarryForwardDefineType == "fixed" || (leaveEndYearProcessing.PayFirstNCarryForwordRemaning == false && leaveEndYearProcessing.CarryForwordFirstNPayRemaning == false))
                leaveEndYearProcessing.PercentagePayNCarryForward = new List<PercentagePayNCarryForward>();

            if (leaveEndYearProcessing.PayNCarryForwardDefineType == "percentage" || (leaveEndYearProcessing.PayFirstNCarryForwordRemaning == false && leaveEndYearProcessing.CarryForwordFirstNPayRemaning == false))
                leaveEndYearProcessing.FixedPayNCarryForward = new List<FixedPayNCarryForward>();

            if (leaveEndYearProcessing.PayFirstNCarryForwordRemaning == false && leaveEndYearProcessing.CarryForwordFirstNPayRemaning == false)
                leaveEndYearProcessing.PayNCarryForwardDefineType = "";

            var result = _db.Execute<LeaveEndYearProcessing>("sp_leave_endyear_processing_insupd", new
            {
                leaveEndYearProcessing.LeaveEndYearProcessingId,
                leaveEndYearProcessing.LeavePlanTypeId,
                leaveEndYearProcessing.IsLeaveBalanceExpiredOnEndOfYear,
                leaveEndYearProcessing.AllConvertedToPaid,
                leaveEndYearProcessing.AllLeavesCarryForwardToNextYear,
                leaveEndYearProcessing.PayFirstNCarryForwordRemaning,
                leaveEndYearProcessing.CarryForwordFirstNPayRemaning,
                leaveEndYearProcessing.PayNCarryForwardForPercent,
                leaveEndYearProcessing.PayNCarryForwardDefineType,
                FixedPayNCarryForward = JsonConvert.SerializeObject(leaveEndYearProcessing.FixedPayNCarryForward),
                PercentagePayNCarryForward = JsonConvert.SerializeObject(leaveEndYearProcessing.PercentagePayNCarryForward),
                leaveEndYearProcessing.DoestCarryForwardExpired,
                leaveEndYearProcessing.ExpiredAfter,
                leaveEndYearProcessing.DoesExpiryLeaveRemainUnchange,
                leaveEndYearProcessing.DeductFromSalaryOnYearChange,
                leaveEndYearProcessing.ResetBalanceToZero,
                leaveEndYearProcessing.CarryForwardToNextYear
            }, true);

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

        public string AddUpdateEmpLeavePlanService(int leavePlanId, List<EmpLeavePlanMapping> empLeavePlanMapping)
        {
            string status = string.Empty;
            if (leavePlanId <= 0)
                throw new Exception("Invalid plan selected.");

            var table = Converter.ToDataTable(empLeavePlanMapping);
            var result = _db.BatchInsert("sp_employee_leaveplan_mapping_insupd", table, true);

            if (result <= 0)
                throw new HiringBellException("Fail to insert or update employee leave plan deatils.");
            else
                status = "updated";

            return status;
        }

        public List<EmpLeavePlanMapping> GetEmpMappingByLeavePlanIdService(int leavePlanId)
        {
            if (leavePlanId <= 0)
                throw new Exception("Invalid plan selected.");

            List<EmpLeavePlanMapping> empLeavePlanMappings = _db.GetList<EmpLeavePlanMapping>("sp_employee_leaveplan_mapping_GetByPlanId", new { LeavePlanId = leavePlanId });
            Parallel.ForEach(empLeavePlanMappings, i =>
            {
                i.IsAdded = true;
            });

            return empLeavePlanMappings;
        }
    }
}
