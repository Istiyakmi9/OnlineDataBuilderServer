using BottomhalfCore.DatabaseLayer.Common.Code;
using BottomhalfCore.Services.Code;
using BottomhalfCore.Services.Interface;
using ModalLayer.Modal;
using ModalLayer.Modal.Accounts;
using ModalLayer.Modal.Leaves;
using Newtonsoft.Json;
using NUnit.Framework.Constraints;
using OpenXmlPowerTools;
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

        private int GetTotalAttendance(PayrollEmployeeData empPayroll, List<PayrollEmployeeData> payrollEmployeeData, DateTime payrollDate)
        {
            var attrDetail = payrollEmployeeData
                                .Where(x => x.EmployeeId == empPayroll.EmployeeId && (x.ForMonth == payrollDate.Month))
                                .FirstOrDefault();

            if (attrDetail == null)
                throw HiringBellException.ThrowBadRequest("Attendance detail not found while running payroll cycle.");


            List<AttendanceDetailJson> attendanceDetailJsons = JsonConvert.DeserializeObject<List<AttendanceDetailJson>>(attrDetail.AttendanceDetail);
            int totalDays = attendanceDetailJsons.Count(x => x.PresentDayStatus != (int)ItemStatus.Rejected && x.PresentDayStatus != (int)ItemStatus.NotSubmitted);
            return totalDays;
        }

        private async Task CalculateRunPayrollForEmployees(Payroll payroll, PayrollCommonData payrollCommonData)
        {
            DateTime payrollDate = (DateTime)_currentSession.TimeZoneNow;
            int offsetindex = 0;
            decimal totalDays = 0;
            int totalDaysInMonth = 0;
            int pageSize = 5;
            while (true)
            {
                List<PayrollEmployeeData> payrollEmployeeData = GetEmployeeDetail(payrollDate, offsetindex, pageSize);
                if (payrollEmployeeData == null || payrollEmployeeData.Count == 0)
                    break;

                // run pay cycle by considering actual days in months
                if (payroll.PayCalculationId == 1)
                {
                    totalDaysInMonth = DateTime.DaysInMonth(payrollCommonData.presentDate.Year, payrollDate.Month);
                }
                else // run pay cycle by considering only weekdays in month
                {
                    totalDaysInMonth = TimezoneConverter.GetNumberOfWeekdaysInMonth(payrollCommonData.presentDate.Year, payrollDate.Month);
                }

                bool IsTaxCalculationRequired = false;
                foreach (PayrollEmployeeData empPayroll in payrollEmployeeData)
                {
                    try
                    {
                        DateTime doj = _timezoneConverter.ToTimeZoneDateTime(empPayroll.Doj, _currentSession.TimeZone);
                        if (doj.Year == payrollDate.Year && doj.Month == payrollDate.Month)
                        {
                            if (doj.Year == payrollDate.Year && doj.Month == payrollDate.Month)
                                totalDaysInMonth = totalDaysInMonth - doj.Day + 1;
                        }

                        totalDays = GetTotalAttendance(empPayroll, payrollEmployeeData, payrollDate);
                        var taxDetails = JsonConvert.DeserializeObject<List<TaxDetails>>(empPayroll.TaxDetail);
                        if (taxDetails == null)
                            throw HiringBellException.ThrowBadRequest("Invalid taxdetail found. Fail to run payroll.");

                        var presentData = taxDetails.Find(x => x.Month == payrollDate.Month);
                        if (presentData == null)
                            throw HiringBellException.ThrowBadRequest("Invalid taxdetail found. Fail to run payroll.");

                        if (!presentData.IsPayrollCompleted)
                        {
                            if (totalDays != totalDaysInMonth)
                            {
                                UpdateSalaryBreakup(payrollDate, totalDays, totalDaysInMonth, empPayroll);

                                var newAmount = (presentData.TaxDeducted / totalDaysInMonth) * totalDays;
                                presentData.TaxPaid = newAmount;
                                presentData.TaxDeducted = newAmount;
                                presentData.IsPayrollCompleted = true;
                                IsTaxCalculationRequired = true;
                            }
                            else
                            {
                                presentData.TaxPaid = presentData.TaxDeducted;
                                presentData.IsPayrollCompleted = true;
                            }

                            empPayroll.TaxDetail = JsonConvert.SerializeObject(taxDetails);
                            await _declarationService.UpdateTaxDetailsService(empPayroll, payrollCommonData, IsTaxCalculationRequired);
                            IsTaxCalculationRequired = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }

                offsetindex = offsetindex + pageSize;
            }

            await Task.CompletedTask;
        }

        private static void UpdateSalaryBreakup(DateTime payrollDate, decimal totalDays, int totalDaysInMonth, PayrollEmployeeData empPayroll)
        {
            var salaryBreakup = JsonConvert.DeserializeObject<List<AnnualSalaryBreakup>>(empPayroll.CompleteSalaryDetail);
            if (salaryBreakup == null)
                throw HiringBellException.ThrowBadRequest("Invalid salary breakup found. Fail to run payroll.");

            var presentMonthSalaryDetail = salaryBreakup.Find(x => x.MonthNumber == payrollDate.Month);
            if (presentMonthSalaryDetail != null)
            {
                foreach (var item in presentMonthSalaryDetail.SalaryBreakupDetails)
                {
                    item.FinalAmount = (item.FinalAmount / totalDaysInMonth) * totalDays;
                }

                presentMonthSalaryDetail.IsPayrollExecutedForThisMonth = true;
            }

            empPayroll.CompleteSalaryDetail = JsonConvert.SerializeObject(salaryBreakup);
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

        public async Task RunPayrollCycle(int i)
        {
            PayrollCommonData payrollCommonData = GetCommonPayrollData();
            foreach (var payroll in payrollCommonData.payrolls)
            {
                _currentSession.TimeZone = TZConvert.GetTimeZoneInfo(payroll.TimezoneName);
                payrollCommonData.timeZone = _currentSession.TimeZone;
                _currentSession.TimeZoneNow = _timezoneConverter.ToTimeZoneDateTime(DateTime.UtcNow, _currentSession.TimeZone);

                payrollCommonData.presentDate = _timezoneConverter.ToTimeZoneDateTime(DateTime.UtcNow, _currentSession.TimeZone).AddMonths(-i);
                _currentSession.TimeZoneNow = payrollCommonData.presentDate;
                payrollCommonData.utcPresentDate = DateTime.UtcNow.AddMonths(-i); ;
                switch (payroll.PayFrequency)
                {
                    case "monthly":
                        if (payroll.PayCycleDayOfMonth == payrollCommonData.presentDate.Day)
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
