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
            decimal availableLeaveLimit = _leavePlanType.AvailableLeave;
            _leavePlanConfiguration = leaveCalculationModal.leavePlanConfiguration;

            // check leave gap between previous and new leave date
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

            await Task.CompletedTask;
        }

        private void NewJoineeIsEligibleForThisLeave(LeaveCalculationModal leaveCalculationModal)
        {
            if (_leavePlanConfiguration.leavePlanRestriction.CanApplyAfterProbation)
            {
                var dateFromApplyLeave = leaveCalculationModal.employee.CreatedOn.AddDays(
                    leaveCalculationModal.companySetting.ProbationPeriodInDays +
                    Convert.ToDouble(_leavePlanConfiguration.leavePlanRestriction.DaysAfterProbation));

                if (dateFromApplyLeave.Subtract(leaveCalculationModal.presentDate).TotalDays < 0)
                    throw new HiringBellException("Days restriction after Probation period is not completed to apply this leave.");
            }
            else
            {
                var dateAfterProbation = leaveCalculationModal.employee.CreatedOn.AddDays(Convert.ToDouble(_leavePlanConfiguration.leavePlanRestriction.DaysAfterJoining));
                if (leaveCalculationModal.fromDate.Subtract(dateAfterProbation).TotalDays < 0)
                    throw new HiringBellException("Days restriction after Joining period is not completed to apply this leave.");
            }
        }
    }
}
