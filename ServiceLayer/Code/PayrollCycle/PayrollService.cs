using BottomhalfCore.DatabaseLayer.Common.Code;
using BottomhalfCore.DatabaseLayer.MySql.Code;
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
using System.Threading.Tasks;

namespace ServiceLayer.Code.PayrollCycle
{
    internal class PayrollService
    {
        private readonly IDb _db;
        private readonly ITimezoneConverter _timezoneConverter;
        private readonly CurrentSession _currentSession;
        private readonly IDeclarationService _declarationService;

        public PayrollService(ITimezoneConverter timezoneConverter,
            Db db,
            IDeclarationService declarationService,
            CurrentSession currentSession)
        {
            _db = db;
            _timezoneConverter = timezoneConverter;
            _currentSession = currentSession;
            _declarationService = declarationService;
        }

        private decimal CheckLeaveFromAttendance(decimal totalDays, PayrollEmployeeData attr)
        {
            if (!string.IsNullOrEmpty(attr.LeaveDetail) && attr.LeaveDetail != "[]")
            {
                var leaves = JsonConvert.DeserializeObject<List<CompleteLeaveDetail>>(attr.LeaveDetail);

                DateTime presentDate = DateTime.UtcNow;
                // find if both from date and to date is on same month
                var leavesOnMonth = leaves.FindAll(x => x.LeaveFromDay.Month == DateTime.UtcNow.Month && x.LeaveToDay.Month == DateTime.UtcNow.Month);
                if (leavesOnMonth.Count > 0)
                {
                    foreach (var item in leavesOnMonth)
                    {
                        totalDays = totalDays + item.NumOfDays;
                    }
                }

                // else find if from date in on present month
                var leave = leaves.Find(x => x.LeaveFromDay.Month == DateTime.UtcNow.Month && x.LeaveToDay.Month != DateTime.UtcNow.Month);
                if (leave != null)
                {
                    totalDays = totalDays + leave.NumOfDays;
                }

                // else find to date on present month
                leave = leaves.Find(x => x.LeaveFromDay.Month != DateTime.UtcNow.Month && x.LeaveToDay.Month == DateTime.UtcNow.Month);
                if (leave != null)
                {
                    totalDays = totalDays + leave.NumOfDays;
                }
            }

            return totalDays;
        }

        private decimal GetFinalAmountMonthly(decimal totalDays, PayrollEmployeeData empPayroll, DateTime presentDate, int totalDaysInMonth)
        {
            var annualSalaryBreakups = JsonConvert.DeserializeObject<List<AnnualSalaryBreakup>>(empPayroll.CompleteSalaryDetail);
            var annualSalaryBreakup = annualSalaryBreakups.Find(x => x.MonthNumber == presentDate.Month);
            var totalAmount = annualSalaryBreakup.SalaryBreakupDetails.Sum(x => x.FinalAmount);

            // now get salary for 1 day
            decimal finalAmount = (totalAmount / totalDaysInMonth) * totalDays;
            return finalAmount;
        }

        private async Task CalculateRunPayrollForEmployees(Payroll payroll, DateTime presentDate)
        {
            int offsetindex = 0;
            int absents = 0;
            decimal totalDays = 0;
            decimal finalAmount = 0;
            int totalDaysInMonth = 0;
            while (true)
            {
                try
                {
                    var attendanceDetail = _db.GetList<PayrollEmployeeData>("sp_attendance_get_all_page", new
                    {
                        ForYear = presentDate.Year,
                        ForMonth = presentDate.Month,
                        OffsetIndex = offsetindex,
                        PageSize = 500
                    }, false);

                    if (attendanceDetail == null || attendanceDetail.Count == 0)
                        break;

                    // run pay cycle by considering actual days in months
                    if (payroll.PayCalculationId == 1)
                    {
                        totalDaysInMonth = DateTime.DaysInMonth(presentDate.Year, presentDate.Month);
                    }
                    else // run pay cycle by considering only weekdays in month
                    {
                        totalDaysInMonth = TimezoneConverter.GetNumberOfWeekdaysInMonth(presentDate.Year, presentDate.Month);
                    }

                    foreach (PayrollEmployeeData empPayroll in attendanceDetail)
                    {
                        var data = JsonConvert.DeserializeObject<List<AttendanceDetailJson>>(empPayroll.AttendanceDetail);
                        absents = data.Count(x => x.PresentDayStatus != (int)ItemStatus.Approved);
                        totalDays = totalDaysInMonth - absents;
                        totalDays = CheckLeaveFromAttendance(totalDays, empPayroll);
                        finalAmount = GetFinalAmountMonthly(totalDays, empPayroll, presentDate, totalDaysInMonth);
                        await _declarationService.GetEmployeeDeclarationDetail(empPayroll.EmployeeId, true);
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            await Task.CompletedTask;
        }

        public async Task RunPayrollCycle()
        {
            var result = _db.GetList<Payroll>("sp_payroll_cycle_setting_get_all");
            foreach (var payroll in result)
            {
                switch (payroll.PayFrequency)
                {
                    case "monthly":
                        var presentDay = _timezoneConverter.ToTimeZoneDateTime(DateTime.UtcNow, _currentSession.TimeZone);
                        if (payroll.PayCycleDayOfMonth == presentDay.Day)
                        {
                            await CalculateRunPayrollForEmployees(payroll, presentDay);
                        }
                        break;
                    case "daily":
                        break;
                    case "hourly":
                        break;
                }
            }

            await Task.CompletedTask;
        }
    }
}
