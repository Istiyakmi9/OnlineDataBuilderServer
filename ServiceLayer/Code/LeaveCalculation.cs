using BottomhalfCore.DatabaseLayer.Common.Code;
using BottomhalfCore.Services.Code;
using BottomhalfCore.Services.Interface;
using ModalLayer.Modal;
using ModalLayer.Modal.Accounts;
using ModalLayer.Modal.Leaves;
using Newtonsoft.Json;
using NUnit.Framework.Constraints;
using ServiceLayer.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TimeZoneConverter;

namespace ServiceLayer.Code
{
    public class LeaveCalculation : ILeaveCalculation
    {
        private readonly IDb _db;
        private LeavePlanConfiguration _leavePlanConfiguration;
        private readonly DateTime now = DateTime.UtcNow;
        private CompanySetting _companySetting;

        private readonly ITimezoneConverter _timezoneConverter;
        private readonly CurrentSession _currentSession;
        private readonly CancellationTokenSource cts = new CancellationTokenSource();

        public LeaveCalculation(IDb db, ITimezoneConverter timezoneConverter, CurrentSession currentSession)
        {
            _db = db;
            _timezoneConverter = timezoneConverter;
            _currentSession = currentSession;
        }

        #region ACCRUAL IF IN PROBATION

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

        // Prorate distribution of leave if rule is defined

        public decimal FirstMonthAccrualInProbation(int JoiningDay)
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

        public decimal MonthAccrualOnDefinedDay(int DayOfMonth, decimal perMonthDays)
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
                    availableLeaves = MonthAccrualOnDefinedDay(leaveCalculationModal.employee.CreatedOn.Day, perMonthLeaves);
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

        #endregion


        #region LEAVE ACCRUAL CYCLE AUTOMATED

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

        private async Task<decimal> ExecuteLeaveAccrualDetail(LeaveCalculationModal leaveCalculationModal)
        {
            decimal availableLeaveLimit = 0;
            decimal leaveFrequencyForDefinedPeriod = 0;
            var leaveDistributedSeq = _leavePlanConfiguration.leaveAccrual.LeaveDistributionSequence;
            switch (leaveDistributedSeq)
            {
                default:
                    if (_leavePlanConfiguration.leaveAccrual.IsLeaveAccruedPatternAvail)
                        leaveFrequencyForDefinedPeriod = _leavePlanConfiguration.leaveDetail.LeaveLimit / 12.0m;
                    else
                        leaveFrequencyForDefinedPeriod = _leavePlanConfiguration.leaveDetail.LeaveLimit;
                    availableLeaveLimit = await this.MonthlyAccrualCalculation(leaveCalculationModal, leaveFrequencyForDefinedPeriod);
                    break;
                case "2":
                    availableLeaveLimit = this.CalculateWhenAccralQuaterly();
                    break;
                case "3":
                    availableLeaveLimit = this.CalculateWhenAccralYearly();
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

        public void RunLeaveCalculationCycle()
        {
            bool TestFlag = false;
            List<Company> companies = _db.GetList<Company>("sp_company_get");
            int i = 0;
            while (i < companies.Count)
            {
                LeaveCalculationModal leaveCalculationModal = null;
                // get employee detail and store it in class level variable

                int PageIndex = 0;

                List<Employee> employees = _db.GetList<Employee>("SP_Employee_GetAll", new
                {
                    SearchString = " 1=1 ",
                    SortBy = "",
                    PageIndex = ++PageIndex,
                    PageSize = 1
                });

                Parallel.ForEach(employees, async e =>
                {
                    leaveCalculationModal = new LeaveCalculationModal();
                    LoadCalculationData(e.EmployeeUid, leaveCalculationModal);
                    leaveCalculationModal.fromDate = DateTime.UtcNow;

                    if (TestFlag)
                        return;
                    TestFlag = true;

                    int k = 0;
                    while (k < leaveCalculationModal.leavePlanTypes.Count)
                    {
                        ValidateAndGetLeavePlanConfiguration(leaveCalculationModal.leavePlanTypes[k]);
                        leaveCalculationModal.employee = e;

                        // Check employee is in probation period
                        CheckForProbationPeriod(leaveCalculationModal);

                        // Check employee is in notice period
                        CheckForNoticePeriod(leaveCalculationModal);

                        await RunEmployeeLeaveAccrualCycle(leaveCalculationModal, leaveCalculationModal.leavePlanTypes[k]);
                    }
                });

                i++;
            }
        }

        public async Task RunEmployeeLeaveAccrualCycle(LeaveCalculationModal leaveCalculationModal, LeavePlanType leavePlanType)
        {
            //var planType = Enumerable.FirstOrDefault<EmployeeLeaveQuota>(
            //    leaveCalculationModal.leaveRequestDetail.EmployeeLeaveQuotaDetail,
            //    (Func<EmployeeLeaveQuota, bool>)(x => x.LeavePlanTypeId == leavePlanType.LeavePlanTypeId));

            //if (planType != null && planType.AvailableLeave > 0)
            //{
            //    leavePlanType.AvailableLeave = planType.AvailableLeave;
            //}
            //else
            //{

            var leaveLimit = _leavePlanConfiguration.leaveDetail.LeaveLimit;
            if (leaveLimit > 0)
            {
                switch (leaveCalculationModal.employeeType)
                {
                    case 1:
                        leavePlanType.AvailableLeave = await CalculateLeaveForProbation(leaveCalculationModal);
                        break;
                    case 2:
                        leavePlanType.AvailableLeave = CalculateLeaveForNotice(leaveCalculationModal);
                        break;
                    default:
                        decimal availableLeave = await ExecuteLeaveAccrualDetail(leaveCalculationModal);
                        leavePlanType.AvailableLeave = LeaveLimitForCurrentType(leavePlanType.LeavePlanTypeId, availableLeave, leaveCalculationModal);
                        break;
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
                leavePlanType.AvailableLeave = RoundUpTheLeaves(leavePlanType.AvailableLeave);

                // check leave expiry
                leavePlanType.AvailableLeave = UpdateLeaveIfSetForExpiry(leavePlanType.AvailableLeave);
            }
            //}
        }

        #endregion


        #region LEAVE ACCRUAL

        private decimal LeaveAccrualForExperienceInProbation(decimal perMonthLeaves)
        {
            decimal days = 0;
            if (_leavePlanConfiguration.leaveAccrual.IsVaryOnProbationOrExprience)
                days = LeaveAccrualInProbationForExperienced(perMonthLeaves);

            return days;
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

        private bool DoesPresentMonthProrateEnabled()
        {
            if (_leavePlanConfiguration.leaveAccrual.LeaveDistributionAppliedFrom - now.Day <= 0)
                // if present month date is less then or equal to the date define for accrual
                return true;
            else
                // if present month date is greather than to the date define for accrual
                return false;
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

        private decimal LeavesOnProratedCalculation(LeaveCalculationModal leaveCalculationModal, decimal perMonthLeaves)
        {
            decimal presentMonthLeaves = 0;
            decimal availableLeaves = 0;
            if (_leavePlanConfiguration.leaveAccrual.IsLeaveAccruedProrateDefined)
                presentMonthLeaves = DistinctLeavesCalculation(now.Day, leaveCalculationModal.employee.CreatedOn);
            else if (DoesPresentMonthProrateEnabled())
                presentMonthLeaves = perMonthLeaves;

            int mounthCount = GetNumOfMonthExceptFirstAndLast(leaveCalculationModal);
            if (leaveCalculationModal.employee.CreatedOn.Year == now.Year)
            {
                decimal firstMonthCalculation = CalculateFirstMonthProrateCount(perMonthLeaves, leaveCalculationModal.employee);
                availableLeaves = (mounthCount * perMonthLeaves) + firstMonthCalculation + presentMonthLeaves;
            }
            else
            {
                availableLeaves = (mounthCount + 1) * perMonthLeaves + presentMonthLeaves;
            }

            return availableLeaves;
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

        private decimal CalculateWhenAccralQuaterly()
        {
            decimal availableLeaves = 0;

            return availableLeaves;
        }

        private decimal CalculateWhenAccralYearly()
        {
            decimal availableLeaves = 0;

            return availableLeaves;
        }

        private decimal LeaveAccrualInProbationForExperienced(decimal perMonthLeaves)
        {
            return 0;
        }

        #endregion

        private async Task<decimal> CalculateLeaveForProbation(LeaveCalculationModal leaveCalculationModal)
        {
            // Check this rule if applying leave
            // NewJoineeIsEligibleForThisLeave(leaveCalculationModal);
            var perMonthDays = _leavePlanConfiguration.leaveDetail.LeaveLimit / 12m;
            decimal leaveCount = await ProbationMonthlyAccrualCalculation(leaveCalculationModal, perMonthDays);
            return leaveCount;
        }

        private decimal CalculateLeaveForNotice(LeaveCalculationModal leaveCalculationModal)
        {
            decimal leaveCount = CalculateLeaveDetailOnNotice(leaveCalculationModal);
            return leaveCount;
        }

        private decimal LeaveLimitBeyondQuota() { return 0; }

        public bool CanReportingManagerAwardCasualCredits() { return false; }

        public decimal LeaveAccruedTillDate()
        {
            return 0;
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
                    if (leaveCalculationModal.fromDate.Year == now.Year && leaveCalculationModal.fromDate.Month == now.Month && _companySetting.IsUseInternationalWeekDays)
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

        private decimal CheckHolidayRuleApplicable(LeaveCalculationModal leaveCalculationModal)
        {
            int totalWeekEnds = 0;
            // check two condition here
            // 1. WeekOffAsAbsentIfAttendaceLessThen must be greater then 0
            // 2. Employee should not exceed leave or absent for the present month more then WeekOffAsAbsentIfAttendaceLessThen value.
            if (_leavePlanConfiguration.leaveAccrual.WeekOffAsAbsentIfAttendaceLessThen > 0 /* && [Put 2nd condition here]*/)
            {
                if (leaveCalculationModal.fromDate.Year == now.Year && leaveCalculationModal.fromDate.Month == now.Month)
                {
                    // get holiday list for present month and then put logic here.
                }
            }

            return totalWeekEnds;
        }

        private bool CheckIfAlreadyOnLeaveForMoreThen(decimal leaveCount)
        {
            return true;
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
                    integralValue = Convert.ToInt32(availableLeaves);
                    if (_leavePlanConfiguration.leaveAccrual.ToNearestHalfDay)
                    {
                        if (fractionValue >= 0.1m && fractionValue <= 0.2m)
                            availableLeaves = (decimal)integralValue;
                        else if (fractionValue > 0.2m && fractionValue <= 0.5m)
                            availableLeaves += (decimal)integralValue + 0.5m;
                        else if (fractionValue < 0.8m)
                            availableLeaves += (decimal)integralValue + 0.5m;
                        else
                            availableLeaves += (decimal)integralValue++;
                    }

                    if (_leavePlanConfiguration.leaveAccrual.ToNearestFullDay)
                    {
                        if (fractionValue >= 0.5m)
                            availableLeaves += (decimal)integralValue++;
                    }

                    if (_leavePlanConfiguration.leaveAccrual.ToNextAvailableHalfDay)
                    {
                        if (fractionValue >= 0.1m && fractionValue < 0.5m)
                            availableLeaves += (decimal)integralValue + 0.5m;
                        else
                            availableLeaves += (decimal)integralValue++;
                    }

                    if (_leavePlanConfiguration.leaveAccrual.ToNextAvailableFullDay)
                    {
                        if (fractionValue >= 0.1m)
                            availableLeaves += (decimal)integralValue++;
                    }

                    if (_leavePlanConfiguration.leaveAccrual.ToPreviousHalfDay)
                    {
                        if (fractionValue < 0.5m)
                            availableLeaves += (decimal)integralValue;
                        else if (fractionValue > 0.5m && fractionValue <= 0.9m)
                            availableLeaves += (decimal)integralValue + 0.5m;
                    }
                }
            }
            return availableLeaves;
        }

        private decimal UpdateLeaveIfSetForExpiry(decimal availableLeaves)
        {
            if (_leavePlanConfiguration.leaveAccrual.DoesLeaveExpireAfterSomeTime)
            {
            }
            return availableLeaves;
        }

        private void CheckSameDateAlreadyApplied(List<CompleteLeaveDetail> completeLeaveDetails, LeaveCalculationModal leaveCalculationModal)
        {
            try
            {
                if (completeLeaveDetails.Count > 0)
                {
                    decimal backDayLimit = _leavePlanConfiguration.leaveApplyDetail.BackDateLeaveApplyNotBeyondDays;
                    DateTime initFilterDate = now.AddDays(Convert.ToDouble(-backDayLimit));

                    var empLeave = completeLeaveDetails
                                    .Where(x => x.LeaveFromDay.Subtract(initFilterDate).TotalDays >= 0);
                    if (empLeave.Any())
                    {
                        var startDate = leaveCalculationModal.fromDate;
                        var endDate = leaveCalculationModal.toDate;
                        Parallel.ForEach(empLeave, i =>
                        {
                            if (i.LeaveFromDay.Month == startDate.Month)
                            {
                                if (startDate.Subtract(i.LeaveFromDay).TotalDays >= 0 &&
                                    startDate.Subtract(i.LeaveToDay).TotalDays <= 0)
                                    throw new HiringBellException($"From date: {startDate} already exist in another leave request");
                            }

                            if (i.LeaveToDay.Month == endDate.Month)
                            {
                                if (endDate.Subtract(i.LeaveFromDay).TotalDays >= 0 &&
                                    endDate.Subtract(i.LeaveToDay).TotalDays <= 0)
                                    throw new HiringBellException($"To date: {endDate} already exist in another leave request");
                            }
                        });
                    }
                }
            }
            catch (AggregateException ax)
            {
                if (ax.Flatten().InnerExceptions.Count > 0)
                {
                    var hex = ax.Flatten().InnerExceptions.ElementAt(0) as HiringBellException;
                    throw hex;
                }
            }
        }

        private decimal LeaveLimitForCurrentType(int leavePlanTypeId, decimal availableLeaves, LeaveCalculationModal leaveCalculationModal)
        {
            decimal alreadyAppliedLeave = 0;

            if (!string.IsNullOrEmpty(leaveCalculationModal.leaveRequestDetail.LeaveDetail))
            {
                List<CompleteLeaveDetail> completeLeaveDetails = JsonConvert.DeserializeObject<List<CompleteLeaveDetail>>(leaveCalculationModal.leaveRequestDetail.LeaveDetail);
                if (completeLeaveDetails.Count > 0)
                {
                    alreadyAppliedLeave = completeLeaveDetails.FindAll(x => x.LeaveType == leavePlanTypeId && x.LeaveStatus != (int)ItemStatus.Rejected).Sum(x => x.NumOfDays);
                    CheckSameDateAlreadyApplied(completeLeaveDetails, leaveCalculationModal);

                    leaveCalculationModal.lastApprovedLeaveDetail = completeLeaveDetails.Where(x => x.LeaveStatus == (int)ItemStatus.Approved)
                                                                    .OrderByDescending(x => x.LeaveToDay).FirstOrDefault();
                }

            }

            alreadyAppliedLeave = availableLeaves - alreadyAppliedLeave;
            return alreadyAppliedLeave;
        }

        // check to allow entire leave at a time or leave will be add a prorated manner
        private decimal LeaveBalanceIfEntireLeaveAllowed()
        {
            decimal leaveBalance = 0;
            if (_leavePlanConfiguration.leaveAccrual.CanApplyEntireLeave)
                leaveBalance = _leavePlanConfiguration.leaveDetail.LeaveLimit;
            return leaveBalance;
        }

        private decimal CalculateLeaveDetailOnProbation(LeaveCalculationModal leaveCalculationModal)
        {
            if (_leavePlanConfiguration.leaveDetail.LeaveLimit <= 0)
                return 0;

            // this condition will be check at very begning of Accrual calculation.
            // decimal availableLeaveLimit = LeaveBalanceIfEntireLeaveAllowed();
            decimal availableLeaveLimit = 0;
            decimal leaveFrequencyForDefinedPeriod = 0;
            var isProrated = _leavePlanConfiguration.leaveAccrual.IsLeavesProratedForJoinigMonth;

            if (isProrated)
            {
                leaveFrequencyForDefinedPeriod = _leavePlanConfiguration.leaveDetail.LeaveLimit / 12.0m;
                availableLeaveLimit = this.CalculateWhenAccralMonthly(leaveCalculationModal, leaveFrequencyForDefinedPeriod);
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
                    availableLeaveLimit = this.CalculateWhenAccralMonthly(leaveCalculationModal, leaveFrequencyForDefinedPeriod);
                    break;
                case "2":
                    availableLeaveLimit = this.CalculateWhenAccralQuaterly();
                    break;
                case "3":
                    availableLeaveLimit = this.CalculateWhenAccralYearly();
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

        private void ValidateAndGetLeavePlanConfiguration(LeavePlanType leavePlanType)
        {
            // fetching data from database using leaveplantypeId
            _leavePlanConfiguration = JsonConvert.DeserializeObject<LeavePlanConfiguration>(leavePlanType.PlanConfigurationDetail);
            if (_leavePlanConfiguration == null)
                throw new HiringBellException("Leave setup/configuration is not defined. Please complete the setup/configuration first.");
        }

        private void LoadCalculationData(long EmployeeId, LeaveCalculationModal leaveCalculationModal)
        {
            var ds = _db.FetchDataSet("sp_leave_plan_calculation_get", new
            {
                EmployeeId,
                IsActive = 1,
                Year = now.Year
            }, false);

            if (ds != null && ds.Tables.Count == 5)
            {
                //if (ds.Tables[0].Rows.Count == 0 || ds.Tables[1].Rows.Count == 0 || ds.Tables[3].Rows.Count == 0)
                if (ds.Tables[0].Rows.Count == 0 || ds.Tables[1].Rows.Count == 0)
                    throw new HiringBellException("Fail to get employee related details. Please contact to admin.");

                leaveCalculationModal.employee = Converter.ToType<Employee>(ds.Tables[0]);
                leaveCalculationModal.leavePlanTypes = Converter.ToList<LeavePlanType>(ds.Tables[1]);
                leaveCalculationModal.leaveRequestDetail = Converter.ToType<LeaveRequestDetail>(ds.Tables[2]);

                if (!string.IsNullOrEmpty(leaveCalculationModal.leaveRequestDetail.LeaveQuotaDetail))
                    leaveCalculationModal.leaveRequestDetail.EmployeeLeaveQuotaDetail = JsonConvert
                        .DeserializeObject<List<EmployeeLeaveQuota>>(leaveCalculationModal.leaveRequestDetail.LeaveQuotaDetail);
                else
                    leaveCalculationModal.leaveRequestDetail.EmployeeLeaveQuotaDetail = new List<EmployeeLeaveQuota>();

                _companySetting = Converter.ToType<CompanySetting>(ds.Tables[3]);
                leaveCalculationModal.leavePlan = Converter.ToType<LeavePlan>(ds.Tables[4]);
            }
            else
                throw new HiringBellException("Employee does not exist. Please contact to admin.");
        }

        private bool IsMinimunGapBeforAffectedDate()
        {
            return false;
        }

        private bool DoesLeaveRequiredComments()
        {
            return false;
        }

        private void RequiredDocumentForExtending()
        {

        }

        private void CheckForProbationPeriod(LeaveCalculationModal leaveCalculationModal)
        {
            leaveCalculationModal.employeeType = 0;
            if ((leaveCalculationModal.employee.CreatedOn.AddDays(_companySetting.ProbationPeriodInDays))
                .Subtract(now).TotalDays > 0)
                leaveCalculationModal.employeeType = 1;
        }

        private void CheckForNoticePeriod(LeaveCalculationModal leaveCalculationModal)
        {
            if (leaveCalculationModal.employee.NoticePeriodId != 0 && leaveCalculationModal.employee.NoticePeriodAppliedOn != null)
                leaveCalculationModal.employeeType = 2;
        }

        private void LeaveEligibilityCheck(LeaveCalculationModal leaveCalculationModal)
        {
            // if future date then > 0 else < 0
            var presentDate = _timezoneConverter.ToUtcTime(DateTime.SpecifyKind(now.Date, DateTimeKind.Unspecified));
            double days = leaveCalculationModal.fromDate.Subtract(presentDate).TotalDays;
            if (days < 0) // past date
            {
                days = days * -1;
                if (_leavePlanConfiguration.leaveApplyDetail.BackDateLeaveApplyNotBeyondDays == 0 ||
                    days < _leavePlanConfiguration.leaveApplyDetail.BackDateLeaveApplyNotBeyondDays)
                    throw new HiringBellException($"Back dated leave more than {_leavePlanConfiguration.leaveApplyDetail.BackDateLeaveApplyNotBeyondDays} days can't be allowed.");
            }
            else // future date
            {
                if (days < _leavePlanConfiguration.leaveApplyDetail.ApplyPriorBeforeLeaveDate)
                    throw new HiringBellException($"Apply this leave before {_leavePlanConfiguration.leaveApplyDetail.ApplyPriorBeforeLeaveDate} days.");
            }
        }

        private void NewJoineeIsEligibleForThisLeave(LeaveCalculationModal leaveCalculationModal)
        {
            if (_leavePlanConfiguration.leavePlanRestriction.CanApplyAfterProbation)
            {
                var dateFromApplyLeave = leaveCalculationModal.employee.CreatedOn.AddDays(
                    _companySetting.ProbationPeriodInDays +
                    Convert.ToDouble(_leavePlanConfiguration.leavePlanRestriction.DaysAfterProbation));

                if (dateFromApplyLeave.Subtract(now).TotalDays < 0)
                    throw new HiringBellException("Days restriction after Probation period is not completed to apply this leave.");
            }
            else
            {
                var dateAfterProbation = leaveCalculationModal.employee.CreatedOn.AddDays(Convert.ToDouble(_leavePlanConfiguration.leavePlanRestriction.DaysAfterJoining));
                if (leaveCalculationModal.fromDate.Subtract(dateAfterProbation).TotalDays < 0)
                    throw new HiringBellException("Days restriction after Joining period is not completed to apply this leave.");
            }
        }

        private bool CheckProbationLeaveRestriction(LeavePlanType leavePlanType, LeaveCalculationModal leaveCalculationModal)
        {
            bool flag = true;
            // if employee completed probation period
            if (_leavePlanConfiguration.leavePlanRestriction.CanApplyAfterProbation && leaveCalculationModal.employeeType == 0)
            {
                var totalDays = Convert.ToDouble(_leavePlanConfiguration.leavePlanRestriction.DaysAfterProbation) +
                                leaveCalculationModal.employee.ProbationPeriodDaysLimit;

                if (leaveCalculationModal.fromDate.Subtract(leaveCalculationModal.employee.CreatedOn.AddDays(totalDays)).TotalDays < 0)
                {
                    flag = false;
                    leavePlanType.IsApplicable = false;
                    leavePlanType.Reason = $"This leave can only be utilized {_leavePlanConfiguration.leaveApplyDetail.ApplyPriorBeforeLeaveDate} days before leave start date.";
                }
            }
            else if (leaveCalculationModal.employeeType == 1) // if employee currently serving probation period
            {
                DateTime afterJoiningDate = leaveCalculationModal.employee.CreatedOn.AddDays(Convert.ToDouble(_leavePlanConfiguration.leavePlanRestriction.DaysAfterJoining));
                if (afterJoiningDate.Subtract(leaveCalculationModal.employee.CreatedOn.AddDays(leaveCalculationModal.employee.ProbationPeriodDaysLimit)).TotalDays <= 0)
                {
                    var totalNumOfDays = leaveCalculationModal.toDate.Subtract(leaveCalculationModal.fromDate).TotalDays;
                    if (_leavePlanConfiguration.leavePlanRestriction.IsAvailRestrictedLeavesInProbation)
                    {
                        if (Convert.ToDouble(_leavePlanConfiguration.leavePlanRestriction.LeaveLimitInProbation) - totalNumOfDays > 0)
                        {
                            flag = false;
                            leavePlanType.IsApplicable = false;
                            leavePlanType.Reason = $"This leave can only be utilized {_leavePlanConfiguration.leaveApplyDetail.ApplyPriorBeforeLeaveDate} days before leave start date.";
                        }
                    }
                }
            }

            return flag;
        }

        private async Task ExecuteAccrualLeaveCycle(LeaveCalculationModal leaveCalculationModal, LeavePlanType leavePlanType)
        {
            var planType = Enumerable.FirstOrDefault<EmployeeLeaveQuota>(
                leaveCalculationModal.leaveRequestDetail.EmployeeLeaveQuotaDetail,
                (Func<EmployeeLeaveQuota, bool>)(x => x.LeavePlanTypeId == leavePlanType.LeavePlanTypeId));

            if (planType != null && planType.AvailableLeave > 0)
            {
                leavePlanType.AvailableLeave = planType.AvailableLeave;
            }
            else
            {
                var leaveLimit = _leavePlanConfiguration.leaveDetail.LeaveLimit;
                if (leaveLimit > 0)
                {
                    switch (leaveCalculationModal.employeeType)
                    {
                        case 1:
                            leavePlanType.AvailableLeave = await CalculateLeaveForProbation(leaveCalculationModal);
                            break;
                        case 2:
                            leavePlanType.AvailableLeave = CalculateLeaveForNotice(leaveCalculationModal);
                            break;
                        default:
                            decimal availableLeave = CalculateLeaveDetailOnProbation(leaveCalculationModal);
                            leavePlanType.AvailableLeave = LeaveLimitForCurrentType(leavePlanType.LeavePlanTypeId, availableLeave, leaveCalculationModal);
                            break;
                    }
                }
            }
        }

        private LeaveCalculationModal GetCalculationModal(long EmployeeId, DateTime FromDate, DateTime ToDate)
        {
            var leaveCalculationModal = new LeaveCalculationModal();
            leaveCalculationModal.fromDate = FromDate;
            leaveCalculationModal.toDate = ToDate;

            // get employee detail and store it in class level variable
            LoadCalculationData(EmployeeId, leaveCalculationModal);

            // Check employee is in probation period
            CheckForProbationPeriod(leaveCalculationModal);

            // Check employee is in notice period
            CheckForNoticePeriod(leaveCalculationModal);

            return leaveCalculationModal;
        }

        public async Task<LeaveCalculationModal> GetBalancedLeave(long EmployeeId, DateTime FromDate, DateTime ToDate)
        {
            var leaveCalculationModal = GetCalculationModal(EmployeeId, FromDate, ToDate);

            LeavePlanType leavePlanType = default(LeavePlanType);
            int i = 0;
            while (i < leaveCalculationModal.leavePlanTypes.Count)
            {
                leavePlanType = leaveCalculationModal.leavePlanTypes[i];
                ValidateAndGetLeavePlanConfiguration(leavePlanType);
                await RunEmployeeLeaveAccrualCycle(leaveCalculationModal, leavePlanType);
                i++;
            }

            return leaveCalculationModal;
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

        private LeavePlanType DoesRequestedLeaveAvailable(int leavePlanTypeId, LeaveCalculationModal leaveCalculationModal)
        {
            var leaveType = leaveCalculationModal.leavePlanTypes.FirstOrDefault(x => x.LeavePlanTypeId == leavePlanTypeId);
            if (leaveType == null)
                throw new HiringBellException("Unable to find requested leave type. Please contact to admin.");

            CalculateLeaveDays(leaveCalculationModal);

            if (leaveCalculationModal.totalNumOfLeaveApplied > leaveType.AvailableLeave)
                CanApplyEntireLeave(leaveCalculationModal, leaveType);

            return leaveType;
        }

        public async Task<string> CheckAndApplyForLeave(LeaveRequestDetail leaveDetail)
        {
            try
            {
                var leaveCalculationModal = GetCalculationModal(leaveDetail.EmployeeId, leaveDetail.LeaveFromDay, leaveDetail.LeaveToDay);

                LeavePlanType leavePlanType = default(LeavePlanType);
                leavePlanType = leaveCalculationModal.leavePlanTypes.Find(x => x.LeavePlanTypeId == leaveDetail.LeaveType);
                if (leavePlanType == null)
                    throw new HiringBellException("Request leave detail not found. Please contact to admin.");

                ValidateAndGetLeavePlanConfiguration(leavePlanType);
                await RunEmployeeLeaveAccrualCycle(leaveCalculationModal, leavePlanType);

                //1. Check all leave restriction
                CheckAllRestrictionForCurrentLeaveType(leaveCalculationModal, leavePlanType.AvailableLeave);

                //2. can see and apply this leave
                AllowToSeeAndApply();

                //3. check befor and after how many days this leave can be applied for future and back dated leave
                LeaveEligibilityCheck(leaveCalculationModal);

                //4. check if leave required comments
                DoesLeaveRequiredComments();

                //5. check if required any document proof for this leave
                RequiredDocumentForExtending();

                //6. check total available leave quota till now            
                leavePlanType = DoesRequestedLeaveAvailable(leaveDetail.LeaveType, leaveCalculationModal);

                //7. save detail to employee leave request table and reaise leave request
                return await ApplyAndSaveChanges(leaveCalculationModal, leavePlanType, leaveDetail);
            }
            catch
            {
                throw;
            }
        }

        private async Task<string> ApplyAndSaveChanges(LeaveCalculationModal leaveCalculationModal, LeavePlanType leavePlanType, LeaveRequestDetail leaveDetail)
        {
            decimal totalAllocatedLeave = leaveCalculationModal.leavePlanTypes.Sum(x => x.MaxLeaveLimit);
            LeaveRequestDetail leaveRequestDetail = _db.Get<LeaveRequestDetail>("sp_employee_leave_request_GetById", new
            {
                EmployeeId = leaveCalculationModal.employee.EmployeeUid,
                Year = DateTime.Now.Year
            });

            if (leaveRequestDetail == null)
                leaveRequestDetail = new LeaveRequestDetail();
            //throw new HiringBellException("Unable to find leave request for current employee.");

            string result = string.Empty;
            await Task.Run(() =>
            {
                leaveRequestDetail.TotalLeaveApplied += leaveCalculationModal.totalNumOfLeaveApplied;
                List<CompleteLeaveDetail> leaveDetails = new List<CompleteLeaveDetail>();
                if (leaveRequestDetail.LeaveDetail != null)
                    leaveDetails = JsonConvert.DeserializeObject<List<CompleteLeaveDetail>>(leaveRequestDetail.LeaveDetail);

                CompleteLeaveDetail newLeaveDeatil = new CompleteLeaveDetail()
                {
                    EmployeeId = leaveDetail.EmployeeId,
                    EmployeeName = leaveCalculationModal.employee.FirstName + " " + leaveCalculationModal.employee.LastName,
                    AssignTo = leaveDetail.AssignTo,
                    Session = leaveDetail.Session,
                    LeaveType = leaveDetail.LeaveType,
                    LeaveFromDay = leaveDetail.LeaveFromDay,
                    LeaveToDay = leaveDetail.LeaveToDay,
                    NumOfDays = Convert.ToDecimal(leaveDetail.LeaveToDay.Subtract(leaveDetail.LeaveFromDay).TotalDays) + 1,
                    LeaveStatus = (int)ItemStatus.Pending,
                    Reason = leaveDetail.Reason,
                    RequestedOn = DateTime.UtcNow
                };

                leaveDetails.Add(newLeaveDeatil);
                //if (leaveDetails != null)
                //{
                //    CompleteLeaveDetail newLeaveDeatil = new CompleteLeaveDetail()
                //    {
                //        EmployeeId = leaveDetail.EmployeeId,
                //        EmployeeName = leaveCalculationModal.employee.FirstName + " " + leaveCalculationModal.employee.LastName,
                //        AssignTo = leaveDetail.AssignTo,
                //        Session = leaveDetail.Session,
                //        LeaveType = leaveDetail.LeaveType,
                //        LeaveFromDay = leaveDetail.LeaveFromDay,
                //        LeaveToDay = leaveDetail.LeaveToDay,
                //        NumOfDays = Convert.ToDecimal(leaveDetail.LeaveToDay.Subtract(leaveDetail.LeaveFromDay).TotalDays),
                //        LeaveStatus = (int)ItemStatus.Pending,
                //        Reason = leaveDetail.Reason,
                //        RequestedOn = DateTime.UtcNow
                //    };

                //    leaveDetails.Add(newLeaveDeatil);
                //}
                //else
                //    leaveDetails = new List<CompleteLeaveDetail>();

                leaveDetail.LeaveQuotaDetail = JsonConvert.SerializeObject(
                    leaveCalculationModal.leavePlanTypes.Select(x => new EmployeeLeaveQuota
                    {
                        LeavePlanTypeId = x.LeavePlanTypeId,
                        AvailableLeave = x.AvailableLeave
                    }));

                leaveDetail.LeaveDetail = JsonConvert.SerializeObject(leaveDetails);
                decimal numOfDays = (decimal)leaveDetail.LeaveToDay.Subtract(leaveDetail.LeaveFromDay).TotalDays;
                result = _db.Execute<LeaveRequestDetail>("sp_employee_leave_request_InsUpdate", new
                {
                    leaveDetail.LeaveRequestId,
                    leaveDetail.EmployeeId,
                    leaveDetail.LeaveDetail,
                    leaveDetail.Reason,
                    AssignTo = _currentSession.CurrentUserDetail.ReportingManagerId,
                    Year = leaveDetail.LeaveFromDay.Year,
                    leaveDetail.LeaveFromDay,
                    leaveDetail.LeaveToDay,
                    leaveDetail.LeaveType,
                    RequestStatusId = (int)ItemStatus.Pending,
                    leaveDetail.AvailableLeaves,
                    leaveRequestDetail.TotalLeaveApplied,
                    leaveDetail.TotalApprovedLeave,
                    TotalLeaveQuota = totalAllocatedLeave,
                    leaveDetail.LeaveQuotaDetail,
                    NumOfDays = numOfDays,
                    LeaveRequestNotificationId = 0
                }, true);

                if (string.IsNullOrEmpty(result))
                    throw new HiringBellException("fail to insert or update");
            });

            return result;
        }

        private void LeaveRestrictionCheck(LeavePlanType leavePlanType, LeaveCalculationModal leaveCalculationModal, decimal availableLeaveLimit)
        {
            AllowToSeeAndApply();

            // get leave quota after deducting already applied leaves
            LeaveLimitForCurrentType(leavePlanType.LeavePlanTypeId, availableLeaveLimit, leaveCalculationModal);

            // calculate num of days applied
            CalculateLeaveDays(leaveCalculationModal);

            LeaveEligibilityCheck(leaveCalculationModal);

            // check leave restriction related to probation period
            var flag = CheckProbationLeaveRestriction(leavePlanType, leaveCalculationModal);

            // get leave quota after deducting already applied leaves
            CheckAllRestrictionForCurrentLeaveType(leaveCalculationModal, availableLeaveLimit);
        }

        private void CalculateLeaveDays(LeaveCalculationModal leaveCalculationModal)
        {
            leaveCalculationModal.totalNumOfLeaveApplied = 0;
            if (_leavePlanConfiguration.leaveHolidaysAndWeekoff.AdJoiningHolidayIsConsiderAsLeave)
            {

            }

            if (!_leavePlanConfiguration.leaveHolidaysAndWeekoff.AdjoiningWeekOffIsConsiderAsLeave)
            {
                var fromDate = leaveCalculationModal.fromDate;
                while (leaveCalculationModal.toDate.Subtract(fromDate).TotalDays >= 0)
                {
                    if (fromDate.DayOfWeek != DayOfWeek.Saturday && fromDate.DayOfWeek != DayOfWeek.Sunday)
                        leaveCalculationModal.totalNumOfLeaveApplied++;

                    fromDate = fromDate.AddDays(1);
                }
            }
        }

        private void CheckAllRestrictionForCurrentLeaveType(LeaveCalculationModal leaveCalculationModal, decimal availableLeaveLimit)
        {
            // check leave gap between previous and new leave date
            if (leaveCalculationModal.lastApprovedLeaveDetail != null)
            {
                if (leaveCalculationModal.fromDate.Subtract(leaveCalculationModal.lastApprovedLeaveDetail.LeaveToDay).TotalDays <
                    Convert.ToDouble(_leavePlanConfiguration.leavePlanRestriction.GapBetweenTwoConsicutiveLeaveDates))
                    throw new HiringBellException($"Minimumn {_leavePlanConfiguration.leavePlanRestriction.GapBetweenTwoConsicutiveLeaveDates} days gap required to apply this leave");
            }

            // check total leave applied and restrict for current year
            if (_leavePlanConfiguration.leavePlanRestriction.LimitOfMaximumLeavesInCalendarYear > 0 &&
                leaveCalculationModal.totalNumOfLeaveApplied > _leavePlanConfiguration.leavePlanRestriction.LimitOfMaximumLeavesInCalendarYear)
                throw new HiringBellException($"Calendar year leave limit is only {_leavePlanConfiguration.leavePlanRestriction.LimitOfMaximumLeavesInCalendarYear} days.");

            // check total leave applied and restrict for current month
            if (_leavePlanConfiguration.leavePlanRestriction.LimitOfMaximumLeavesInCalendarMonth > 0 &&
                leaveCalculationModal.totalNumOfLeaveApplied > _leavePlanConfiguration.leavePlanRestriction.LimitOfMaximumLeavesInCalendarMonth)
                throw new HiringBellException($"Calendar month leave limit is only {_leavePlanConfiguration.leavePlanRestriction.LimitOfMaximumLeavesInCalendarMonth} days.");

            // check total leave applied and restrict for entire tenure
            if (_leavePlanConfiguration.leavePlanRestriction.LimitOfMaximumLeavesInEntireTenure > 0 &&
                leaveCalculationModal.totalNumOfLeaveApplied > _leavePlanConfiguration.leavePlanRestriction.LimitOfMaximumLeavesInEntireTenure)
                throw new HiringBellException($"Entire tenure leave limit is only {_leavePlanConfiguration.leavePlanRestriction.LimitOfMaximumLeavesInEntireTenure} days.");

            // check available if any restrict minimun leave to be appllied
            if (availableLeaveLimit >= _leavePlanConfiguration.leavePlanRestriction.AvailableLeaves &&
                leaveCalculationModal.totalNumOfLeaveApplied < _leavePlanConfiguration.leavePlanRestriction.MinLeaveToApplyDependsOnAvailable)
                throw new HiringBellException($"Minimun {_leavePlanConfiguration.leavePlanRestriction.AvailableLeaves} days of leave only allowed for this type.");

            // restrict leave date every month
            if (_leavePlanConfiguration.leavePlanRestriction.RestrictFromDayOfEveryMonth > 0 &&
                leaveCalculationModal.fromDate.Day <= _leavePlanConfiguration.leavePlanRestriction.RestrictFromDayOfEveryMonth)
                throw new HiringBellException($"Apply this leave before {_leavePlanConfiguration.leavePlanRestriction.RestrictFromDayOfEveryMonth} day of any month.");
        }

        public async Task<bool> CanApplyForHalfDay(long EmployeeId)
        {
            return await Task.Run<bool>(() =>
            {
                return false;
            }, cts.Token);
        }

        public void AllowToSeeAndApply()
        {
            if (!_leavePlanConfiguration.leaveApplyDetail.EmployeeCanSeeAndApplyCurrentPlanLeave)
                throw new HiringBellException("You don't have enough permission to apply this leave.");
        }

        public async Task<bool> CanApplyBackDatedLeave(long EmployeeId)
        {
            return await Task.Run<bool>(() =>
            {
                return false;
            }, cts.Token);
        }
    }
}