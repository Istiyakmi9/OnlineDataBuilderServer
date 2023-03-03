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
using System.Threading.Tasks;
using TimeZoneConverter;

namespace ServiceLayer.Code.PayrollCycle
{
    public class PayrollService : IPayrollService
    {
        private readonly IDb _db;
        private readonly ITimezoneConverter _timezoneConverter;
        private readonly CurrentSession _currentSession;
        private readonly IDeclarationService _declarationService;

        public PayrollService(ITimezoneConverter timezoneConverter,
            IDb db,
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

            // now get salary for 1 day
            var totalAmount = annualSalaryBreakup.SalaryBreakupDetails.Where(x => x.ComponentId.ToLower() != "gross" && x.ComponentId.ToLower() != "ctc")
                .Sum(x => x.FinalAmount);

            decimal finalAmount = (totalAmount / totalDaysInMonth) * totalDays;
            return finalAmount;
        }

        private List<PayrollEmployeeData> GetEmployeeDetail(DateTime presentDate, int offsetindex, int pageSize)
        {
            var resultSet = _db.FetchDataSet("sp_employee_payroll_get_by_page", new
            {
                ForYear = presentDate.Year,
                ForMonth = presentDate.Month,
                OffsetIndex = offsetindex,
                PageSize = pageSize
            }, false);

            if (resultSet == null || resultSet.Tables.Count != 2)
                throw HiringBellException.ThrowBadRequest($"[GetEmployeeDetail]: Employee data not found for date: {presentDate} of offSet: {offsetindex}");

            List<PayrollEmployeeData> payrollEmployeeData = Converter.ToList<PayrollEmployeeData>(resultSet.Tables[0]);
            List<EmployeeDeclaration> employeeDeclarations = Converter.ToList<EmployeeDeclaration>(resultSet.Tables[1]);

            Parallel.ForEach(payrollEmployeeData, x =>
            {
                var context = employeeDeclarations.Find(i => i.EmployeeId == x.EmployeeId);
                if (context != null)
                    x.employeeDeclaration = context;
                else
                    x.employeeDeclaration = null;
            });

            return payrollEmployeeData;
        }

        private async Task CalculateRunPayrollForEmployees(Payroll payroll, PayrollCommonData payrollCommonData)
        {
            int offsetindex = 0;
            int absents = 0;
            decimal totalDays = 0;
            decimal finalAmount = 0;
            int totalDaysInMonth = 0;
            int pageSize = 5;
            while (true)
            {
                try
                {
                    List<PayrollEmployeeData> payrollEmployeeData = GetEmployeeDetail(payrollCommonData.presentDate, offsetindex, pageSize);
                    if (payrollEmployeeData == null || payrollEmployeeData.Count == 0)
                        break;

                    // run pay cycle by considering actual days in months
                    if (payroll.PayCalculationId == 1)
                    {
                        totalDaysInMonth = DateTime.DaysInMonth(payrollCommonData.presentDate.Year, payrollCommonData.presentDate.Month);
                    }
                    else // run pay cycle by considering only weekdays in month
                    {
                        totalDaysInMonth = TimezoneConverter.GetNumberOfWeekdaysInMonth(payrollCommonData.presentDate.Year, payrollCommonData.presentDate.Month);
                    }

                    foreach (PayrollEmployeeData empPayroll in payrollEmployeeData)
                    {
                        var data = JsonConvert.DeserializeObject<List<AttendanceDetailJson>>(empPayroll.AttendanceDetail);
                        absents = data.Count(x => x.PresentDayStatus != (int)ItemStatus.Approved);
                        totalDays = totalDaysInMonth - absents;
                        totalDays = CheckLeaveFromAttendance(totalDays, empPayroll);
                        finalAmount = GetFinalAmountMonthly(totalDays, empPayroll, payrollCommonData.presentDate, totalDaysInMonth);
                        await _declarationService.UpdateTaxDetailsService(empPayroll, payrollCommonData);
                    }

                    offsetindex = offsetindex + pageSize;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            await Task.CompletedTask;
        }

        private PayrollCommonData GetCommonPayrollData()
        {
            PayrollCommonData payrollCommonData = new PayrollCommonData();
            var result = _db.FetchDataSet("sp_payroll_cycle_setting_get_all");
            if (result.Tables.Count != 5)
                throw HiringBellException.ThrowBadRequest($"[GetCommonPayrollData]: Fail to get payroll cycle data to run it. Please contact to admin");

            if (result.Tables[0].Rows.Count == 0)
                throw HiringBellException.ThrowBadRequest($"[GetCommonPayrollData]: Payroll cycle and company setting detail not found. Please contact to admin");

            if (result.Tables[1].Rows.Count == 0)
                throw HiringBellException.ThrowBadRequest($"[GetCommonPayrollData]: Salary component not found. Please contact to admin");

            if (result.Tables[2].Rows.Count == 0)
                throw HiringBellException.ThrowBadRequest($"[GetCommonPayrollData]: Surcharge slab detail not found. Please contact to admin");

            if (result.Tables[3].Rows.Count == 0)
                throw HiringBellException.ThrowBadRequest($"[GetCommonPayrollData]: Professional tax detail not found. Please contact to admin");

            if (result.Tables[4].Rows.Count == 0)
                throw HiringBellException.ThrowBadRequest($"[GetCommonPayrollData]: Salary group detail not found. Please contact to admin");

            payrollCommonData.payrolls = Converter.ToList<Payroll>(result.Tables[0]);
            payrollCommonData.salaryComponents = Converter.ToList<SalaryComponents>(result.Tables[1]);
            payrollCommonData.surchargeSlabs = Converter.ToList<SurChargeSlab>(result.Tables[2]);
            payrollCommonData.ptaxSlab = Converter.ToList<PTaxSlab>(result.Tables[3]);
            payrollCommonData.salaryGroups = Converter.ToList<SalaryGroup>(result.Tables[4]);

            return payrollCommonData;
        }

        public async Task RunPayrollCycle()
        {
            PayrollCommonData payrollCommonData = GetCommonPayrollData();
            foreach (var payroll in payrollCommonData.payrolls)
            {
                _currentSession.TimeZone = TZConvert.GetTimeZoneInfo(payroll.TimezoneName);
                payrollCommonData.timeZone = _currentSession.TimeZone;

                payrollCommonData.presentDate = _timezoneConverter.ToTimeZoneDateTime(DateTime.UtcNow, _currentSession.TimeZone);
                payrollCommonData.utcPresentDate = DateTime.UtcNow;
                switch (payroll.PayFrequency)
                {
                    case "monthly":
                        if (payroll.PayCycleDayOfMonth == payrollCommonData.presentDate.Day || true)
                        {
                            await CalculateRunPayrollForEmployees(payroll, payrollCommonData);
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
