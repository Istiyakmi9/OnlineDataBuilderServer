using BottomhalfCore.Services.Interface;
using ModalLayer.Modal;
using ModalLayer.Modal.Leaves;
using ServiceLayer.Interface;
using System;
using System.Threading.Tasks;

namespace ServiceLayer.Code.Leaves
{
    public class HolidaysAndWeekoffs : IHolidaysAndWeekoffs
    {
        private readonly ITimezoneConverter _timezoneConverter;
        private readonly CurrentSession _currentSession;
        private LeavePlanConfiguration _leavePlanConfiguration;
        private readonly ICompanyCalendar _companyCalendar;

        public HolidaysAndWeekoffs(
            ITimezoneConverter timezoneConverter,
            ICompanyCalendar companyCalendar,
            CurrentSession currentSession)
        {
            _timezoneConverter = timezoneConverter;
            _currentSession = currentSession;
            _companyCalendar = companyCalendar;
        }

        public async Task CheckHolidayWeekOffRules(LeaveCalculationModal leaveCalculationModal)
        {
            _leavePlanConfiguration = leaveCalculationModal.leavePlanConfiguration;

            await CheckAdjoiningHolidyOnLeave(leaveCalculationModal);

            await CheckAdjoiningWeekOffOnLeave(leaveCalculationModal);

            await ApplySandwitchPolicy();

            await ComputeApplyingLeaveDays(leaveCalculationModal, false);

            await Task.CompletedTask;
        }

        // step - 1 -- adjoining holiday
        private async Task CheckAdjoiningHolidyOnLeave(LeaveCalculationModal leaveCalculationModal)
        {
            if (_leavePlanConfiguration.leaveHolidaysAndWeekoff.AdJoiningHolidayIsConsiderAsLeave)
            {
                // Yes
                var totalDays = leaveCalculationModal.fromDate.Date.Subtract(leaveCalculationModal.toDate.Date).TotalDays;
                if (totalDays >= (double)_leavePlanConfiguration.leaveHolidaysAndWeekoff.ConsiderLeaveIfNumOfDays)
                {
                    if (_leavePlanConfiguration.leaveHolidaysAndWeekoff.IfLeaveLieBetweenTwoHolidays)
                    {
                        bool flag = await _companyCalendar.IsHolidayBetweenTwoDates(leaveCalculationModal.fromDate, leaveCalculationModal.toDate);
                        await ComputeApplyingLeaveDays(leaveCalculationModal, flag);
                    }
                    else if (_leavePlanConfiguration.leaveHolidaysAndWeekoff.IfHolidayIsRightBeforLeave)
                    {
                        bool flag = await _companyCalendar.IsHoliday(leaveCalculationModal.fromDate.AddDays(1));
                        await ComputeApplyingLeaveDays(leaveCalculationModal, flag);
                    }
                    else if (_leavePlanConfiguration.leaveHolidaysAndWeekoff.IfHolidayIsRightAfterLeave)
                    {
                        bool flag = await _companyCalendar.IsHoliday(leaveCalculationModal.fromDate.AddDays(-1));
                        await ComputeApplyingLeaveDays(leaveCalculationModal, flag);
                    }
                    else if (_leavePlanConfiguration.leaveHolidaysAndWeekoff.IfHolidayIsBetweenLeave)
                    {
                        bool flag = await _companyCalendar.IsHolidayBetweenTwoDates(leaveCalculationModal.fromDate.AddDays(-1), leaveCalculationModal.fromDate.AddDays(1));
                        await ComputeApplyingLeaveDays(leaveCalculationModal, flag);
                    }
                    else if (_leavePlanConfiguration.leaveHolidaysAndWeekoff.IfHolidayIsRightBeforeAfterOrInBetween)
                    {
                        bool flag = await _companyCalendar.IsHolidayBetweenTwoDates(leaveCalculationModal.fromDate, leaveCalculationModal.toDate);
                        await ComputeApplyingLeaveDays(leaveCalculationModal, flag);
                    }
                }
            }
            else
            {
                var holidayDates = await _companyCalendar.GetHolidayBetweenTwoDates(leaveCalculationModal.fromDate, leaveCalculationModal.toDate);
                var totalApplyingDate = leaveCalculationModal.toDate.Date.Subtract(leaveCalculationModal.fromDate.Date).TotalDays;
                leaveCalculationModal.numberOfLeaveApplyring = (decimal)totalApplyingDate - (decimal)holidayDates.Count;
                // No = take only week days don't consider weekends as leave
            }

            await Task.CompletedTask;
        }

        // step - 2 -- adjoining weekoff
        private async Task CheckAdjoiningWeekOffOnLeave(LeaveCalculationModal leaveCalculationModal)
        {
            var totalDays = leaveCalculationModal.fromDate.Date.Subtract(leaveCalculationModal.toDate.Date).TotalDays;
            if (totalDays >= (double)_leavePlanConfiguration.leaveHolidaysAndWeekoff.ConsiderLeaveIfIncludeDays)
            {
                if (_leavePlanConfiguration.leaveHolidaysAndWeekoff.IfLeaveLieBetweenWeekOff)
                {
                    bool flag = await _companyCalendar.IsWeekOffBetweenTwoDates(leaveCalculationModal.fromDate, leaveCalculationModal.toDate);
                    await ComputeApplyingLeaveDays(leaveCalculationModal, flag);
                }
                else if (_leavePlanConfiguration.leaveHolidaysAndWeekoff.IfWeekOffIsRightBeforLeave)
                {
                    bool flag = await _companyCalendar.IsWeekOff(leaveCalculationModal.fromDate.AddDays(1));
                    await ComputeApplyingLeaveDays(leaveCalculationModal, flag);
                }
                else if (_leavePlanConfiguration.leaveHolidaysAndWeekoff.IfWeekOffIsRightAfterLeave)
                {
                    bool flag = await _companyCalendar.IsWeekOff(leaveCalculationModal.fromDate.AddDays(-1));
                    await ComputeApplyingLeaveDays(leaveCalculationModal, flag);
                }
                else if (_leavePlanConfiguration.leaveHolidaysAndWeekoff.IfWeekOffIsBetweenLeave)
                {
                    bool flag = await _companyCalendar.IsWeekOffBetweenTwoDates(leaveCalculationModal.fromDate.AddDays(-1), leaveCalculationModal.fromDate.AddDays(1));
                    await ComputeApplyingLeaveDays(leaveCalculationModal, flag);
                }
                else if (_leavePlanConfiguration.leaveHolidaysAndWeekoff.IfWeekOffIsRightBeforeAfterOrInBetween)
                {
                    bool flag = await _companyCalendar.IsWeekOffBetweenTwoDates(leaveCalculationModal.fromDate, leaveCalculationModal.toDate);
                    await ComputeApplyingLeaveDays(leaveCalculationModal, flag);
                }
            }
            else
            {
                var weekoffDates = await _companyCalendar.GetWeekOffBetweenTwoDates(leaveCalculationModal.fromDate, leaveCalculationModal.toDate);
                var totalApplyingDate = leaveCalculationModal.toDate.Date.Subtract(leaveCalculationModal.fromDate.Date).TotalDays;
                leaveCalculationModal.numberOfLeaveApplyring = (decimal)totalApplyingDate - (decimal)weekoffDates.Count;
            }
            await Task.CompletedTask;
        }

        // step - 3 -- sandwitch policy
        private async Task ApplySandwitchPolicy()
        {
            if (_leavePlanConfiguration.leaveHolidaysAndWeekoff.ClubSandwichPolicy)
            {

            }
            else
            {

            }
            await Task.CompletedTask;
        }

        private async Task ComputeApplyingLeaveDays(LeaveCalculationModal leaveCalculationModal, bool includeHoliday)
        {
            leaveCalculationModal.numberOfLeaveApplyring = 0;
            if (_leavePlanConfiguration.leaveHolidaysAndWeekoff.AdJoiningHolidayIsConsiderAsLeave)
            {
            }

            if (!_leavePlanConfiguration.leaveHolidaysAndWeekoff.AdjoiningWeekOffIsConsiderAsLeave)
            {
                var fromDate = _timezoneConverter.ToTimeZoneDateTime(
                    leaveCalculationModal.fromDate.ToUniversalTime(),
                    _currentSession.TimeZone
                    );

                var toDate = _timezoneConverter.ToTimeZoneDateTime(
                    leaveCalculationModal.toDate.ToUniversalTime(),
                    _currentSession.TimeZone
                    );

                while (toDate.Subtract(fromDate).TotalDays >= 0)
                {
                    if (fromDate.DayOfWeek != DayOfWeek.Saturday && fromDate.DayOfWeek != DayOfWeek.Sunday)
                        leaveCalculationModal.numberOfLeaveApplyring++;

                    fromDate = fromDate.AddDays(1);
                }
            }

            await Task.CompletedTask;
        }
    }
}
