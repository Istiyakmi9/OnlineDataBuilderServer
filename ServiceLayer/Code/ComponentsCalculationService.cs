using ModalLayer.Modal;
using ModalLayer.Modal.Accounts;
using Newtonsoft.Json;
using ServiceLayer.Interface;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ServiceLayer.Code
{
    public class ComponentsCalculationService : IComponentsCalculationService
    {
        public decimal StandardDeductionComponent(EmployeeCalculation empCal)
        {
            return 50000;
        }

        public decimal ProfessionalTaxComponent(EmployeeCalculation empCal)
        {
            decimal amount = 0;
            SalaryComponents component = null;
            component = empCal.employeeDeclaration.SalaryComponentItems.Find(x => x.ComponentId == "PTAX");
            if (component != null)
            {
                component.DeclaredValue = 2400;
                amount = component.DeclaredValue;
            }

            return amount;
        }
        public decimal EmployerProvidentFund(EmployeeDeclaration employeeDeclaration, SalaryGroup salaryGroup)
        {
            decimal value = 0;
            SalaryComponents component = null;
            component = salaryGroup.GroupComponents.Find(x => x.ComponentId == "EPF");
            if (component != null)
                value = component.DeclaredValue;

            return value;
        }

        private decimal SurchargeAndCess(decimal GrossIncomeTax, decimal GrossIncome)
        {
            decimal Cess = 0;
            decimal Surcharges = 0;
            if (GrossIncomeTax > 0)
                Cess = (4 * GrossIncomeTax) / 100;

            if (GrossIncome > 5000000 && GrossIncome <= 10000000)
                Surcharges = (10 * GrossIncome) / 100;
            else if (GrossIncome > 10000000 && GrossIncome <= 20000000)
                Surcharges = (15 * GrossIncome) / 100;
            else if (GrossIncome > 20000000 && GrossIncome <= 50000000)
                Surcharges = (25 * GrossIncome) / 100;
            else if (GrossIncome > 50000000)
                Surcharges = (37 * GrossIncome) / 100;

            return (Cess + Surcharges);
        }

        public void TaxRegimeCalculation(EmployeeDeclaration employeeDeclaration, decimal grossIncome, List<TaxRegime> taxRegimeSlabs)
        {
            decimal taxableIncome = employeeDeclaration.TotalAmount;
            if (taxableIncome < 0)
                throw new HiringBellException("Invalid TaxableIncome");

            decimal tax = 0;
            decimal remainingAmount = taxableIncome;
            taxRegimeSlabs = taxRegimeSlabs.OrderByDescending(x => x.MinTaxSlab).ToList<TaxRegime>();
            var taxSlab = new Dictionary<int, TaxSlabDetail>();
            decimal slabAmount = 0;
            var i = 0;
            taxSlab.Add(i, new TaxSlabDetail { Description = "Gross Income Tax", Value = tax });
            while (taxableIncome > 0)
            {
                slabAmount = 0;
                if (taxableIncome >= taxRegimeSlabs[i].MinTaxSlab)
                {
                    remainingAmount = taxableIncome - (taxRegimeSlabs[i].MinTaxSlab - 1);
                    slabAmount = (taxRegimeSlabs[i].TaxRatePercentage * remainingAmount) / 100;
                    tax += slabAmount;
                    taxableIncome -= remainingAmount;

                    var maxSlabAmount = taxRegimeSlabs[i].MaxTaxSlab.ToString() == "0" ? "Above" : taxRegimeSlabs[i].MaxTaxSlab.ToString();
                    var taxSlabDetail = new TaxSlabDetail
                    {
                        Description = $"{taxRegimeSlabs[i].TaxRatePercentage}% Tax on income between {taxRegimeSlabs[i].MinTaxSlab} and {maxSlabAmount}",
                        Value = slabAmount
                    };
                    taxSlab.Add(i + 1, taxSlabDetail);
                }

                i++;
            }

            employeeDeclaration.SurChargesAndCess = this.SurchargeAndCess(tax, grossIncome); //(tax * 4) / 100;
            //taxSlab.Add(taxRegimeSlabs.Count, new TaxSlabDetail { Description = "Gross Income Tax", Value = tax });
            taxSlab[0].Value = tax;
            employeeDeclaration.TaxNeedToPay = tax + employeeDeclaration.SurChargesAndCess;
            employeeDeclaration.IncomeTaxSlab = taxSlab;
        }

        public void HRAComponent(EmployeeDeclaration employeeDeclaration, List<CalculatedSalaryBreakupDetail> calculatedSalaryBreakupDetails)
        {
            var calculatedSalaryBreakupDetail = calculatedSalaryBreakupDetails.Find(x => x.ComponentId.ToUpper() == ComponentNames.HRA);
            if (calculatedSalaryBreakupDetail == null)
                calculatedSalaryBreakupDetail = new CalculatedSalaryBreakupDetail
                {
                    FinalAmount = 0
                };

            var basicComponent = calculatedSalaryBreakupDetails.Find(x => x.ComponentId.ToUpper() == ComponentNames.Basic);
            if (basicComponent == null)
                throw new HiringBellException("Invalid gross amount not found. Please contact to admin.");

            decimal HRA1 = calculatedSalaryBreakupDetail.FinalAmount;
            decimal HRA2 = basicComponent.FinalAmount / 2;
            decimal HRA3 = 0;
            decimal HRAAmount = 0;

            if (HRA1 < HRA2 && HRA1 > 0)
                HRAAmount = HRA1;
            else
                HRAAmount = HRA2;

            var houseProperty = employeeDeclaration.Declarations.Find(x => x.DeclarationName == ApplicationConstants.HouseProperty);
            if (houseProperty != null && houseProperty.TotalAmountDeclared > 0)
            {
                decimal declaredValue = houseProperty.TotalAmountDeclared;
                HRA3 = (declaredValue / 12) - (basicComponent.FinalAmount * .1M);

                if (HRA3 > 0 && HRA3 < HRAAmount)
                    HRAAmount = HRA3;
            }

            employeeDeclaration.HRADeatils = new EmployeeHRA { HRA1 = HRA1, HRA2 = HRA2, HRA3 = HRA3, HRAAmount = HRAAmount };

            var hraComponent = employeeDeclaration.SalaryComponentItems.Find(x => x.ComponentId == "HRA");
            if (houseProperty != null)
                hraComponent.DeclaredValue = houseProperty.TotalAmountDeclared;
        }

        public void BuildTaxDetail(long EmployeeId, EmployeeDeclaration employeeDeclaration, EmployeeSalaryDetail salaryBreakup)
        {
            List<TaxDetails> taxdetails = null;
            if (salaryBreakup.TaxDetail != null)
            {
                taxdetails = JsonConvert.DeserializeObject<List<TaxDetails>>(salaryBreakup.TaxDetail);

                if (taxdetails != null && taxdetails.Count > 0)
                {
                    decimal previousMonthTax = 0;
                    int i = taxdetails.FindIndex(x => x.Month == DateTime.Now.Month && x.Year == DateTime.Now.Year);
                    employeeDeclaration.TaxPaid = Convert.ToDecimal(string.Format("{0:0.00}", taxdetails.Select(x => x.TaxPaid).Aggregate((i, k) => i + k)));
                    int currentMonthIndex = i;
                    if (currentMonthIndex > 0)
                    {
                        previousMonthTax = taxdetails[currentMonthIndex - 1].TaxDeducted;
                    }

                    if (employeeDeclaration.TaxPaid == 0)
                        i = 0;

                    decimal currentMonthTax = Convert.ToDecimal(string.Format("{0:0.00}", ((employeeDeclaration.TaxNeedToPay - employeeDeclaration.TaxPaid) / (12 - i))));
                    while (i < taxdetails.Count)
                    {
                        taxdetails[i].TaxDeducted = currentMonthTax;
                        i++;
                    }
                }
            }
            else
            {
                if (employeeDeclaration.TaxNeedToPay > 0)
                {
                    var permonthTax = employeeDeclaration.TaxNeedToPay / 12;
                    taxdetails = new List<TaxDetails>();
                    DateTime financialYearMonth = new DateTime(DateTime.Now.Year, 4, 1);
                    int i = 0;
                    while (i <= 11)
                    {
                        taxdetails.Add(new TaxDetails
                        {
                            Month = financialYearMonth.AddMonths(i).Month,
                            Year = financialYearMonth.AddMonths(i).Year,
                            EmployeeId = EmployeeId,
                            TaxDeducted = Convert.ToDecimal(String.Format("{0:0.00}", permonthTax)),
                            TaxPaid = 0
                        });

                        i++;
                    }
                }
                employeeDeclaration.TaxPaid = 0;
            }
            salaryBreakup.TaxDetail = JsonConvert.SerializeObject(taxdetails);
        }

        public decimal OneAndHalfLakhsComponent(EmployeeDeclaration employeeDeclaration)
        {
            decimal totalDeduction = 0;
            var items = employeeDeclaration.Declarations.FindAll(x => x.DeclarationName == ApplicationConstants.OneAndHalfLakhsExemptions);
            items.ForEach(i =>
            {
                decimal value = i.TotalAmountDeclared;
                if (i.TotalAmountDeclared >= i.MaxAmount)
                    value = i.MaxAmount;
                else
                    value = i.TotalAmountDeclared;

                totalDeduction += value;
            });

            return totalDeduction;
        }

        public decimal OtherDeclarationComponent(EmployeeDeclaration employeeDeclaration)
        {
            decimal totalDeduction = 0;
            var items = employeeDeclaration.Declarations.FindAll(x => x.DeclarationName == ApplicationConstants.OtherDeclarationName);
            items.ForEach(i =>
            {
                totalDeduction = i.TotalAmountDeclared;
            });

            return totalDeduction;
        }

        public decimal TaxSavingComponent(EmployeeDeclaration employeeDeclaration)
        {
            decimal totalDeduction = 0;
            var items = employeeDeclaration.Declarations.FindAll(x => x.DeclarationName == ApplicationConstants.TaxSavingAlloanceName);
            items.ForEach(i =>
            {
                totalDeduction = i.TotalAmountDeclared;
            });

            return totalDeduction;
        }

        public decimal HousePropertyComponent(EmployeeDeclaration employeeDeclaration)
        {
            decimal totalDeduction = 0;
            var items = employeeDeclaration.Declarations.FindAll(x => x.DeclarationName == ApplicationConstants.HouseProperty);
            items.ForEach(i =>
            {
                totalDeduction = i.TotalAmountDeclared;
            });

            return totalDeduction;
        }
    }
}
