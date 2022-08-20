using BottomhalfCore.DatabaseLayer.Common.Code;
using BottomhalfCore.Services.Code;
using ModalLayer.Modal;
using ModalLayer.Modal.Leaves;
using Newtonsoft.Json;
using ServiceLayer.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceLayer.Code
{
    public class LeaveCalculation : ILeaveCalculation
    {
        private readonly IDb _db;
        private Employee _employee;
        private Leave _employeeLeaveDetail;
        private LeavePlan _leavePlan;
        private List<LeavePlanType> _leavePlanTypes;
        private LeavePlanConfiguration _leavePlanConfiguration;
        private readonly DateTime now = DateTime.UtcNow;
        private CompanySetting _companySetting;
        private DateTime _fromDate;
        private DateTime _toDate;
        private readonly CancellationTokenSource cts = new CancellationTokenSource();

        public LeaveCalculation(IDb db)
        {
            _db = db;
        }

        private decimal CalculateLeaveIfInProbationPeriond(int leavePlanTypeId)
        {
            decimal leaveCount = 0;

            return leaveCount;
        }

        private decimal CalculateLeaveIfInNoticePeriond(int leavePlanTypeId)
        {
            decimal leaveCount = 0;

            return leaveCount;
        }

        private decimal LeaveLimitBeyondQuota() { return 0; }

        public bool CanReportingManagerAwardCasualCredits() { return false; }

        public decimal LeaveAccruedTillDate()
        {
            return 0;
        }

        public decimal EffectIfJoinInMidOfYear() { return 0; }
        public decimal EffectIfExitInMidOfYear() { return 0; }

        public decimal LeaveBasedOnProbationOrExprience() { return 0; }
        public decimal AttendanceImpactOnLeave() { return 0; }
        public decimal WeekOffAndHolidayConsideration() { return 0; }
        public decimal FutureDateLeave() { return 0; }
        public decimal ExtraLeaveOnAccrualBalance() { return 0; }
        public decimal RoundUpTheLeaveIfPresentInFraction() { return 0; }
        public decimal LeaveExpiryCalculation() { return 0; }

        private decimal CalculateFirstMonthOnDefinedLeaveRate(decimal perMonthLeaves)
        {
            decimal firstMonthProrateCount = 0;

            List<AllocateTimeBreakup> allocateTimeBreakup = _leavePlanConfiguration.leaveAccrual.JoiningMonthLeaveDistribution;
            if (allocateTimeBreakup != null && allocateTimeBreakup.Count > 0)
            {
                foreach (var rule in allocateTimeBreakup)
                {
                    if (now.Day >= rule.FromDate)
                        firstMonthProrateCount += (rule.AllocatedLeave * perMonthLeaves);
                }
            }

            return firstMonthProrateCount;
        }

        private decimal CalculateFirstMonthProrateCount(decimal perMonthLeaves)
        {
            decimal firstMonthProrateCount = 0;
            if (_leavePlanConfiguration.leaveAccrual.IsLeavesProratedForJoinigMonth)
            {
                if (_employee.CreatedOn.Day >= 15)
                    // if joining date is after 15 then add half the number of days defined per month as accral
                    firstMonthProrateCount = perMonthLeaves / 2;
                else
                    // if joining date is before 15 then add full the number of days defined per month as accral
                    firstMonthProrateCount = perMonthLeaves;
            }
            else
            {
                firstMonthProrateCount = CalculateFirstMonthOnDefinedLeaveRate(perMonthLeaves);
            }

            return firstMonthProrateCount;
        }

        public decimal NoticePeriodDistinctLeavesCalculation(decimal perMonthLeaves)
        {
            decimal days = 0;
            if (_leavePlanConfiguration.leaveAccrual.ExitMonthLeaveDistribution != null
                && _leavePlanConfiguration.leaveAccrual.ExitMonthLeaveDistribution.Count > 0)
            {
                AllocateTimeBreakup allocateTimeBreakup = null;
                int i = 0;
                while (i < _leavePlanConfiguration.leaveAccrual.ExitMonthLeaveDistribution.Count)
                {
                    allocateTimeBreakup = _leavePlanConfiguration.leaveAccrual.ExitMonthLeaveDistribution.ElementAt(i);
                    if (now.Day >= allocateTimeBreakup.FromDate)
                        days += (allocateTimeBreakup.AllocatedLeave + perMonthLeaves);

                    i++;
                }
            }

            return days;
        }

        private decimal CalculatePresentMonthProrateCount(decimal perMonthLeaves)
        {
            decimal presentMonthDays = 0;
            if (_leavePlanConfiguration.leaveAccrual.LeaveDistributionAppliedFrom - now.Day <= 0)
                // if present month date is less then or equal to the date define for accrual
                presentMonthDays = perMonthLeaves;
            else
                // if present month date is greather than to the date define for accrual
                presentMonthDays = 0;

            return presentMonthDays;
        }

        private int GetLeaveProjectedLastMonth()
        {
            int month = 0;
            if (_leavePlanConfiguration.leaveAccrual.CanApplyForFutureDate)
                month = _fromDate.Month;
            else
                month = now.Month;

            return month;
        }

        private int GetNumOfMonthExceptFirstAndLast()
        {
            int remaningMonths = 0;
            int firstMonth = 1;
            int lastMonth = GetLeaveProjectedLastMonth();

            if (_employee.CreatedOn.Year == now.Year)
                firstMonth = _employee.CreatedOn.Month;

            if (lastMonth - firstMonth >= 2)
                remaningMonths = lastMonth - firstMonth - 1;

            return remaningMonths;
        }

        private decimal CalculateWhenAccralMonthly(decimal alreadyAppliedLeave, decimal perMonthLeaves)
        {
            decimal availableLeaves = 0;
            decimal presentMonthLeaves = 0;

            if (_leavePlanConfiguration.leaveAccrual.IsLeavesProratedOnNotice)
                presentMonthLeaves = CalculatePresentMonthProrateCount(perMonthLeaves);
            else
                presentMonthLeaves = NoticePeriodDistinctLeavesCalculation(perMonthLeaves);

            int mounthCount = GetNumOfMonthExceptFirstAndLast();
            if (_employee.CreatedOn.Year == now.Year)
            {
                decimal firstMonthCalculation = CalculateFirstMonthProrateCount(perMonthLeaves);
                availableLeaves = (mounthCount * perMonthLeaves) + firstMonthCalculation + presentMonthLeaves;
            }
            else
            {
                availableLeaves = mounthCount * perMonthLeaves + presentMonthLeaves;
            }

            return availableLeaves;
        }

        private decimal CalculateWhenAccralQuaterly(decimal alreadyAppliedLeave)
        {
            decimal availableLeaves = 0;

            return availableLeaves;
        }

        private decimal CalculateWhenAccralYearly(decimal alreadyAppliedLeave)
        {
            decimal availableLeaves = 0;

            return availableLeaves;
        }

        private decimal LeaveAccrualInProbationForExperienced(decimal perMonthLeaves)
        {
            return 0;
        }

        private decimal CheckWeekOffRuleApplication(decimal perMonthLeaves)
        {
            return 0;
        }

        private decimal CheckHolidayRuleApplication(decimal perMonthLeaves)
        {
            return 0;
        }

        private decimal LeaveAccrualForExperienceInProbation(decimal perMonthLeaves)
        {
            decimal days = 0;
            if (_leavePlanConfiguration.leaveAccrual.IsVaryOnProbationOrExprience)
                days = LeaveAccrualInProbationForExperienced(perMonthLeaves);

            return days;
        }

        private bool CheckIfAlreadyOnLeaveForMoreThen(decimal leaveCount)
        {
            return true;
        }

        private decimal CheckAddExtraAccrualLeaveBalance(decimal perMonthLeaves, decimal leaveBalance)
        {
            if (_leavePlanConfiguration.leaveAccrual.IsExtraLeaveBeyondAccruedBalance)
            {
                // yes can apply leave beyond their quota.
                if (_leavePlanConfiguration.leaveAccrual.IsAccrueIfHavingLeaveBalance)
                {
                    if (_leavePlanConfiguration.leaveAccrual.AllowOnlyIfAccrueBalanceIsAlleast < leaveBalance)
                    {
                        return leaveBalance + _leavePlanConfiguration.leaveAccrual.NoOfDaysForExtraLeave;
                    }
                }

                if (_leavePlanConfiguration.leaveAccrual.IsAccrueIfOnOtherLeave)
                {
                    if (!CheckIfAlreadyOnLeaveForMoreThen(_leavePlanConfiguration.leaveAccrual.NotAllowIfAlreadyOnLeaveMoreThan))
                    {
                        return leaveBalance + _leavePlanConfiguration.leaveAccrual.NoOfDaysForExtraLeave;
                    }
                }
            }

            return leaveBalance;
        }

        private decimal RoundUpTheLeaves(decimal availableLeaves)
        {
            return availableLeaves;
        }

        private decimal UpdateLeaveIfSetForExpiry(decimal availableLeaves)
        {
            return availableLeaves;
        }

        private decimal CalculateLeaveIfRegularEmployement(int leavePlanTypeId)
        {
            decimal alreadyAppliedLeave = 0;
            decimal availableLeaves = 0;
            decimal leaveLimit = 0;

            List<CompleteLeaveDetail> completeLeaveDetails = JsonConvert.DeserializeObject<List<CompleteLeaveDetail>>(_employeeLeaveDetail.LeaveDetail);
            if (completeLeaveDetails.Count > 0)
                alreadyAppliedLeave = completeLeaveDetails.FindAll(x => x.LeaveType == leavePlanTypeId && x.LeaveStatus == (int)ItemStatus.Approved).Sum(x => x.NumOfDays);

            leaveLimit = _leavePlanConfiguration.leaveDetail.LeaveLimit;
            decimal perMonthLeaves = _leavePlanConfiguration.leaveDetail.LeaveLimit / 12;

            if (_leavePlanConfiguration.leaveAccrual.CanApplyEntireLeave)
            {
                availableLeaves = leaveLimit - alreadyAppliedLeave;
            }
            else
            {
                var leaveDistributedSeq = _leavePlanConfiguration.leaveAccrual.LeaveDistributionSequence;
                switch (leaveDistributedSeq)
                {
                    case "1":
                        availableLeaves = this.CalculateWhenAccralMonthly(alreadyAppliedLeave, perMonthLeaves);
                        break;
                    case "2":
                        availableLeaves = this.CalculateWhenAccralQuaterly(alreadyAppliedLeave);
                        break;
                    case "3":
                        availableLeaves = this.CalculateWhenAccralYearly(alreadyAppliedLeave);
                        break;
                }

                availableLeaves = availableLeaves - alreadyAppliedLeave;
            }

            decimal extraLeaveLimit = 0;

            // calculate leave for experienced people and allowed accrual in probation
            extraLeaveLimit = LeaveAccrualForExperienceInProbation(perMonthLeaves);
            availableLeaves = availableLeaves + extraLeaveLimit;

            // check weekoff as absent if rule applicable
            extraLeaveLimit = CheckWeekOffRuleApplication(perMonthLeaves);
            availableLeaves = availableLeaves - extraLeaveLimit;

            // check weekoff as absent if rule applicable
            extraLeaveLimit = CheckHolidayRuleApplication(perMonthLeaves);
            availableLeaves = availableLeaves - extraLeaveLimit;

            // check weather can apply for leave based on future date projected balance
            availableLeaves = CheckAddExtraAccrualLeaveBalance(perMonthLeaves, availableLeaves);

            // round up decimal value of available as per rule defined
            availableLeaves = RoundUpTheLeaves(availableLeaves);

            // check leave expiry
            availableLeaves = UpdateLeaveIfSetForExpiry(availableLeaves);

            return availableLeaves;
        }

        private void ValidateAndGetLeavePlanConfiguration(LeavePlanType leavePlanType)
        {
            // fetching data from database using leaveplantypeId
            _leavePlanConfiguration = JsonConvert.DeserializeObject<LeavePlanConfiguration>(leavePlanType.PlanConfigurationDetail);
            if (_leavePlanConfiguration == null)
                throw new HiringBellException("Leave setup/configuration is not defined. Please complete the setup/configuration first.");
        }

        private int LoadCalculationData(long EmployeeId)
        {
            var ds = _db.FetchDataSet("sp_leave_plan_calculation_get", new { EmployeeId, IsActive = 1, Year = now.Year }, false);
            if (ds != null && ds.Tables.Count == 4)
            {
                if (ds.Tables[0].Rows.Count == 0 || ds.Tables[1].Rows.Count == 0 || ds.Tables[3].Rows.Count == 0)
                    throw new HiringBellException("Fail to get employee related details. Please contact to admin.");

                _employee = Converter.ToType<Employee>(ds.Tables[0]);
                _leavePlanTypes = Converter.ToList<LeavePlanType>(ds.Tables[1]);
                _employeeLeaveDetail = Converter.ToType<Leave>(ds.Tables[2]);
                _companySetting = Converter.ToType<CompanySetting>(ds.Tables[3]);
            }
            else
                throw new HiringBellException("Employee does not exist. Please contact to admin.");

            int type = 0;
            if (now.Subtract(_employee.CreatedOn).TotalDays <= _employee.ProbationPeriodDaysLimit)
                type = 1;

            //if (_employee.NoticePeriodId > 0)
            //    type = 2;

            return type;
        }

        private bool IsMinimunGapBeforAffectedDate()
        {
            return false;
        }

        private bool DoesLeaveRequiredComments()
        {
            return false;
        }

        private bool RequiredDocumentForExtending()
        {
            return false;
        }

        public async Task<List<LeavePlanType>> GetBalancedLeave(long EmployeeId, DateTime FromDate, DateTime ToDate)
        {
            _fromDate = FromDate;
            _toDate = ToDate;

            // get employee detail and store it in class level variable
            int type = LoadCalculationData(EmployeeId);

            return await Task.Run<List<LeavePlanType>>(() =>
            {
                LeavePlanType leavePlanType = null;
                int i = 0;
                while (i < _leavePlanTypes.Count)
                {
                    leavePlanType = _leavePlanTypes[i];
                    ValidateAndGetLeavePlanConfiguration(leavePlanType);

                    if (_leavePlanConfiguration.leaveDetail.IsLeaveDaysLimit == true)
                    {
                        var leaveLimit = _leavePlanConfiguration.leaveDetail.LeaveLimit;
                        if (leaveLimit <= 0)
                            throw new HiringBellException("leave limit is null or empty");

                        if (_leavePlanConfiguration.leaveAccrual.IsNoLeaveOnNoticePeriod)
                            type = 2;

                        switch (type)
                        {
                            case 1:
                                leavePlanType.AvailableLeave = CalculateLeaveIfInProbationPeriond(leavePlanType.LeavePlanTypeId);
                                break;
                            case 2:
                                leavePlanType.AvailableLeave = CalculateLeaveIfInNoticePeriond(leavePlanType.LeavePlanTypeId);
                                break;
                            default:
                                leavePlanType.AvailableLeave = CalculateLeaveIfRegularEmployement(leavePlanType.LeavePlanTypeId);
                                break;
                        }
                    }
                    else
                        leavePlanType.AvailableLeave = -1;

                    i++;
                }

                return _leavePlanTypes;
            }, cts.Token);
        }

        public async Task<bool> CanApplyForHalfDay(long EmployeeId, DateTime FromDate, DateTime ToDate)
        {
            return await Task.Run<bool>(() =>
            {
                return false;
            }, cts.Token);
        }

        public async Task<bool> AllowToSeeAndApply(long EmployeeId, DateTime FromDate, DateTime ToDate)
        {
            return await Task.Run<bool>(() =>
            {
                return false;
            }, cts.Token);
        }

        public async Task<bool> CanApplyBackDatedLeave(long EmployeeId, DateTime FromDate, DateTime ToDate)
        {
            return await Task.Run<bool>(() =>
            {
                return false;
            }, cts.Token);
        }
    }
}