using BottomhalfCore.Services.Interface;
using ModalLayer.Modal;
using ModalLayer.Modal.Leaves;
using System;
using System.Linq;
using System.Threading.Tasks;
using TimeZoneConverter;

namespace ServiceLayer.Code.Leaves
{
    public class Accrual
    {
        private readonly ITimezoneConverter _timezoneConverter;
        private readonly CurrentSession _currentSession;
        private LeavePlanConfiguration _leavePlanConfiguration = default;
        private LeaveCalculationModal _leaveCalculationModal;
        private DateTime now;

        public Accrual(ITimezoneConverter timezoneConverter,
            CurrentSession currentSession)
        {
            _timezoneConverter = timezoneConverter;
            _currentSession = currentSession;
        }

        // applicable while appling leave, include steps 6, 7, 8
        #region APPLYING AND CONDITIONAL ACCRUALS

        public async Task ConditionaLeaveAccruals(DateTime leaveFromDate, decimal leavePerDay, decimal extraLeave, decimal availableLeaveBalance, decimal onLeaveFromDays, LeavePlanType leavePlanType)
        {
            // check for projected furute accruals
            await ProjectedFutureLeaveAccrualedBalance(leaveFromDate, leavePerDay);

            // check can apply beyond accrual leaves
            bool flag = await CanApplyBeyondAccrualBalance(extraLeave, availableLeaveBalance, onLeaveFromDays);

            // step - 8
            // round up decimal value of available as per rule defined
            leavePlanType.AvailableLeave = RoundUpTheLeaves(leavePlanType.AvailableLeave);
        }

        // step 6
        // projected leave if applied for future date.
        private async Task ProjectedFutureLeaveAccrualedBalance(DateTime leaveFromDate, decimal leavePerDay)
        {
            decimal leaves = 0;
            int futureProjectedMonths = 0;
            if (_leavePlanConfiguration.leaveAccrual.LeaveDistributionAppliedFrom <= now.Day)
                futureProjectedMonths++;

            futureProjectedMonths += leaveFromDate.Month - now.Month;

            int i = 0;
            while (i <= futureProjectedMonths)
            {
                leaves += MonthlyAccrual(leavePerDay);
                i++;
            }


            await Task.CompletedTask;
        }

        // step 7
        // call this method when applying for leave of this catagory or leave type but 
        // leaves are not in adequate amount
        private async Task<bool> CanApplyBeyondAccrualBalance(decimal extraLeave, decimal availableLeaveBalance, decimal onLeaveFromDays)
        {
            bool flag = false;

            // check only if available balance (including carry forward) less then defined value.
            if (_leavePlanConfiguration.leaveAccrual.IsAccrueIfHavingLeaveBalance)
            {
                if (availableLeaveBalance >= _leavePlanConfiguration.leaveAccrual.AllowOnlyIfAccrueBalanceIsAlleast)
                    return await Task.FromResult(false);
            }

            // don't allow if already in leave for more then defined period
            if (_leavePlanConfiguration.leaveAccrual.IsAccrueIfOnOtherLeave)
            {
                if (onLeaveFromDays >= _leavePlanConfiguration.leaveAccrual.NotAllowIfAlreadyOnLeaveMoreThan)
                    return await Task.FromResult(false);
            }

            // yes but not more then defined quantity
            if (_leavePlanConfiguration.leaveAccrual.IsExtraLeaveBeyondAccruedBalance)
            {
                if (extraLeave <= _leavePlanConfiguration.leaveAccrual.NoOfDaysForExtraLeave)
                    flag = true;
            }
            else
                flag = false;

            return await Task.FromResult(flag);
        }

        #endregion


        public async Task CalculateLeaveAccrual(LeaveCalculationModal leaveCalculationModal, LeavePlanType leavePlanType)
        {
            _leaveCalculationModal = leaveCalculationModal;
            _leavePlanConfiguration = leaveCalculationModal.leavePlanConfiguration;
            now = leaveCalculationModal.presentDate;

            if (!await CanApplyEntireLeave(leavePlanType))
            {
                var leaveLimit = _leavePlanConfiguration.leaveDetail.LeaveLimit;
                if (leaveLimit > 0)
                {
                    // check leave expiry
                    if (!await DoesLeaveExpired())
                    {
                        // start leave accrual calculation for present month
                        leavePlanType.AvailableLeave = await ExecuteLeaveAccrualDetail();
                    }
                }
            }

            await Task.CompletedTask;
        }

        // step - 1
        private async Task<bool> CanApplyEntireLeave(LeavePlanType leaveType)
        {
            bool flag = false;
            if (_leavePlanConfiguration.leaveAccrual.CanApplyEntireLeave)
            {
                flag = true;
                leaveType.AvailableLeave = _leavePlanConfiguration.leaveDetail.LeaveLimit;
            }

            return await Task.FromResult(flag);
        }

        private async Task<decimal> ExecuteLeaveAccrualDetail()
        {
            decimal availableLeaveLimit = 0;
            decimal leaveFrequencyForDefinedPeriod = 0;
            var leaveDistributedSeq = _leavePlanConfiguration.leaveAccrual.LeaveDistributionSequence;
            switch (leaveDistributedSeq)
            {
                default:
                    if (_leavePlanConfiguration.leaveAccrual.IsLeaveAccruedPatternAvail)
                    {
                        leaveFrequencyForDefinedPeriod = _leavePlanConfiguration.leaveDetail.LeaveLimit / 12.0m;
                        availableLeaveLimit = await MonthlyAccrualCalculation(leaveFrequencyForDefinedPeriod);
                    }
                    else
                    {
                        leaveFrequencyForDefinedPeriod = _leavePlanConfiguration.leaveDetail.LeaveLimit / 12.0m;
                        availableLeaveLimit = _leavePlanConfiguration.leaveDetail.LeaveLimit;
                    }
                    break;
                case "2":
                    leaveFrequencyForDefinedPeriod = _leavePlanConfiguration.leaveDetail.LeaveLimit / 4.0m;
                    availableLeaveLimit = QuaterlyAccrualCalculation(leaveFrequencyForDefinedPeriod);
                    break;
                case "3":
                    leaveFrequencyForDefinedPeriod = _leavePlanConfiguration.leaveDetail.LeaveLimit / 2.0m;
                    availableLeaveLimit = HalfYearlyAccrualCalculation(leaveFrequencyForDefinedPeriod);
                    break;
            }

            return await Task.FromResult(availableLeaveLimit);
        }

        // step - 4
        private async Task<bool> CheckAccrualEligibility()
        {
            bool flag = false;
            DateTime accrualStartDate = _leaveCalculationModal.employee.CreatedOn;
            if (_leavePlanConfiguration.leaveAccrual.IsAccrualStartsAfterJoining)
            {
                var daysAfterJoining = _leavePlanConfiguration.leaveAccrual.AccrualDaysAfterJoining;
                accrualStartDate.AddDays((double)daysAfterJoining);
            }
            else if (_leavePlanConfiguration.leaveAccrual.IsAccrualStartsAfterProbationEnds)
            {
                var daysProbationEnds = _leavePlanConfiguration.leaveAccrual.AccrualDaysAfterProbationEnds;
                daysProbationEnds += _leaveCalculationModal.companySetting.ProbationPeriodInDays;
                accrualStartDate.AddDays((double)daysProbationEnds);
            }

            if (accrualStartDate.Date.Subtract(now).TotalDays >= 0)
                flag = true;

            return await Task.FromResult(flag);
        }

        #region HALF YEARLY ACCRUAL CALCULATION

        private decimal HalfYearlyAccrualCalculation(decimal perHalflyLeave)
        {
            decimal availableLeaves = 0;
            switch (_leaveCalculationModal.employeeType)
            {
                case ApplicationConstants.InProbationPeriod:
                    availableLeaves = MonthlyAccrualInProbation();
                    break;
                case ApplicationConstants.InNoticePeriod:
                    availableLeaves = MonthlyAccrualInNotice(perHalflyLeave);
                    break;
                default:
                    availableLeaves = MonthlyAccrual(perHalflyLeave) * 2;
                    break;
            }

            return availableLeaves;
        }

        #endregion


        #region QUATERLY ACCRUAL CALCULATION

        private decimal QuaterlyAccrualCalculation(decimal perQuarterLeave)
        {
            decimal availableLeaves = 0;
            switch (_leaveCalculationModal.employeeType)
            {
                case ApplicationConstants.InProbationPeriod:
                    availableLeaves = MonthlyAccrualInProbation();
                    break;
                case ApplicationConstants.InNoticePeriod:
                    availableLeaves = MonthlyAccrualInNotice(perQuarterLeave);
                    break;
                default:
                    availableLeaves = MonthlyAccrual(perQuarterLeave) * 4;
                    break;
            }

            return availableLeaves;
        }

        #endregion


        #region MONTHLY ACCRUAL CALCULATION
        private async Task<decimal> MonthlyAccrualCalculation(decimal perMonthLeaves)
        {
            decimal availableLeaves = 0;
            switch (_leaveCalculationModal.employeeType)
            {
                case ApplicationConstants.InProbationPeriod:
                    // step - 4
                    if (_leavePlanConfiguration.leaveAccrual.IsVaryOnProbationOrExprience)
                    {
                        if (await CheckAccrualEligibility())
                            availableLeaves = MonthlyAccrualInProbation();
                        else
                            return availableLeaves;
                    }
                    else
                    {
                        availableLeaves = MonthlyAccrualInProbation();
                    }

                    break;
                case ApplicationConstants.InNoticePeriod:
                    availableLeaves = MonthlyAccrualInNotice(perMonthLeaves);
                    break;
                default:
                    availableLeaves = MonthlyAccrual(perMonthLeaves);
                    break;
            }

            return await Task.FromResult(availableLeaves);
        }

        // used for steps 3
        // Prorate distribution of leave if employee is serving notice period
        public decimal MonthlyAccrualInNotice(decimal perMonthLeaves)
        {
            int leavingDay = 31;
            decimal accruledLeave = 0;
            // if present month is last month of his/her notice period.
            if (_leaveCalculationModal.noticePeriodEndDate.Month == now.Month)
                leavingDay = _leaveCalculationModal.noticePeriodEndDate.Day;


            if (_leavePlanConfiguration.leaveAccrual.IsLeavesProratedOnNotice)
            {
                return MonthlyAccrual(perMonthLeaves);
            }
            else if (_leavePlanConfiguration.leaveAccrual.IsNotAllowProratedOnNotice)
            {
                if (_leavePlanConfiguration.leaveAccrual.ExitMonthLeaveDistribution != null
                    && _leavePlanConfiguration.leaveAccrual.ExitMonthLeaveDistribution.Count > 0)
                {
                    AllocateTimeBreakup allocateTimeBreakup = null;
                    int i = 0;
                    while (i < _leavePlanConfiguration.leaveAccrual.ExitMonthLeaveDistribution.Count)
                    {
                        allocateTimeBreakup = _leavePlanConfiguration.leaveAccrual.ExitMonthLeaveDistribution.ElementAt(i);
                        if (leavingDay >= allocateTimeBreakup.ToDate)
                            accruledLeave += allocateTimeBreakup.AllocatedLeave;

                        i++;
                    }
                }
            }

            return accruledLeave;
        }

        private decimal MonthlyAccrual(decimal perMonthDays)
        {
            return perMonthDays;
        }

        private decimal MonthlyAccrualInProbation()
        {
            decimal accrualedLeaves = 0;
            int joiningDay = 31;
            var joiningDate = _leaveCalculationModal.employee.CreatedOn;

            // joined in present month then calculate based on the joining day
            if (joiningDate.Month == now.Month)
                joiningDay = joiningDate.Day;

            // use define leavedistribution and calcualte leave accrued
            //_leavePlanConfiguration.leaveAccrual.IsLeavesProratedForJoinigMonth
            if (!_leavePlanConfiguration.leaveAccrual.IsLeavesProratedForJoinigMonth &&
               _leavePlanConfiguration.leaveAccrual.JoiningMonthLeaveDistribution != null)
            {
                AllocateTimeBreakup allocateTimeBreakup = null;
                int i = 0;
                while (i < _leavePlanConfiguration.leaveAccrual.JoiningMonthLeaveDistribution.Count)
                {
                    allocateTimeBreakup = _leavePlanConfiguration.leaveAccrual
                                            .JoiningMonthLeaveDistribution.ElementAt(i);
                    if (joiningDay >= allocateTimeBreakup.ToDate)
                        accrualedLeaves += allocateTimeBreakup.AllocatedLeave;
                    i++;
                }
            }

            return accrualedLeaves;
        }

        #endregion


        private decimal CheckWeekOffRuleApplicable(LeaveCalculationModal leaveCalculationModal)
        {
            int totalWeekEnds = 0;
            decimal presentMonthAttendance = 0;
            // check two condition here
            // 1. WeekOffAsAbsentIfAttendaceLessThen must be greater then 0
            // 2. Employee should not exceed leave or absent for the present month more then WeekOffAsAbsentIfAttendaceLessThen value.

            if (presentMonthAttendance > 0)
            {
                decimal percentageValueForCurrentMonth = (now.Day / presentMonthAttendance) * 100;
                if (_leavePlanConfiguration.leaveAccrual.WeekOffAsAbsentIfAttendaceLessThen > 0 /* && [Put 2nd condition here]*/)
                {
                    if (leaveCalculationModal.fromDate.Year == now.Year && leaveCalculationModal.fromDate.Month == now.Month
                        && leaveCalculationModal.companySetting.IsUseInternationalWeekDays)
                    {
                        TimeZoneInfo timeZoneInfo = TimeZoneInfo.Utc;
                        if (!string.IsNullOrEmpty(_currentSession.Culture))
                        {
                            switch (_currentSession.Culture)
                            {
                                case "ist":
                                    timeZoneInfo = TZConvert.GetTimeZoneInfo("India Standard Time");
                                    break;
                            }
                        }

                        totalWeekEnds = _timezoneConverter.TotalWeekEndsBetweenDates(Convert.ToDateTime($"{leaveCalculationModal.toDate.Year}-{leaveCalculationModal.toDate.Month}-1"), leaveCalculationModal.toDate, timeZoneInfo);

                        // get all leaves or absent data of present employee and then place logic to calculate leave
                        totalWeekEnds = 0;
                    }
                }
            }

            return totalWeekEnds;
        }

        private decimal RoundUpTheLeaves(decimal availableLeaves)
        {
            decimal fractionValue = 0;
            int integralValue = 0;
            if (!_leavePlanConfiguration.leaveAccrual.RoundOffLeaveBalance)
            {
                fractionValue = availableLeaves % 1.0m;
                if (fractionValue > 0)
                {
                    integralValue = Convert.ToInt32(Math.Truncate(availableLeaves));
                    if (_leavePlanConfiguration.leaveAccrual.ToNearestHalfDay)
                    {
                        if (fractionValue >= 0.1m && fractionValue <= 0.2m)
                            availableLeaves = (decimal)integralValue;
                        else if (fractionValue > 0.2m && fractionValue <= 0.5m)
                            availableLeaves = (decimal)integralValue + 0.5m;
                        else if (fractionValue < 0.8m)
                            availableLeaves = (decimal)integralValue + 0.5m;
                        else
                            availableLeaves = (decimal)integralValue + 1;
                    }

                    if (_leavePlanConfiguration.leaveAccrual.ToNearestFullDay)
                    {
                        if (fractionValue >= 0.5m)
                            availableLeaves = (decimal)integralValue + 1;
                    }

                    if (_leavePlanConfiguration.leaveAccrual.ToNextAvailableHalfDay)
                    {
                        if (fractionValue >= 0.1m && fractionValue < 0.5m)
                            availableLeaves = (decimal)integralValue + 0.5m;
                        else
                            availableLeaves = (decimal)integralValue + 1;
                    }

                    if (_leavePlanConfiguration.leaveAccrual.ToNextAvailableFullDay)
                    {
                        if (fractionValue >= 0.1m)
                            availableLeaves = (decimal)integralValue++;
                    }

                    if (_leavePlanConfiguration.leaveAccrual.ToPreviousHalfDay)
                    {
                        if (fractionValue < 0.5m)
                            availableLeaves = (decimal)integralValue;
                        else if (fractionValue > 0.5m && fractionValue <= 0.9m)
                            availableLeaves = (decimal)integralValue + 0.5m;
                    }
                }
            }
            return availableLeaves;
        }

        private async Task<bool> DoesLeaveExpired()
        {
            bool flag = false;
            if (_leavePlanConfiguration.leaveAccrual.DoesLeaveExpireAfterSomeTime)
            {
                // restrict on expiry
                var days = now.Day - _leavePlanConfiguration.leaveAccrual.LeaveDistributionAppliedFrom;

                if (days <= _leavePlanConfiguration.leaveAccrual.AfterHowManyDays)
                    flag = true;
            }

            return await Task.FromResult(flag);
        }
    }
}
