using BottomhalfCore.Services.Interface;
using ModalLayer.Modal;
using ModalLayer.Modal.Accounts;
using ModalLayer.Modal.Leaves;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
        private DateTime now;

        public Accrual(ITimezoneConverter timezoneConverter,
            CurrentSession currentSession)
        {
            _timezoneConverter = timezoneConverter;
            _currentSession = currentSession;
        }

        public async Task CalculateLeaveAccrual(LeaveCalculationModal leaveCalculationModal, LeavePlanType leavePlanType)
        {
            _leavePlanConfiguration = leaveCalculationModal.leavePlanConfiguration;
            now = leaveCalculationModal.presentDate;

            var leaveLimit = _leavePlanConfiguration.leaveDetail.LeaveLimit;
            if (leaveLimit > 0)
            {
                decimal availableLeave = 0;
                switch (leaveCalculationModal.employeeType)
                {
                    case 1:
                        availableLeave = await CalculateLeaveForProbation(leaveCalculationModal);
                        break;
                    case 2:
                        availableLeave = await CalculateLeaveForNotice(leaveCalculationModal);
                        break;
                    default:
                        availableLeave = await ExecuteLeaveAccrualDetail(leaveCalculationModal);
                        break;
                }

                leavePlanType.AvailableLeave = LeaveLimitForCurrentType(leavePlanType.LeavePlanTypeId, availableLeave, leaveCalculationModal);


                // round up decimal value of available as per rule defined
                leavePlanType.AvailableLeave = RoundUpTheLeaves(leavePlanType.AvailableLeave);

                // check leave expiry
                leavePlanType.AvailableLeave = UpdateLeaveIfSetForExpiry(leavePlanType.AvailableLeave);

                /* -------------------------------------  this condition will be use for attendance -------------------------

                // check weekoff as absent if rule applicable
                CheckWeekOffRuleApplicable(perMonthLeaves);

                // check weekoff as absent if rule applicable
                CheckHolidayRuleApplicable(perMonthLeaves);

                -----------------------------------------------------------------------------------------------------------*/


                // check weather can apply for leave based on future date projected balance
                // availableLeaveLimit = CheckAddExtraAccrualLeaveBalance(perMonthLeaves, availableLeaveLimit);
            }
            //}
        }

        private void CanApplyEntireLeave(LeaveCalculationModal leaveCalculationModal, LeavePlanType leaveType)
        {
            if (leaveCalculationModal.leavePlan.CanApplyEntireLeave)
            {
                if ((leaveType.AvailableLeave - leaveCalculationModal.totalNumOfLeaveApplied) <= leaveType.MaxLeaveLimit)
                    leaveType.AvailableLeave = leaveType.AvailableLeave - leaveCalculationModal.totalNumOfLeaveApplied;
                else
                    throw new HiringBellException("Can't apply more then your maximun leave limit.");
            }
            else
            {
                throw new HiringBellException("You don't have enough balance to apply this leave.");
            }
        }

        private async Task<decimal> CalculateLeaveForProbation(LeaveCalculationModal leaveCalculationModal)
        {
            // Check this rule if applying leave
            // NewJoineeIsEligibleForThisLeave(leaveCalculationModal);
            var perMonthDays = _leavePlanConfiguration.leaveDetail.LeaveLimit / 12m;
            decimal leaveCount = await ProbationMonthlyAccrualCalculation(leaveCalculationModal, perMonthDays);
            return leaveCount;
        }

        private async Task<decimal> CalculateLeaveForNotice(LeaveCalculationModal leaveCalculationModal)
        {
            decimal leaveCount = CalculateLeaveDetailOnNotice(leaveCalculationModal);
            return await Task.FromResult(leaveCount);
        }

        private async Task<decimal> ExecuteLeaveAccrualDetail(LeaveCalculationModal leaveCalculationModal)
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
                        availableLeaveLimit = await MonthlyAccrualCalculation(leaveCalculationModal, leaveFrequencyForDefinedPeriod);
                    }
                    else
                    {
                        leaveFrequencyForDefinedPeriod = _leavePlanConfiguration.leaveDetail.LeaveLimit / 12.0m;
                        availableLeaveLimit = _leavePlanConfiguration.leaveDetail.LeaveLimit;
                    }
                    break;
                case "2":
                    leaveFrequencyForDefinedPeriod = _leavePlanConfiguration.leaveDetail.LeaveLimit / 4.0m;
                    availableLeaveLimit = CalculateWhenAccralQuaterly(leaveCalculationModal, leaveFrequencyForDefinedPeriod);
                    break;
                case "3":
                    leaveFrequencyForDefinedPeriod = _leavePlanConfiguration.leaveDetail.LeaveLimit / 2.0m;
                    availableLeaveLimit = CalculateWhenAccralYearly(leaveCalculationModal, leaveFrequencyForDefinedPeriod);
                    break;
            }

            if (leaveCalculationModal.employeeType == 1)
            {
                // calculate leave for experienced people and allowed accrual in probation
                decimal extraLeaveLimit = LeaveAccrualForExperienceInProbation(leaveFrequencyForDefinedPeriod);
                availableLeaveLimit = availableLeaveLimit + extraLeaveLimit;
            }

            return await Task.FromResult(availableLeaveLimit);
        }

        private decimal LeaveAccrualForExperienceInProbation(decimal perMonthLeaves)
        {
            decimal days = 0;
            if (_leavePlanConfiguration.leaveAccrual.IsVaryOnProbationOrExprience)
                days = LeaveAccrualInProbationForExperienced(perMonthLeaves);

            return days;
        }

        private decimal LeaveAccrualInProbationForExperienced(decimal perMonthLeaves)
        {
            return 0;
        }

        private int GetHalflyInYear(DateTime dateTime)
        {
            if (dateTime.Month <= 6)
                return 1;

            return 2;
        }

        private decimal CalculateWhenAccralYearly(LeaveCalculationModal leaveCalculationModal, decimal PerHalflyLeave)
        {
            decimal availableLeaves = 0;
            var currentHalfly = GetHalflyInYear(DateTime.Now);
            var currentHalflyStartMonth = 1;
            if (currentHalfly > 1)
                currentHalflyStartMonth = 6;

            if (_leavePlanConfiguration.leaveAccrual.LeaveDistributionAppliedFrom > DateTime.UtcNow.Day && DateTime.UtcNow.Month == currentHalflyStartMonth)
                currentHalfly = currentHalfly - 1;

            if (leaveCalculationModal.employee.CreatedOn.Year != DateTime.Now.Year)
                availableLeaves = PerHalflyLeave * currentHalfly;
            else
            {
                var halfly = currentHalfly - GetHalflyInYear(leaveCalculationModal.employee.CreatedOn) + 1;
                availableLeaves = halfly * PerHalflyLeave;
            }

            return availableLeaves;
        }

        private decimal CalculateWhenAccralQuaterly(LeaveCalculationModal leaveCalculationModal, decimal PerQuarterLeave)
        {
            decimal availableLeaves = 0;
            var currentQuarter = GetQuarterInYear(DateTime.Now);
            var currentQuraterStartMonth = 1;
            switch (currentQuarter)
            {
                case 2:
                    currentQuraterStartMonth = 4;
                    break;
                case 3:
                    currentQuraterStartMonth = 7;
                    break;
                case 4:
                    currentQuraterStartMonth = 10;
                    break;
            }
            if (_leavePlanConfiguration.leaveAccrual.LeaveDistributionAppliedFrom > DateTime.UtcNow.Day && DateTime.UtcNow.Month == currentQuraterStartMonth)
                currentQuarter = currentQuarter - 1;

            if (leaveCalculationModal.employee.CreatedOn.Year != DateTime.Now.Year)
                availableLeaves = PerQuarterLeave * currentQuarter;
            else
            {
                var qurater = currentQuarter - GetQuarterInYear(leaveCalculationModal.employee.CreatedOn) + 1;
                availableLeaves = qurater * PerQuarterLeave;
            }

            return availableLeaves;
        }

        private int GetQuarterInYear(DateTime dateTime)
        {
            if (dateTime.Month <= 3)
                return 1;

            if (dateTime.Month <= 6)
                return 2;

            if (dateTime.Month <= 9)
                return 3;

            return 4;
        }

        private async Task<decimal> MonthlyAccrualCalculation(LeaveCalculationModal leaveCalculationModal, decimal perMonthLeaves)
        {
            List<Task> tasks = new List<Task>();
            decimal availableLeaves = 0;
            if (_leavePlanConfiguration.leaveAccrual.IsLeaveAccruedProrateDefined)
            {
                // Prorated accrual days on first month
                var t1 = Task.Run(() =>
                {
                    decimal leaves = 0;
                    if (leaveCalculationModal.employee.CreatedOn.Year == now.Year)
                    {
                        leaves = FirstMonthAccrualInProbation(leaveCalculationModal.employee.CreatedOn.Day);
                        if (leaves == 0)
                            leaves = perMonthLeaves;
                    }
                    else
                    {
                        leaves = MonthAccrualOnDefinedDay(leaveCalculationModal.employee.CreatedOn.Day, perMonthLeaves);
                    }
                    return leaves;
                });

                // Prorated accrual days on present month
                var t2 = Task.Run(() =>
                {
                    var leaves = MonthAccrualOnDefinedDay(now.Day, perMonthLeaves);
                    return leaves;
                });

                await Task.WhenAll(t1, t2);
                availableLeaves = t1.Result + t2.Result;
            }
            else
                availableLeaves = perMonthLeaves * 2;

            var joiningDate = leaveCalculationModal.employee.CreatedOn;
            if (now.Month - joiningDate.Month > 1)
            {
                availableLeaves += perMonthLeaves * (now.Month - 2);
            }

            return availableLeaves;
        }

        private decimal MonthAccrualOnDefinedDay(int DayOfMonth, decimal perMonthDays)
        {
            decimal accrualedLeaves = 0;
            if (_leavePlanConfiguration.leaveAccrual.LeaveDistributionRateOnStartOfPeriod != null)
            {
                AllocateTimeBreakup allocateTimeBreakup = null;
                int i = 0;
                while (i < _leavePlanConfiguration.leaveAccrual.LeaveDistributionRateOnStartOfPeriod.Count)
                {
                    allocateTimeBreakup = _leavePlanConfiguration.leaveAccrual
                                            .LeaveDistributionRateOnStartOfPeriod.ElementAt(i);
                    if (DayOfMonth >= allocateTimeBreakup.FromDate)
                        accrualedLeaves += allocateTimeBreakup.AllocatedLeave;
                    i++;
                }
            }

            if (accrualedLeaves == 0)
                accrualedLeaves = perMonthDays;

            return accrualedLeaves;
        }

        private decimal FirstMonthAccrualInProbation(int JoiningDay)
        {
            decimal accrualedLeaves = 0;
            //_leavePlanConfiguration.leaveAccrual.IsLeavesProratedForJoinigMonth
            if (_leavePlanConfiguration.leaveAccrual.JoiningMonthLeaveDistribution != null &&
                _leavePlanConfiguration.leaveAccrual.JoiningMonthLeaveDistribution.Count > 0)
            {
                AllocateTimeBreakup allocateTimeBreakup = null;
                int i = 0;
                while (i < _leavePlanConfiguration.leaveAccrual.JoiningMonthLeaveDistribution.Count)
                {
                    allocateTimeBreakup = _leavePlanConfiguration.leaveAccrual
                                            .JoiningMonthLeaveDistribution.ElementAt(i);
                    if (allocateTimeBreakup.ToDate >= JoiningDay)
                        accrualedLeaves += allocateTimeBreakup.AllocatedLeave;
                    i++;
                }
            }
            else if (_leavePlanConfiguration.leaveAccrual.LeaveDistributionRateOnStartOfPeriod != null)
            {
                AllocateTimeBreakup allocateTimeBreakup = null;
                int i = 0;
                while (i < _leavePlanConfiguration.leaveAccrual.LeaveDistributionRateOnStartOfPeriod.Count)
                {
                    allocateTimeBreakup = _leavePlanConfiguration.leaveAccrual
                                            .LeaveDistributionRateOnStartOfPeriod.ElementAt(i);
                    if (allocateTimeBreakup.ToDate >= JoiningDay)
                        accrualedLeaves += allocateTimeBreakup.AllocatedLeave;
                    i++;
                }
            }

            return accrualedLeaves;
        }

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

        private async Task<decimal> ProbationMonthlyAccrualCalculation(LeaveCalculationModal leaveCalculationModal, decimal perMonthLeaves)
        {
            List<Task> tasks = new List<Task>();
            decimal availableLeaves = 0;
            if (!_leavePlanConfiguration.leaveAccrual.IsLeavesProratedForJoinigMonth)
            {
                // Prorated accrual days on first month
                var t1 = Task.Run(() =>
                {
                    var leaves = FirstMonthAccrualInProbation(leaveCalculationModal.employee.CreatedOn.Day);
                    return leaves;
                });

                // Prorated accrual days on present month
                var t2 = Task.Run(() =>
                {
                    decimal leaves = 0;
                    if (leaveCalculationModal.employee.CreatedOn.Month != now.Month)
                        leaves = MonthAccrualOnDefinedDay(now.Day, perMonthLeaves);
                    return leaves;
                });

                await Task.WhenAll(t1, t2);
                availableLeaves = t1.Result + t2.Result;
            }
            else
            {
                if (leaveCalculationModal.employee.CreatedOn.Month == now.Month)
                {
                    availableLeaves = FirstMonthAccrualInProbation(leaveCalculationModal.employee.CreatedOn.Day);
                }
                else
                {
                    availableLeaves = await MonthlyAccrualCalculation(leaveCalculationModal, perMonthLeaves);
                    if (availableLeaves == 0)
                        availableLeaves = perMonthLeaves * 2;
                }
            }

            var joiningDate = leaveCalculationModal.employee.CreatedOn;
            if (now.Month - joiningDate.Month > 1)
            {
                availableLeaves += perMonthLeaves * (now.Month - 2);
            }

            return availableLeaves;
        }

        private decimal LeaveLimitForCurrentType(int leavePlanTypeId, decimal availableLeaves, LeaveCalculationModal leaveCalculationModal)
        {
            decimal alreadyAppliedLeave = 0;

            if (!string.IsNullOrEmpty(leaveCalculationModal.leaveRequestDetail.LeaveDetail))
            {
                List<CompleteLeaveDetail> completeLeaveDetails = JsonConvert.DeserializeObject<List<CompleteLeaveDetail>>(leaveCalculationModal.leaveRequestDetail.LeaveDetail);
                if (completeLeaveDetails.Count > 0)
                {
                    alreadyAppliedLeave = completeLeaveDetails
                        .FindAll(x => x.LeaveTypeId == leavePlanTypeId && x.LeaveStatus != (int)ItemStatus.Rejected)
                        .Sum(x => x.NumOfDays);

                    leaveCalculationModal.lastApprovedLeaveDetail = completeLeaveDetails
                        .Where(x => x.LeaveStatus != (int)ItemStatus.Rejected)
                        .OrderByDescending(x => x.LeaveToDay).FirstOrDefault();
                }

            }

            alreadyAppliedLeave = availableLeaves - alreadyAppliedLeave;
            return alreadyAppliedLeave;
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

        private decimal CalculateLeaveDetailOnNotice(LeaveCalculationModal leaveCalculationModal)
        {
            if (_leavePlanConfiguration.leaveDetail.LeaveLimit <= 0)
                return 0;

            // this condition will be check at very begning of Accrual calculation.
            // decimal availableLeaveLimit = LeaveBalanceIfEntireLeaveAllowed();
            decimal availableLeaveLimit = 0;
            decimal leaveFrequencyForDefinedPeriod = 0;
            var leaveDistributedSeq = _leavePlanConfiguration.leaveAccrual.LeaveDistributionSequence;
            switch (leaveDistributedSeq)
            {
                default:
                    leaveFrequencyForDefinedPeriod = _leavePlanConfiguration.leaveDetail.LeaveLimit / 12.0m;
                    availableLeaveLimit = CalculateWhenAccralMonthly(leaveCalculationModal, leaveFrequencyForDefinedPeriod);
                    break;
                case "2":
                    leaveFrequencyForDefinedPeriod = _leavePlanConfiguration.leaveDetail.LeaveLimit / 4.0m;
                    availableLeaveLimit = this.CalculateWhenAccralQuaterly(leaveCalculationModal, leaveFrequencyForDefinedPeriod);
                    break;
                case "3":
                    leaveFrequencyForDefinedPeriod = _leavePlanConfiguration.leaveDetail.LeaveLimit / 2.0m;
                    availableLeaveLimit = this.CalculateWhenAccralYearly(leaveCalculationModal, leaveFrequencyForDefinedPeriod);
                    break;
            }

            if (leaveCalculationModal.employeeType == 1)
            {
                // calculate leave for experienced people and allowed accrual in probation
                decimal extraLeaveLimit = LeaveAccrualForExperienceInProbation(leaveFrequencyForDefinedPeriod);
                availableLeaveLimit = availableLeaveLimit + extraLeaveLimit;
            }

            /* -------------------------------------  this condition will be use for attendance -------------------------
            
            // check weekoff as absent if rule applicable
            CheckWeekOffRuleApplicable(perMonthLeaves);

            // check weekoff as absent if rule applicable
            CheckHolidayRuleApplicable(perMonthLeaves);
            
            -----------------------------------------------------------------------------------------------------------*/


            // check weather can apply for leave based on future date projected balance
            // availableLeaveLimit = CheckAddExtraAccrualLeaveBalance(perMonthLeaves, availableLeaveLimit);

            // round up decimal value of available as per rule defined
            availableLeaveLimit = RoundUpTheLeaves(availableLeaveLimit);

            // check leave expiry
            availableLeaveLimit = UpdateLeaveIfSetForExpiry(availableLeaveLimit);

            return availableLeaveLimit;
        }

        private decimal CalculateWhenAccralMonthly(LeaveCalculationModal leaveCalculationModal, decimal perMonthLeaves)
        {
            decimal availableLeaves = 0;

            // will return leave accrualed till now for the present month,
            // this will be overridden if any leave distribution rule is defined in below code

            if (_leavePlanConfiguration.leaveAccrual.IsLeavesProratedOnNotice ||
                leaveCalculationModal.employeeType == 0)
            {
                availableLeaves = LeavesOnProratedCalculation(leaveCalculationModal, perMonthLeaves);
            }
            else if (leaveCalculationModal.employeeType == 2) // serving notice period
            {
                if (_leavePlanConfiguration.leaveAccrual.IsNotAllowProratedOnNotice)
                    availableLeaves = NoticePeriodDistinctLeavesCalculation();
            }
            else if (leaveCalculationModal.employeeType == 1) // in probation
            {
                if (!_leavePlanConfiguration.leaveAccrual.IsLeavesProratedOnNotice)
                    availableLeaves = ProbationPeriodDistinctLeavesCalculation();
            }

            return availableLeaves;
        }

        // Prorate distribution of leave if employee is serving probation period
        public decimal ProbationPeriodDistinctLeavesCalculation()
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
                        days += allocateTimeBreakup.AllocatedLeave;

                    i++;
                }
            }

            return days;
        }

        // Prorate distribution of leave if employee is serving notice period
        public decimal NoticePeriodDistinctLeavesCalculation()
        {
            // steps to calculate leave if serving notice
            // calculate all leave till notice month i.e. if given notice on 10th of June
            // then calculate leave from Jan to May
            // if leave is calcuated by distribution then calculate for June and other month 
            // else depending on notice date divide days 

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
                        days += allocateTimeBreakup.AllocatedLeave;

                    i++;
                }
            }

            return days;
        }

        private decimal LeavesOnProratedCalculation(LeaveCalculationModal leaveCalculationModal, decimal perMonthLeaves)
        {
            decimal presentMonthLeaves = 0;
            decimal availableLeaves = 0;
            if (_leavePlanConfiguration.leaveAccrual.IsLeaveAccruedProrateDefined)
                presentMonthLeaves = DistinctLeavesCalculation(now.Day, leaveCalculationModal.employee.CreatedOn);
            else if (DoesPresentMonthProrateEnabled())
                presentMonthLeaves = perMonthLeaves;

            int monthCount = GetNumOfMonthExceptFirstAndLast(leaveCalculationModal);
            if (leaveCalculationModal.employee.CreatedOn.Year == now.Year)
            {
                decimal firstMonthCalculation = CalculateFirstMonthProrateCount(perMonthLeaves, leaveCalculationModal.employee);
                availableLeaves = (monthCount * perMonthLeaves) + firstMonthCalculation + presentMonthLeaves;
            }
            else
            {
                availableLeaves = (monthCount + 1) * perMonthLeaves + presentMonthLeaves;
            }

            return availableLeaves;
        }

        private decimal CalculateFirstMonthProrateCount(decimal perMonthLeaves, Employee employee)
        {
            decimal firstMonthProrateCount = 0;
            if (_leavePlanConfiguration.leaveAccrual.IsLeavesProratedForJoinigMonth)
            {
                if (employee.CreatedOn.Day >= 15)
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

        private int GetNumOfMonthExceptFirstAndLast(LeaveCalculationModal leaveCalculationModal)
        {
            int remaningMonths = 0;
            int firstMonth = 1;
            int lastMonth = GetLeaveProjectedLastMonth(leaveCalculationModal);

            if (leaveCalculationModal.employee.CreatedOn.Year == now.Year)
                firstMonth = leaveCalculationModal.employee.CreatedOn.Month;

            if (lastMonth - firstMonth >= 2)
                remaningMonths = lastMonth - firstMonth - 1;

            return remaningMonths;
        }

        private int GetLeaveProjectedLastMonth(LeaveCalculationModal leaveCalculationModal)
        {
            int month = 0;
            if (_leavePlanConfiguration.leaveAccrual.CanApplyForFutureDate)
                month = leaveCalculationModal.fromDate.Month;
            else
                month = now.Month;

            return month;
        }

        private bool DoesPresentMonthProrateEnabled()
        {
            if (_leavePlanConfiguration.leaveAccrual.LeaveDistributionAppliedFrom - now.Day <= 0)
                // if present month date is less then or equal to the date define for accrual
                return true;
            else
                // if present month date is greather than to the date define for accrual
                return false;
        }

        public decimal DistinctLeavesCalculation(int PresentMonthDay, DateTime FirstMonthDate)
        {
            decimal days = 0;
            decimal firstMonthDays = 0;
            if (_leavePlanConfiguration.leaveAccrual.LeaveDistributionRateOnStartOfPeriod != null
                && _leavePlanConfiguration.leaveAccrual.LeaveDistributionRateOnStartOfPeriod.Count > 0)
            {
                AllocateTimeBreakup allocateTimeBreakup = null;
                int i = 0;
                while (i < _leavePlanConfiguration.leaveAccrual.LeaveDistributionRateOnStartOfPeriod.Count)
                {
                    allocateTimeBreakup = _leavePlanConfiguration.leaveAccrual
                                            .LeaveDistributionRateOnStartOfPeriod.ElementAt(i);
                    if (PresentMonthDay >= allocateTimeBreakup.FromDate)
                        days += allocateTimeBreakup.AllocatedLeave;

                    if (FirstMonthDate.Year == now.Year)
                    {
                        if (FirstMonthDate.Month != now.Month)
                            if (FirstMonthDate.Day <= allocateTimeBreakup.ToDate)
                                firstMonthDays += allocateTimeBreakup.AllocatedLeave;
                    }
                    else
                    {
                        firstMonthDays += allocateTimeBreakup.AllocatedLeave;
                    }

                    i++;
                }
            }

            return (days + firstMonthDays);
        }

        private decimal UpdateLeaveIfSetForExpiry(decimal availableLeaves)
        {
            if (_leavePlanConfiguration.leaveAccrual.DoesLeaveExpireAfterSomeTime)
            {
            }
            return availableLeaves;
        }
    }
}
