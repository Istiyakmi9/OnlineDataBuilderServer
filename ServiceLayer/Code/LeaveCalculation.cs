using BottomhalfCore.DatabaseLayer.Common.Code;
using BottomhalfCore.Services.Code;
using BottomhalfCore.Services.Interface;
using ModalLayer.Modal;
using ModalLayer.Modal.Accounts;
using ModalLayer.Modal.Leaves;
using Newtonsoft.Json;
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
        private Leave _employeeLeaveDetail;
        private List<LeavePlanType> _leavePlanTypes;
        private LeavePlanConfiguration _leavePlanConfiguration;
        private readonly DateTime now = DateTime.UtcNow;
        private CompanySetting _companySetting;

        private LeaveCalculationModal _leaveCalculationModal = default(LeaveCalculationModal);
        private readonly ITimezoneConverter _timezoneConverter;
        private readonly CurrentSession _currentSession;
        private readonly CancellationTokenSource cts = new CancellationTokenSource();

        public LeaveCalculation(IDb db, ITimezoneConverter timezoneConverter, CurrentSession currentSession)
        {
            _db = db;
            _timezoneConverter = timezoneConverter;
            _currentSession = currentSession;
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

        public decimal NoticePeriodDistinctLeavesCalculation()
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

        public decimal DistinctLeavesCalculation()
        {
            decimal days = 0;
            if (_leavePlanConfiguration.leaveAccrual.LeaveDistributionRateOnStartOfPeriod != null
                    && _leavePlanConfiguration.leaveAccrual.LeaveDistributionRateOnStartOfPeriod.Count > 0)
            {
                AllocateTimeBreakup allocateTimeBreakup = null;
                int i = 0;
                while (i < _leavePlanConfiguration.leaveAccrual.LeaveDistributionRateOnStartOfPeriod.Count)
                {
                    allocateTimeBreakup = _leavePlanConfiguration.leaveAccrual
                                            .LeaveDistributionRateOnStartOfPeriod.ElementAt(i);
                    if (now.Day >= allocateTimeBreakup.FromDate)
                        days += allocateTimeBreakup.AllocatedLeave;

                    i++;
                }
            }

            return days;
        }

        private decimal PresentMonthProrateCount(decimal perMonthLeaves)
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

        private decimal CalculateWhenAccralMonthly(LeaveCalculationModal leaveCalculationModal, decimal perMonthLeaves)
        {
            decimal availableLeaves = 0;

            // calculate for notice period (not completed)
            decimal presentMonthLeaves = PresentMonthProrateCount(perMonthLeaves);
            if (leaveCalculationModal.employeeType == 2)
            {
                if (!_leavePlanConfiguration.leaveAccrual.IsLeavesProratedOnNotice)
                    presentMonthLeaves = NoticePeriodDistinctLeavesCalculation();
            }
            if (leaveCalculationModal.employeeType == 1)
            {
                if (!_leavePlanConfiguration.leaveAccrual.IsLeavesProratedOnNotice)
                    presentMonthLeaves = ProbationPeriodDistinctLeavesCalculation();
            }
            else
            {
                if (_leavePlanConfiguration.leaveAccrual.IsLeaveAccruedProrateDefined)
                    presentMonthLeaves = DistinctLeavesCalculation();
            }

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
            decimal fractionValue = 0;
            int integralValue = 0;
            if (!_leavePlanConfiguration.leaveAccrual.RoundOffLeaveBalance)
            {
                fractionValue = availableLeaves % 1.0m;
                if (fractionValue > 0)
                {
                    integralValue = Convert.ToInt32(fractionValue);
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

        private decimal LeaveLimitForCurrentType(int leavePlanTypeId, decimal availableLeaves, LeaveCalculationModal leaveCalculationModal)
        {
            decimal alreadyAppliedLeave = 0;

            List<CompleteLeaveDetail> completeLeaveDetails = JsonConvert.DeserializeObject<List<CompleteLeaveDetail>>(_employeeLeaveDetail.LeaveDetail);
            if (completeLeaveDetails.Count > 0)
            {
                alreadyAppliedLeave = completeLeaveDetails.FindAll(x => x.LeaveType == leavePlanTypeId && x.LeaveStatus == (int)ItemStatus.Approved).Sum(x => x.NumOfDays);
                leaveCalculationModal.lastApprovedLeaveDetail = completeLeaveDetails.Where(x => x.LeaveStatus == (int)ItemStatus.Approved)
                                                                .OrderByDescending(x => x.LeaveToDay).FirstOrDefault();
            }

            alreadyAppliedLeave = availableLeaves - alreadyAppliedLeave;
            return alreadyAppliedLeave;
        }

        private decimal CalculateLeaveIfRegularEmployement(LeaveCalculationModal leaveCalculationModal)
        {
            decimal availableLeaveLimit = 0;
            if (_leavePlanConfiguration.leaveDetail.LeaveLimit <= 0)
                return 0;

            decimal perMonthLeaves = _leavePlanConfiguration.leaveDetail.LeaveLimit / 12.0m;

            var leaveDistributedSeq = _leavePlanConfiguration.leaveAccrual.LeaveDistributionSequence;
            switch (leaveDistributedSeq)
            {
                default:
                    availableLeaveLimit = this.CalculateWhenAccralMonthly(leaveCalculationModal, perMonthLeaves);
                    break;
                case "2":
                    availableLeaveLimit = this.CalculateWhenAccralQuaterly();
                    break;
                case "3":
                    availableLeaveLimit = this.CalculateWhenAccralYearly();
                    break;
            }


            decimal extraLeaveLimit = 0;

            if (leaveCalculationModal.employeeType == 1)
            {
                // calculate leave for experienced people and allowed accrual in probation
                extraLeaveLimit = LeaveAccrualForExperienceInProbation(perMonthLeaves);
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

        private Employee LoadCalculationData(long EmployeeId)
        {
            Employee employee = default(Employee);
            var ds = _db.FetchDataSet("sp_leave_plan_calculation_get", new { EmployeeId, IsActive = 1, Year = now.Year }, false);
            if (ds != null && ds.Tables.Count == 4)
            {
                if (ds.Tables[0].Rows.Count == 0 || ds.Tables[1].Rows.Count == 0 || ds.Tables[3].Rows.Count == 0)
                    throw new HiringBellException("Fail to get employee related details. Please contact to admin.");

                employee = Converter.ToType<Employee>(ds.Tables[0]);
                _leavePlanTypes = Converter.ToList<LeavePlanType>(ds.Tables[1]);
                _employeeLeaveDetail = Converter.ToType<Leave>(ds.Tables[2]);
                _companySetting = Converter.ToType<CompanySetting>(ds.Tables[3]);
            }
            else
                throw new HiringBellException("Employee does not exist. Please contact to admin.");

            return employee;
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

        private void CheckForProbationPeriod(LeaveCalculationModal leaveCalculationModal)
        {
            leaveCalculationModal.employeeType = 0;
            if ((leaveCalculationModal.employee.CreatedOn.AddDays(_companySetting.ProbationPeriodInDays))
                .Subtract(leaveCalculationModal.fromDate).TotalDays > 0)
                leaveCalculationModal.employeeType = 1;
        }

        private void CheckForNoticePeriod(LeaveCalculationModal leaveCalculationModal)
        {
            if (leaveCalculationModal.employee.NoticePeriodId != 0 && leaveCalculationModal.employee.NoticePeriodAppliedOn != null)
                leaveCalculationModal.employeeType = 2;
        }

        private bool LeaveEligibilityCheck(LeavePlanType leavePlanType, LeaveCalculationModal leaveCalculationModal)
        {
            bool flag = false;
            // if future date then > 0 else < 0
            if (now.Subtract(leaveCalculationModal.fromDate).TotalDays > 0) // past date
            {
                if (_leavePlanConfiguration.leaveApplyDetail.BackDateLeaveApplyNotBeyondDays != 0 &&
                    leaveCalculationModal.fromDate.Subtract(now).TotalDays <= _leavePlanConfiguration.leaveApplyDetail.BackDateLeaveApplyNotBeyondDays)
                    flag = true;
                else
                {
                    leavePlanType.IsApplicable = false;
                    leavePlanType.Reason = $"This leave can only be utilized {_leavePlanConfiguration.leaveApplyDetail.ApplyPriorBeforeLeaveDate} days before leave start date.";
                }
            }
            else // future date
            {
                if (leaveCalculationModal.fromDate.Subtract(now).TotalDays >= _leavePlanConfiguration.leaveApplyDetail.ApplyPriorBeforeLeaveDate)
                    flag = true;
                else
                {
                    leavePlanType.IsApplicable = false;
                    leavePlanType.Reason = $"This leave can only be utilized {_leavePlanConfiguration.leaveApplyDetail.ApplyPriorBeforeLeaveDate} days before leave start date.";
                }
            }

            return flag;
        }

        private bool NewJoineeIsEligibleForThisLeave(LeavePlanType leavePlanType, Employee employee)
        {
            bool flag = false;
            if (_leavePlanConfiguration.leavePlanRestriction.CanApplyAfterProbation)
            {
                var daysAfterProbation = (decimal)now.Subtract(employee.CreatedOn.AddDays(_companySetting.ProbationPeriodInDays)).TotalDays;
                if (daysAfterProbation >= _leavePlanConfiguration.leavePlanRestriction.DaysAfterProbation)
                    flag = true;
                else
                {
                    flag = false;
                    leavePlanType.IsApplicable = false;
                    leavePlanType.Reason = "Days restriction after Probation period is not completed to apply this leave.";
                }
            }
            else if (_leavePlanConfiguration.leavePlanRestriction.CanApplyAfterJoining)
            {
                var daysAfterProbation = (decimal)now.Subtract(employee.CreatedOn).TotalDays;
                if (daysAfterProbation >= _leavePlanConfiguration.leavePlanRestriction.DaysAfterJoining)
                    flag = true;
                else
                {
                    flag = false;
                    leavePlanType.IsApplicable = false;
                    leavePlanType.Reason = "Days restriction after Joining period is not completed to apply this leave.";
                }
            }

            return flag;
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

        private void LoadAccrualCalculationData(int CompanyId)
        {
            var ds = _db.FetchDataSet("sp_leave_plan_calculation_get", new { CompanyId }, false);
            if (ds != null && ds.Tables.Count == 2)
            {
                if (ds.Tables[0].Rows.Count == 0 || ds.Tables[1].Rows.Count == 0)
                    throw new HiringBellException("Fail to load leave detail. Please contact to admin.");

                _leavePlanTypes = Converter.ToList<LeavePlanType>(ds.Tables[1]);
                _companySetting = Converter.ToType<CompanySetting>(ds.Tables[3]);
            }
            else
                throw new HiringBellException("Employee does not exist. Please contact to admin.");
        }

        public void RunLeaveCalculationCycle(Employee employee)
        {
            List<Company> companies = _db.GetList<Company>("");
            int i = 0;
            while (companies.Count > 0)
            {
                // get employee detail and store it in class level variable
                LoadAccrualCalculationData(companies.ElementAt(i).CompanyId);

                int PageIndex = 0;

                while (true)
                {
                    List<Employee> employees = _db.GetList<Employee>("SP_Employee_GetAll", new
                    {
                        SearchString = " 1=1 ",
                        SortBy = "",
                        PageIndex = ++PageIndex,
                        PageSize = 500
                    });

                    int k = 0;
                    while (k < _leavePlanTypes.Count)
                    {
                        ValidateAndGetLeavePlanConfiguration(_leavePlanTypes[k]);
                        if (!_leavePlanConfiguration.leaveAccrual.IsNoLeaveOnNoticePeriod)
                        {
                            Parallel.ForEach(employees, async e =>
                            {
                                _leaveCalculationModal = new LeaveCalculationModal();
                                _leaveCalculationModal.employee = e;

                                // Check employee is in probation period
                                CheckForProbationPeriod(_leaveCalculationModal);

                                // Check employee is in notice period
                                CheckForNoticePeriod(_leaveCalculationModal);

                                await RunEmployeeLeaveAccrualCycle(_leaveCalculationModal, _leavePlanTypes[k]);
                            });
                        }
                    }

                    if (employees.Last().RowIndex == employees.First().Total)
                        break;
                }
            }
        }

        public async Task RunEmployeeLeaveAccrualCycle(LeaveCalculationModal leaveCalculationModal, LeavePlanType leavePlanType)
        {
            await Task.Run<List<LeavePlanType>>(() =>
            {
                if (leaveCalculationModal.leaveRequestDetail.TotalLeaveBalanceTillNow > 0)
                {
                    leavePlanType.AvailableLeave = leaveCalculationModal.leaveRequestDetail.TotalLeaveBalanceTillNow;
                }
                else
                {
                    var leaveLimit = _leavePlanConfiguration.leaveDetail.LeaveLimit;
                    if (leaveLimit > 0)
                    {
                        switch (leaveCalculationModal.employeeType)
                        {
                            case 1:
                                if (NewJoineeIsEligibleForThisLeave(leavePlanType, leaveCalculationModal.employee))
                                    leavePlanType.AvailableLeave = CalculateLeaveIfInProbationPeriond(leavePlanType.LeavePlanTypeId);
                                break;
                            case 2:
                                leavePlanType.AvailableLeave = CalculateLeaveIfInNoticePeriond(leavePlanType.LeavePlanTypeId);
                                break;
                            default:
                                decimal availableLeave = CalculateLeaveIfRegularEmployement(leaveCalculationModal);
                                leavePlanType.AvailableLeave = LeaveLimitForCurrentType(leavePlanType.LeavePlanTypeId, availableLeave, leaveCalculationModal);
                                break;
                        }
                    }
                }

                return _leavePlanTypes;
            }, cts.Token);
        }

        public async Task<List<LeavePlanType>> GetBalancedLeave(long EmployeeId, DateTime FromDate, DateTime ToDate)
        {
            _leaveCalculationModal = new LeaveCalculationModal();
            _leaveCalculationModal.fromDate = FromDate;
            _leaveCalculationModal.toDate = ToDate;

            // get employee detail and store it in class level variable
            _leaveCalculationModal.employee = LoadCalculationData(EmployeeId);

            // Check employee is in probation period
            CheckForProbationPeriod(_leaveCalculationModal);

            // Check employee is in notice period
            CheckForNoticePeriod(_leaveCalculationModal);

            LeavePlanType leavePlanType = null;
            int i = 0;
            while (i < _leavePlanTypes.Count)
            {
                leavePlanType = _leavePlanTypes[i];
                ValidateAndGetLeavePlanConfiguration(leavePlanType);
                await RunEmployeeLeaveAccrualCycle(_leaveCalculationModal, leavePlanType);
                i++;
            }

            return _leavePlanTypes;
        }

        private async Task LeaveRestrictionCheck(LeavePlanType leavePlanType, LeaveCalculationModal leaveCalculationModal, decimal availableLeaveLimit)
        {
            var flag = await AllowToSeeAndApply();
            if (flag)
                throw new HiringBellException("You are not aligible to apply this leave");

            // get leave quota after deducting already applied leaves
            LeaveLimitForCurrentType(leavePlanType.LeavePlanTypeId, availableLeaveLimit, leaveCalculationModal);

            // calculate num of days applied
            CalculateLeaveDays(leaveCalculationModal);

            flag = LeaveEligibilityCheck(leavePlanType, leaveCalculationModal);

            // check leave restriction related to probation period
            flag = CheckProbationLeaveRestriction(leavePlanType, leaveCalculationModal);

            // get leave quota after deducting already applied leaves
            flag = CheckAllRestrictionForCurrentLeaveType(leaveCalculationModal, availableLeaveLimit);
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

        private bool CheckAllRestrictionForCurrentLeaveType(LeaveCalculationModal leaveCalculationModal, decimal availableLeaveLimit)
        {
            bool flag = true;

            // check leave gap between previous and new leave date
            if (leaveCalculationModal.lastApprovedLeaveDetail != null)
            {
                if (leaveCalculationModal.fromDate.Subtract(leaveCalculationModal.lastApprovedLeaveDetail.LeaveToDay).TotalDays <
                    Convert.ToDouble(_leavePlanConfiguration.leavePlanRestriction.GapBetweenTwoConsicutiveLeaveDates))
                {
                    flag = false;
                }
            }

            // check total leave applied and restrict for current year
            if (_leavePlanConfiguration.leavePlanRestriction.LimitOfMaximumLeavesInCalendarYear > 0 &&
                leaveCalculationModal.totalNumOfLeaveApplied > _leavePlanConfiguration.leavePlanRestriction.LimitOfMaximumLeavesInCalendarYear)
            {
                flag = false;
            }

            // check total leave applied and restrict for current month
            if (_leavePlanConfiguration.leavePlanRestriction.LimitOfMaximumLeavesInCalendarMonth > 0 &&
                leaveCalculationModal.totalNumOfLeaveApplied > _leavePlanConfiguration.leavePlanRestriction.LimitOfMaximumLeavesInCalendarMonth)
            {
                flag = false;
            }

            // check total leave applied and restrict for entire tenure
            if (_leavePlanConfiguration.leavePlanRestriction.LimitOfMaximumLeavesInEntireTenure > 0 &&
                leaveCalculationModal.totalNumOfLeaveApplied > _leavePlanConfiguration.leavePlanRestriction.LimitOfMaximumLeavesInEntireTenure)
            {
                flag = false;
            }

            // check available if any restrict minimun leave to be appllied
            if (availableLeaveLimit >= _leavePlanConfiguration.leavePlanRestriction.AvailableLeaves &&
                leaveCalculationModal.totalNumOfLeaveApplied < _leavePlanConfiguration.leavePlanRestriction.MinLeaveToApplyDependsOnAvailable)
            {
                flag = false;
            }

            // restrict leave date every month
            if (_leavePlanConfiguration.leavePlanRestriction.RestrictFromDayOfEveryMonth > 0 &&
                leaveCalculationModal.fromDate.Day <= _leavePlanConfiguration.leavePlanRestriction.RestrictFromDayOfEveryMonth)
            {
                flag = false;
            }

            return flag;
        }

        public async Task<bool> CanApplyForHalfDay(long EmployeeId)
        {
            return await Task.Run<bool>(() =>
            {
                return false;
            }, cts.Token);
        }

        public async Task<bool> AllowToSeeAndApply()
        {
            return await Task.Run<bool>(() =>
            {
                if (_leavePlanConfiguration.leaveApplyDetail.EmployeeCanSeeAndApplyCurrentPlanLeave)
                    return true;

                return false;
            }, cts.Token);
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