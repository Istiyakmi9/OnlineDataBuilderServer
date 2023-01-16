using ModalLayer.Modal;
using ModalLayer.Modal.Leaves;
using System;
using System.Threading.Tasks;

namespace ServiceLayer.Code.Leaves
{
    public class Restriction
    {
        private LeavePlanConfiguration _leavePlanConfiguration;
        private LeavePlanType _leavePlanType;

        public async Task CheckRestrictionForLeave(LeaveCalculationModal leaveCalculationModal, LeavePlanType leavePlanType)
        {
            _leavePlanType = leavePlanType;
            _leavePlanConfiguration = leaveCalculationModal.leavePlanConfiguration;

            // step - 1
            // check employee type and apply restriction 
            await NewJoineeIsEligibleForThisLeave(leaveCalculationModal);

            // step 2
            await CheckAvailAllBalanceLeaveInAMonth(leaveCalculationModal);

            // step 3
            // call if manage tries of overrite and apply leave for you.
            // await ManagerOverrideAndApplyLeave(leaveCalculationModal);

            // step - 4
            await LeaveGapRestriction(leaveCalculationModal);
        }

        public async Task<bool> ManagerOverrideAndApplyLeave(LeaveCalculationModal leaveCalculationModal)
        {
            bool flag = false;
            if (_leavePlanConfiguration.leavePlanRestriction.CanManageOverrideLeaveRestriction)
                flag = true;
            return await Task.FromResult(flag);
        }

        private async Task CheckAvailAllBalanceLeaveInAMonth(LeaveCalculationModal leaveCalculationModal)
        {
            if (!_leavePlanConfiguration.leavePlanRestriction.IsLeaveInNoticeExtendsNoticePeriod)
            {
                if (leaveCalculationModal.numberOfLeaveApplyring == _leavePlanType.AvailableLeave)
                    throw new HiringBellException("You can't apply your entire available leave in one month.");
            }
            else
            {

            }

            await Task.CompletedTask;
        }

        private async Task LeaveGapRestriction(LeaveCalculationModal leaveCalculationModal)
        {
            decimal availableLeaveLimit = _leavePlanType.AvailableLeave;

            // check leave gap between two consucutive leaves
            if (leaveCalculationModal.lastApprovedLeaveDetail != null)
            {
                // date after last applied todate.
                var dayDiff = leaveCalculationModal.fromDate.Subtract(leaveCalculationModal.lastApprovedLeaveDetail.LeaveToDay).TotalDays;
                if (dayDiff > 0)
                {
                    var barrierFromDate = leaveCalculationModal.lastApprovedLeaveDetail.LeaveToDay
                    .AddDays(
                            Convert.ToDouble(_leavePlanConfiguration.leavePlanRestriction.GapBetweenTwoConsicutiveLeaveDates)
                        );

                    if (leaveCalculationModal.fromDate.Subtract(barrierFromDate).TotalDays <= 0)
                        throw new HiringBellException($"Minimumn {_leavePlanConfiguration.leavePlanRestriction.GapBetweenTwoConsicutiveLeaveDates} days gap required to apply this leave");
                }
                else // date before last applied from date.
                {
                    var barrierFromDate = leaveCalculationModal.lastApprovedLeaveDetail.LeaveFromDay
                    .AddDays(
                                                -1 * Convert.ToDouble(_leavePlanConfiguration.leavePlanRestriction.GapBetweenTwoConsicutiveLeaveDates)
                                            );

                    if (leaveCalculationModal.fromDate.Subtract(barrierFromDate).TotalDays >= 0)
                        throw new HiringBellException($"Minimumn {_leavePlanConfiguration.leavePlanRestriction.GapBetweenTwoConsicutiveLeaveDates} days gap required to apply this leave");
                }
            }

            // check total leave applied and restrict for current year
            if (_leavePlanConfiguration.leavePlanRestriction.LimitOfMaximumLeavesInCalendarYear > 0 &&
                leaveCalculationModal.numberOfLeaveApplyring > _leavePlanConfiguration.leavePlanRestriction.LimitOfMaximumLeavesInCalendarYear)
                throw new HiringBellException($"Calendar year leave limit is only {_leavePlanConfiguration.leavePlanRestriction.LimitOfMaximumLeavesInCalendarYear} days.");

            // check total leave applied and restrict for current month
            if (_leavePlanConfiguration.leavePlanRestriction.LimitOfMaximumLeavesInCalendarMonth > 0 &&
                leaveCalculationModal.numberOfLeaveApplyring > _leavePlanConfiguration.leavePlanRestriction.LimitOfMaximumLeavesInCalendarMonth)
                throw new HiringBellException($"Calendar month leave limit is only {_leavePlanConfiguration.leavePlanRestriction.LimitOfMaximumLeavesInCalendarMonth} days.");

            // check total leave applied and restrict for entire tenure
            if (_leavePlanConfiguration.leavePlanRestriction.LimitOfMaximumLeavesInEntireTenure > 0 &&
                leaveCalculationModal.numberOfLeaveApplyring > _leavePlanConfiguration.leavePlanRestriction.LimitOfMaximumLeavesInEntireTenure)
                throw new HiringBellException($"Entire tenure leave limit is only {_leavePlanConfiguration.leavePlanRestriction.LimitOfMaximumLeavesInEntireTenure} days.");

            // check available if any restrict minimun leave to be appllied
            if (availableLeaveLimit >= _leavePlanConfiguration.leavePlanRestriction.AvailableLeaves &&
                leaveCalculationModal.numberOfLeaveApplyring < _leavePlanConfiguration.leavePlanRestriction.MinLeaveToApplyDependsOnAvailable)
                throw new HiringBellException($"Minimun {_leavePlanConfiguration.leavePlanRestriction.AvailableLeaves} days of leave only allowed for this type.");

            // restrict leave date every month
            if (_leavePlanConfiguration.leavePlanRestriction.RestrictFromDayOfEveryMonth > 0 &&
                leaveCalculationModal.fromDate.Day <= _leavePlanConfiguration.leavePlanRestriction.RestrictFromDayOfEveryMonth)
                throw new HiringBellException($"Apply this leave before {_leavePlanConfiguration.leavePlanRestriction.RestrictFromDayOfEveryMonth} day of any month.");

            await Task.CompletedTask;
        }

        private async Task NewJoineeIsEligibleForThisLeave(LeaveCalculationModal leaveCalculationModal)
        {
            if (_leavePlanConfiguration.leavePlanRestriction.CanApplyAfterProbation
                && leaveCalculationModal.employeeType == ApplicationConstants.Regular)
            {
                var dateFromApplyLeave = leaveCalculationModal.employee.CreatedOn.AddDays(
                    leaveCalculationModal.companySetting.ProbationPeriodInDays +
                    Convert.ToDouble(_leavePlanConfiguration.leavePlanRestriction.DaysAfterProbation));

                if (dateFromApplyLeave.Subtract(leaveCalculationModal.presentDate).TotalDays < 0)
                    throw new HiringBellException("Days restriction after Probation period is not completed to apply this leave.");
            }
            else if (leaveCalculationModal.employeeType == ApplicationConstants.InProbationPeriod)
            {
                var dateAfterProbation = leaveCalculationModal.employee.CreatedOn.AddDays(Convert.ToDouble(_leavePlanConfiguration.leavePlanRestriction.DaysAfterJoining));
                if (leaveCalculationModal.fromDate.Subtract(dateAfterProbation).TotalDays < 0)
                    throw new HiringBellException("Days restriction after Joining period is not completed to apply this leave.");
            }

            if (_leavePlanConfiguration.leavePlanRestriction.IsAvailRestrictedLeavesInProbation &&
                leaveCalculationModal.employeeType == ApplicationConstants.InProbationPeriod)
            {
                var numberOfDaysApplying = leaveCalculationModal.fromDate.Date.Subtract(leaveCalculationModal.toDate).TotalDays;
                if (numberOfDaysApplying > (double)_leavePlanConfiguration.leavePlanRestriction.LeaveLimitInProbation)
                    throw new HiringBellException($"In probation period you can take upto {numberOfDaysApplying} no. of days only.");
            }

            await Task.CompletedTask;
        }
    }
}
