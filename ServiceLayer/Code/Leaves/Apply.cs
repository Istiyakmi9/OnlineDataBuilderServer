using BottomhalfCore.Services.Interface;
using ModalLayer.Modal;
using ModalLayer.Modal.Leaves;
using System;
using System.Threading.Tasks;

namespace ServiceLayer.Code.Leaves
{
    public class Apply
    {
        private LeavePlanConfiguration _leavePlanConfiguration;
        private LeavePlanType _leavePlanType;

        private readonly ITimezoneConverter _timezoneConverter;

        public Apply(ITimezoneConverter timezoneConverter)
        {
            _timezoneConverter = timezoneConverter;
        }

        public async Task CheckLeaveApplyRules(LeaveCalculationModal leaveCalculationModal, LeavePlanType leavePlanType)
        {
            _leavePlanType = leavePlanType;
            _leavePlanConfiguration = leaveCalculationModal.leavePlanConfiguration;

            if (leaveCalculationModal.isApplyingForHalfDay)
                CheckForHalfDayRestriction();

            // IsAllowedToSeeAndApply();

            LeaveEligibilityCheck(leaveCalculationModal);

            // await RequiredDocumentForExtending(leaveCalculationModal);

            await Task.CompletedTask;
        }

        // step - 1
        public void CheckForHalfDayRestriction()
        {
            if (!_leavePlanConfiguration.leaveApplyDetail.IsAllowForHalfDay)
            {
                throw HiringBellException.ThrowBadRequest("Half day leave not allow under current leave type.");
            }
        }

        // step - 2
        public void IsAllowedToSeeAndApply()
        {
            if (!_leavePlanConfiguration.leaveApplyDetail.EmployeeCanSeeAndApplyCurrentPlanLeave)
            {

            }
        }

        // step - 3, 4
        private void LeaveEligibilityCheck(LeaveCalculationModal leaveCalculationModal)
        {
            // if future date then > 0 else < 0
            var calculationDate = leaveCalculationModal.timeZonepresentDate.AddDays(_leavePlanConfiguration.leaveApplyDetail.ApplyPriorBeforeLeaveDate);

            // step - 4  future date
            if (leaveCalculationModal.fromDate.Date.Subtract(calculationDate.Date).TotalDays < 0)
            {
                throw new HiringBellException($"Only applycable atleast, before " +
                    $"{_leavePlanConfiguration.leaveApplyDetail.ApplyPriorBeforeLeaveDate} calendar days.");
            }


            // step - 3 past date
            calculationDate = leaveCalculationModal.timeZonepresentDate.AddDays(-_leavePlanConfiguration.leaveApplyDetail.BackDateLeaveApplyNotBeyondDays);

            if (calculationDate.Date.Subtract(leaveCalculationModal.fromDate.Date).TotalDays > 0)
            {
                throw HiringBellException.ThrowBadRequest($"Can't apply back date leave beyond then " +
                    $"{_leavePlanConfiguration.leaveApplyDetail.BackDateLeaveApplyNotBeyondDays} calendar days.");
            }
        }


        // step - 5
        public void DoesLeaveRequiredComments(LeaveCalculationModal leaveCalculationModal)
        {
            if (_leavePlanConfiguration.leaveApplyDetail.CurrentLeaveRequiredComments && string.IsNullOrEmpty(leaveCalculationModal.leaveRequestDetail.Reason))
            {
                throw HiringBellException.ThrowBadRequest("Comment is required for this leave type");
            }
        }

        // step - 6
        public async Task<bool> RequiredDocumentForExtending(LeaveCalculationModal leaveCalculationModal)
        {
            bool flag = false;
            if (_leavePlanConfiguration.leaveApplyDetail.ProofRequiredIfDaysExceeds)
            {
                var leaveDay = leaveCalculationModal.fromDate.Date.Subtract(leaveCalculationModal.toDate.Date).TotalDays;
                if (leaveDay > _leavePlanConfiguration.leaveApplyDetail.NoOfDaysExceeded)
                    flag = true;
            }

            return await Task.FromResult(flag);
        }
    }
}
