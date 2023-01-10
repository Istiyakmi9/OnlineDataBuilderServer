using BottomhalfCore.DatabaseLayer.Common.Code;
using BottomhalfCore.Services.Code;
using BottomhalfCore.Services.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using ModalLayer;
using ModalLayer.Modal;
using ModalLayer.Modal.Accounts;
using Newtonsoft.Json;
using ServiceLayer.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceLayer.Code
{
    public class DeclarationService : IDeclarationService
    {
        private readonly IDb _db;
        private readonly IFileService _fileService;
        private readonly FileLocationDetail _fileLocationDetail;
        private readonly CurrentSession _currentSession;
        private readonly Dictionary<string, List<string>> _sections;
        private readonly ISalaryComponentService _salaryComponentService;
        private readonly IComponentsCalculationService _componentsCalculationService;
        private readonly ITimezoneConverter _timezoneConverter;

        public DeclarationService(IDb db, IFileService fileService,
            FileLocationDetail fileLocationDetail,
            CurrentSession currentSession,
            IOptions<Dictionary<string, List<string>>> options,
            ISalaryComponentService salaryComponentService,
            IComponentsCalculationService componentsCalculationService,
            ITimezoneConverter timezoneConverter
            )
        {
            _db = db;
            _fileService = fileService;
            _fileLocationDetail = fileLocationDetail;
            _currentSession = currentSession;
            _sections = options.Value;
            _salaryComponentService = salaryComponentService;
            _componentsCalculationService = componentsCalculationService;
            _timezoneConverter = timezoneConverter;
        }

        public EmployeeDeclaration GetDeclarationByEmployee(long EmployeeId)
        {
            throw new NotImplementedException();
        }

        public async Task<EmployeeDeclaration> UpdateDeclarationDetail(long EmployeeDeclarationId, EmployeeDeclaration employeeDeclaration, IFormFileCollection FileCollection, List<Files> files)
        {
            EmployeeDeclaration empDeclaration = new EmployeeDeclaration();
            EmployeeDeclaration declaration = this.GetDeclarationById(EmployeeDeclarationId);
            SalaryComponents salaryComponent = null;
            if (declaration != null && !string.IsNullOrEmpty(declaration.DeclarationDetail))
            {
                declaration.SalaryComponentItems = JsonConvert.DeserializeObject<List<SalaryComponents>>(declaration.DeclarationDetail);
                salaryComponent = declaration.SalaryComponentItems.Find(x => x.ComponentId == employeeDeclaration.ComponentId);
                if (salaryComponent == null)
                    throw new HiringBellException("Requested component not found. Please contact to admin.");

                if (employeeDeclaration.DeclaredValue == 0)
                    throw new HiringBellException("Declaration value must be greater than 0. Please check your detail once.");

                salaryComponent.DeclaredValue = employeeDeclaration.DeclaredValue;
            }
            else
            {
                throw new HiringBellException("Requested component not found. Please contact to admin.");
            }

            await ExecuteDeclarationDetail(files, declaration, FileCollection, salaryComponent);
            return await this.GetEmployeeDeclarationDetail(employeeDeclaration.EmployeeId, true);
        }

        public EmployeeDeclaration GetDeclarationById(long EmployeeDeclarationId)
        {
            (List<EmployeeDeclaration> declarations, List<SalaryComponents> salaryComponents) = GetDeclarationWithComponents(EmployeeDeclarationId);
            if (declarations.Count != 1)
                throw new HiringBellException("Fail to get current employee declaration detail");
            return declarations.FirstOrDefault();
        }

        public (List<EmployeeDeclaration> declarations, List<SalaryComponents> salaryComponents) GetDeclarationWithComponents(long EmployeeDeclarationId)
        {
            (List<EmployeeDeclaration> declarations, List<SalaryComponents> salaryComponents) = _db.GetList<EmployeeDeclaration, SalaryComponents>("sp_employee_declaration_get_byId",
                new
                {
                    EmployeeDeclarationId = EmployeeDeclarationId
                });
            return (declarations, salaryComponents);
        }

        private async Task<bool> GetEmployeeDeclaration(EmployeeDeclaration employeeDeclaration, List<SalaryComponents> salaryComponents)
        {
            bool reCalculateFlag = false;

            if (string.IsNullOrEmpty(employeeDeclaration.DeclarationDetail))
            {
                employeeDeclaration.DeclarationDetail = JsonConvert.SerializeObject(salaryComponents);
                employeeDeclaration.SalaryComponentItems = salaryComponents;
                reCalculateFlag = true;
            }
            else
            {
                try
                {
                    employeeDeclaration.SalaryComponentItems = JsonConvert
                                       .DeserializeObject<List<SalaryComponents>>(employeeDeclaration.DeclarationDetail);

                    if (employeeDeclaration.SalaryComponentItems == null || employeeDeclaration.SalaryComponentItems.Count == 0)
                    {
                        employeeDeclaration.DeclarationDetail = JsonConvert.SerializeObject(salaryComponents);
                        employeeDeclaration.SalaryComponentItems = salaryComponents;
                        reCalculateFlag = true;
                        return await Task.FromResult(reCalculateFlag);
                    }

                    Parallel.ForEach(salaryComponents, x =>
                    {
                        if (employeeDeclaration.SalaryComponentItems.Find(i => i.ComponentId == x.ComponentId) == null)
                        {
                            employeeDeclaration.SalaryComponentItems.Add(x);
                            reCalculateFlag = true;
                        }
                    });
                }
                catch
                {
                    employeeDeclaration.DeclarationDetail = JsonConvert.SerializeObject(salaryComponents);
                    employeeDeclaration.SalaryComponentItems = salaryComponents;
                    reCalculateFlag = true;
                }
            }

            return await Task.FromResult(reCalculateFlag);
        }

        public async Task<EmployeeDeclaration> GetEmployeeDeclarationDetail(long EmployeeId, bool reCalculateFlag = false)
        {
            List<Files> files = default;
            EmployeeDeclaration employeeDeclaration = default;
            if (EmployeeId <= 0)
                throw new HiringBellException("Invalid employee selected. Please select a vlid employee");

            DataSet resultSet = _db.FetchDataSet("sp_employee_declaration_get_byEmployeeId", new
            {
                EmployeeId = EmployeeId,
                UserTypeId = (int)UserType.Compnay
            });

            if ((resultSet == null || resultSet.Tables.Count == 0) && resultSet.Tables.Count != 2)
                throw new HiringBellException("Unable to get the detail");

            employeeDeclaration = Converter.ToType<EmployeeDeclaration>(resultSet.Tables[0]);
            if (employeeDeclaration == null)
                throw new HiringBellException("Employee declaration detail not defined. Please contact to admin.");

            if (resultSet.Tables[1].Rows.Count > 0)
                files = Converter.ToList<Files>(resultSet.Tables[1]);

            employeeDeclaration.SalaryDetail = await this.CalculateSalaryDetail(EmployeeId, employeeDeclaration, reCalculateFlag);

            employeeDeclaration.FileDetails = files;
            employeeDeclaration.Sections = _sections;
            return employeeDeclaration;
        }

        private async Task UpdateDeclarationDetail(List<Files> files, EmployeeDeclaration declaration, IFormFileCollection FileCollection, HousingDeclartion housingDeclartion)
        {
            SalaryComponents salaryComponent = declaration.SalaryComponentItems.Find(x => x.ComponentId == housingDeclartion.ComponentId);
            if (salaryComponent == null)
                throw new HiringBellException("Requested component not found. Please contact to admin.");

            salaryComponent.DeclaredValue = housingDeclartion.HousePropertyDetail.TotalRent;
            declaration.HouseRentDetail = JsonConvert.SerializeObject(housingDeclartion.HousePropertyDetail);
            await ExecuteDeclarationDetail(files, declaration, FileCollection, salaryComponent);
        }

        private async Task ExecuteDeclarationDetail(List<Files> files, EmployeeDeclaration declaration, IFormFileCollection FileCollection, SalaryComponents salaryComponent)
        {
            try
            {
                _db.StartTransaction(IsolationLevel.ReadUncommitted);

                DbResult Result = null;
                List<int> fileIds = new List<int>();
                if (FileCollection.Count > 0)
                {
                    // save file to server filesystem
                    _fileService.SaveFileToLocation(declaration.DocumentPath, files, FileCollection);

                    foreach (var n in files)
                    {
                        Result = await _db.ExecuteAsync("sp_userfiledetail_Upload", new
                        {
                            FileId = n.FileUid,
                            FileOwnerId = declaration.EmployeeId,
                            FilePath = declaration.DocumentPath,
                            FileName = n.FileName,
                            FileExtension = n.FileExtension,
                            UserTypeId = (int)UserType.Compnay,
                            AdminId = _currentSession.CurrentUserDetail.UserId
                        }, true);

                        if (!Bot.IsSuccess(Result))
                            throw new HiringBellException("Fail to update housing property document detail. Please contact to admin.");

                        fileIds.Add(Convert.ToInt32(Result.statusMessage));
                    }
                }

                salaryComponent.UploadedFileIds = JsonConvert.SerializeObject(fileIds);
                declaration.DeclarationDetail = JsonConvert.SerializeObject(declaration.SalaryComponentItems);

                Result = await _db.ExecuteAsync("sp_employee_declaration_insupd", new
                {
                    EmployeeDeclarationId = declaration.EmployeeDeclarationId,
                    EmployeeId = declaration.EmployeeId,
                    DocumentPath = declaration.DocumentPath,
                    DeclarationDetail = declaration.DeclarationDetail,
                    HouseRentDetail = declaration.HouseRentDetail,
                    TotalDeclaredAmount = declaration.TotalDeclaredAmount,
                    TotalApprovedAmount = declaration.TotalApprovedAmount,
                    TotalRejectedAmount = declaration.TotalRejectedAmount,
                    EmployeeCurrentRegime = declaration.EmployeeCurrentRegime
                }, true);

                if (!Bot.IsSuccess(Result))
                    throw new HiringBellException("Fail to update housing property document detail. Please contact to admin.");

                _db.Commit();
            }
            catch
            {
                _db.RollBack();
                _fileService.DeleteFiles(files);
                throw;
            }
        }

        public async Task<EmployeeDeclaration> HouseRentDeclarationService(long EmployeeDeclarationId, HousingDeclartion DeclarationDetail, IFormFileCollection FileCollection, List<Files> files)
        {
            try
            {
                (List<EmployeeDeclaration> declarations, List<SalaryComponents> dbSalaryComponents) = this.GetDeclarationWithComponents(EmployeeDeclarationId);
                if (declarations.Count != 1)
                    throw new HiringBellException("Fail to get current employee declaration detail");

                EmployeeDeclaration declaration = declarations.FirstOrDefault();

                if (declaration == null || string.IsNullOrEmpty(declaration.DeclarationDetail))
                    throw new HiringBellException("Requested component not found. Please contact to admin.");

                declaration.SalaryComponentItems = JsonConvert.DeserializeObject<List<SalaryComponents>>(declaration.DeclarationDetail);

                if (FileCollection.Count > 0)
                {
                    var email = DeclarationDetail.Email.Replace("@", "_").Replace(".", "_");
                    if (string.IsNullOrEmpty(declaration.DocumentPath))
                    {
                        declaration.DocumentPath = Path.Combine(
                            _fileLocationDetail.UserFolder,
                            email,
                            ApplicationConstants.DeclarationDocumentPath
                        );
                    }
                }

                DeclarationDetail.ComponentId = ComponentNames.HRA;

                // update declaration detail with housing detail in database
                await UpdateDeclarationDetail(files, declaration, FileCollection, DeclarationDetail);
                return await this.GetEmployeeDeclarationDetail(DeclarationDetail.EmployeeId, true);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task GetEmployeeSalaryDetail(EmployeeCalculation employeeCalculation)
        {
            var ResultSet = _db.FetchDataSet("sp_salary_components_group_by_employeeid",
                new { employeeCalculation.EmployeeId });
            if (ResultSet == null || ResultSet.Tables.Count != 4)
                throw new HiringBellException("Unbale to get salary detail. Please contact to admin.");

            if (ResultSet.Tables[1].Rows.Count == 0)
                throw new HiringBellException($"Salary detail not found for employee Id: [{employeeCalculation.EmployeeId}]");

            if (ResultSet.Tables[2].Rows.Count == 0)
                throw new HiringBellException($"Employee company setting is not defined. Please contact to admin.");

            if (ResultSet.Tables[3].Rows.Count == 0)
                throw new Exception("Salary component are not defined, unable to perform calculation. Please contact to admin");

            employeeCalculation.salaryComponents = Converter.ToList<SalaryComponents>(ResultSet.Tables[3]);
            employeeCalculation.employeeSalaryDetail = Converter.ToType<EmployeeSalaryDetail>(ResultSet.Tables[1]);
            employeeCalculation.CTC = employeeCalculation.employeeSalaryDetail.CTC;
            employeeCalculation.salaryGroup = Converter.ToType<SalaryGroup>(ResultSet.Tables[0]);
            
            if (employeeCalculation.salaryGroup.SalaryGroupId == 1)
                employeeCalculation.employeeDeclaration.DefaultSlaryGroupMessage = $"Salary group for salary {employeeCalculation.CTC} not found. Default salary group for all calculation. For any query please contact to admin.";

            employeeCalculation.companySetting = Converter.ToType<CompanySetting>(ResultSet.Tables[2]);

            if (string.IsNullOrEmpty(employeeCalculation.salaryGroup.SalaryComponents))
                throw new HiringBellException($"Salary components not found for salary: [{employeeCalculation.employeeSalaryDetail.CTC}]");

            employeeCalculation.salaryGroup.GroupComponents = JsonConvert
                .DeserializeObject<List<SalaryComponents>>(employeeCalculation.salaryGroup.SalaryComponents);

            if (employeeCalculation.employeeDeclaration.SalaryDetail != null)
                employeeCalculation.employeeDeclaration.SalaryDetail.CTC = employeeCalculation.CTC;

            await Task.CompletedTask;
        }

        private List<CalculatedSalaryBreakupDetail> GetGrossIncome(EmployeeDeclaration employeeDeclaration, List<AnnualSalaryBreakup> completeSalaryBreakups)
        {
            List<CalculatedSalaryBreakupDetail> calculatedSalaryBreakupDetails = new List<CalculatedSalaryBreakupDetail>();

            var currentMonthDateTime = DateTime.UtcNow;
            TimeZoneInfo timeZoneInfo = _currentSession.TimeZone;
            if (timeZoneInfo != null)
                currentMonthDateTime = _timezoneConverter.ToIstTime(currentMonthDateTime);
            var currentMonthSalaryBreakup = completeSalaryBreakups.Find(i => i.MonthNumber == currentMonthDateTime.Month);
            if (currentMonthSalaryBreakup == null)
                throw new HiringBellException("Unable to find salary detail. Please contact to admin.");
            else
                calculatedSalaryBreakupDetails = currentMonthSalaryBreakup.SalaryBreakupDetails;

            var grossComponent = calculatedSalaryBreakupDetails.Find(x => x.ComponentId.ToUpper() == ComponentNames.Gross);
            if (grossComponent == null)
                throw new HiringBellException("Invalid gross amount not found. Please contact to admin.");

            employeeDeclaration.TotalAmount = grossComponent.FinalAmount * 12;

            return calculatedSalaryBreakupDetails;
        }

        public async Task<EmployeeSalaryDetail> CalculateSalaryDetail(long EmployeeId, EmployeeDeclaration employeeDeclaration, bool reCalculateFlag = false)
        {
            EmployeeCalculation employeeCalculation = new EmployeeCalculation
            {
                EmployeeId = EmployeeId,
                employeeDeclaration = employeeDeclaration,
                employee = new Employee { EmployeeUid = EmployeeId }
            };

            await GetEmployeeSalaryDetail(employeeCalculation);
            return await CalculateSalaryNDeclaration(employeeCalculation, reCalculateFlag);
        }

        private async Task<bool> CalculateAndBuildDeclarationDetail(EmployeeCalculation employeeCalculation, bool reCalculateFlag)
        {
            bool flag = await GetEmployeeDeclaration(employeeCalculation.employeeDeclaration, employeeCalculation.salaryComponents);
            if (!reCalculateFlag)
                reCalculateFlag = flag;

            if (employeeCalculation.salaryComponents.Count != employeeCalculation.employeeDeclaration.SalaryComponentItems.Count)
                throw new HiringBellException("Salary component and Employee declaration count is not match. Please contact to admin");

            this.BuildSectionWiseComponents(employeeCalculation);

            return await Task.FromResult(flag);
        }

        public async Task<EmployeeSalaryDetail> CalculateSalaryNDeclaration(EmployeeCalculation empCal, bool reCalculateFlag)
        {
            EmployeeSalaryDetail salaryBreakup = empCal.employeeSalaryDetail;

            List<AnnualSalaryBreakup> completeSalaryBreakups =
                JsonConvert.DeserializeObject<List<AnnualSalaryBreakup>>(salaryBreakup.CompleteSalaryDetail);

            await CalculateAndBuildDeclarationDetail(empCal, reCalculateFlag);
            if (completeSalaryBreakups.Count == 0 || empCal.employee.IsCTCChanged)
            {
                completeSalaryBreakups = _salaryComponentService.CreateSalaryBreakupWithValue(empCal);
                if (completeSalaryBreakups == null || completeSalaryBreakups.Count == 0)
                    throw new HiringBellException("Unable to build salary detail. Please contact to admin.");

                salaryBreakup.CompleteSalaryDetail = JsonConvert.SerializeObject(completeSalaryBreakups);
                reCalculateFlag = true;
            }
            else
            {
                if (empCal.salaryGroup == null || string.IsNullOrEmpty(empCal.salaryGroup.SalaryComponents)
                    || empCal.salaryGroup.SalaryComponents == "[]")
                    throw new HiringBellException("Salary group or its component not defined. Please contact to admin.");

                empCal.salaryGroup.GroupComponents = JsonConvert
                    .DeserializeObject<List<SalaryComponents>>(empCal.salaryGroup.SalaryComponents);
            }

            decimal totalDeduction = 0;

            // calculate and get gross income value and salary breakup detail
            var calculatedSalaryBreakupDetails = GetGrossIncome(empCal.employeeDeclaration, completeSalaryBreakups);

            //check and apply standard deduction
            totalDeduction += _componentsCalculationService.StandardDeductionComponent(empCal);

            // check and apply professional tax
            var ptaxSlab = _db.GetList<PTaxSlab>("sp_ptax_slab_getby_compId", new {CompanyId = empCal.companySetting.CompanyId});
            if (ptaxSlab == null || ptaxSlab.Count == 0)
                throw new HiringBellException("Professional tax slab is not declared. Please add PTax slab");

            totalDeduction += _componentsCalculationService.ProfessionalTaxComponent(empCal, ptaxSlab);

            // check and apply employer providentfund
            totalDeduction += _componentsCalculationService.EmployerProvidentFund(empCal.employeeDeclaration, empCal.salaryGroup);

            // check and apply 1.5 lakhs components
            totalDeduction += _componentsCalculationService.OneAndHalfLakhsComponent(empCal.employeeDeclaration);

            // check and apply other components
            totalDeduction += _componentsCalculationService.OtherDeclarationComponent(empCal.employeeDeclaration);

            // check and apply tax saving components
            totalDeduction += _componentsCalculationService.TaxSavingComponent(empCal.employeeDeclaration);

            // check and apply house property components
            // totalDeduction += _componentsCalculationService.HousePropertyComponent(employeeDeclaration);

            decimal hraAmount = 0;
            salaryBreakup.GrossIncome = empCal.employeeDeclaration.TotalAmount;

            // Calculate hra and apply on deduction
            _componentsCalculationService.HRAComponent(empCal.employeeDeclaration, calculatedSalaryBreakupDetails);

            //Convert.ToDecimal(string.Format("{0:0.00}", employeeDeclaration.HRADeatils.TryGetValue("HRAAmount", out hraAmount)));
            if (empCal.employeeDeclaration.HRADeatils != null)
                hraAmount = (empCal.employeeDeclaration.HRADeatils.HRAAmount * 12);

            // final total taxable amount.
            empCal.employeeDeclaration.TotalAmount = empCal.employeeDeclaration.TotalAmount - (totalDeduction + hraAmount);

            //Tax regime calculation 
            if (empCal.employeeDeclaration.TotalAmount < 0)
                empCal.employeeDeclaration.TotalAmount = 0;

            var taxRegimeSlabs = _db.GetList<TaxRegime>("sp_tax_regime_by_id_age", new
            {
                RegimeDescId = empCal.employeeDeclaration.EmployeeCurrentRegime,
                empCal.EmployeeId
            });
            var surchargeSlabs = _db.GetList<SurChargeSlab>("sp_surcharge_slab_getall");
            if (surchargeSlabs == null || surchargeSlabs.Count == 0)
                throw new Exception("Surcharge slabs are not found. Please add surcharge slsb first");

            _componentsCalculationService.TaxRegimeCalculation(empCal.employeeDeclaration, salaryBreakup.GrossIncome, taxRegimeSlabs, surchargeSlabs);

            //Tac Calculation for every month
            await TaxDetailsCalculation(empCal, reCalculateFlag);

            return salaryBreakup;
        }

        private async Task UpdateEmployeeSalaryDetailChanges(long EmployeeId, EmployeeSalaryDetail salaryBreakup)
        {
            var result = await _db.ExecuteAsync("sp_employee_salary_detail_InsUpd", new
            {
                EmployeeId,
                salaryBreakup.CTC,
                salaryBreakup.GrossIncome,
                salaryBreakup.NetSalary,
                salaryBreakup.CompleteSalaryDetail,
                salaryBreakup.GroupId,
                salaryBreakup.TaxDetail,
            }, true);

            if (!Bot.IsSuccess(result.statusMessage))
                throw new HiringBellException("Fail to save calculation detail. Please contact to admin.");
        }

        private async Task TaxDetailsCalculation(EmployeeCalculation empCal, bool reCalculateFlag)
        {
            DateTime presentDate = _timezoneConverter.ToTimeZoneDateTime(DateTime.UtcNow, _currentSession.TimeZone);
            List<TaxDetails> taxdetails = null;
            if (empCal.companySetting == null)
                throw new HiringBellException("Company setting not found. Please contact to admin.");

            if (empCal.employeeSalaryDetail.TaxDetail != null)
            {
                decimal totalTaxNeedToPay = 0;
                taxdetails = JsonConvert.DeserializeObject<List<TaxDetails>>(empCal.employeeSalaryDetail.TaxDetail);
                if (taxdetails != null && taxdetails.Count > 0)
                    totalTaxNeedToPay = Convert.ToDecimal(taxdetails.Select(x => x.TaxDeducted).Aggregate((i, k) => i + k));

                if (totalTaxNeedToPay > 0)
                {
                    if (reCalculateFlag)
                    {
                        empCal.employeeDeclaration.TaxPaid = Convert.ToDecimal(taxdetails
                            .Select(x => x.TaxPaid).Aggregate((i, k) => i + k));

                        decimal remaningTaxAmount = empCal.employeeDeclaration.TaxNeedToPay - empCal.employeeDeclaration.TaxPaid;
                        DateTime financialYearMonth = new DateTime(empCal.companySetting.FinancialYear, empCal.companySetting.DeclarationStartMonth, 1);

                        int useCurrentMonth = 0;
                        if (presentDate.Day <= empCal.companySetting.EveryMonthLastDayOfDeclaration)
                            useCurrentMonth = 1;

                        int remaningMonths = (12 - (presentDate.Month - financialYearMonth.Month + 1) + useCurrentMonth);
                        presentDate = presentDate.AddMonths(useCurrentMonth == 0 ? 1 : 0);

                        TaxDetails taxDetail = default(TaxDetails);
                        decimal singleMonthTax = Convert.ToDecimal((remaningTaxAmount / remaningMonths));
                        while (presentDate.Month != (empCal.companySetting.DeclarationEndMonth + 1))
                        {
                            taxDetail = taxdetails.FirstOrDefault(x => x.Month == presentDate.Month);
                            if (taxDetail != null)
                            {
                                taxDetail.TaxDeducted = singleMonthTax;
                                taxDetail.TaxPaid = 0M;
                            }

                            presentDate = presentDate.AddMonths(1);
                        }

                        empCal.employeeSalaryDetail.TaxDetail = JsonConvert.SerializeObject(taxdetails);
                        await UpdateEmployeeSalaryDetailChanges(empCal.employeeDeclaration.EmployeeId, empCal.employeeSalaryDetail);
                    }
                    else
                    {
                        empCal.employeeDeclaration.TaxPaid = Convert.ToDecimal(taxdetails
                                .Select(x => x.TaxPaid).Aggregate((i, k) => i + k));
                    }
                }
                else
                {
                    taxdetails = GetPerMontTaxInitialData(empCal.companySetting, empCal.employeeDeclaration);
                    empCal.employeeDeclaration.TaxPaid = Convert.ToDecimal(taxdetails
                                .Select(x => x.TaxPaid).Aggregate((i, k) => i + k));

                    empCal.employeeSalaryDetail.TaxDetail = JsonConvert.SerializeObject(taxdetails);
                    await UpdateEmployeeSalaryDetailChanges(empCal.employeeDeclaration.EmployeeId, empCal.employeeSalaryDetail);
                }
            }
            else
            {
                if (empCal.employeeDeclaration.TaxNeedToPay > 0)
                {
                    taxdetails = GetPerMontTaxInitialData(empCal.companySetting, empCal.employeeDeclaration);
                }
                else
                {
                    taxdetails = GetPerMontTaxDetail(empCal.companySetting, empCal.employeeDeclaration.EmployeeId);
                }

                empCal.employeeSalaryDetail.TaxDetail = JsonConvert.SerializeObject(taxdetails);
                await UpdateEmployeeSalaryDetailChanges(empCal.employeeDeclaration.EmployeeId, empCal.employeeSalaryDetail);
            }
        }

        private List<TaxDetails> GetPerMontTaxDetail(CompanySetting companySetting, long EmployeeId)
        {
            var taxdetails = new List<TaxDetails>();
            DateTime financialYearMonth = new DateTime(companySetting.FinancialYear, companySetting.DeclarationStartMonth, 1);
            int i = 0;
            while (i <= 11)
            {
                taxdetails.Add(new TaxDetails
                {
                    Month = financialYearMonth.AddMonths(i).Month,
                    Year = financialYearMonth.AddMonths(i).Year,
                    EmployeeId = EmployeeId,
                    TaxDeducted = 0,
                    TaxPaid = 0
                });
                i++;
            }

            return taxdetails;
        }

        private List<TaxDetails> GetPerMontTaxInitialData(CompanySetting companySetting, EmployeeDeclaration employeeDeclaration)
        {
            var permonthTax = employeeDeclaration.TaxNeedToPay / 12;
            employeeDeclaration.TaxPaid = 0;
            List<TaxDetails> taxdetails = new List<TaxDetails>();
            DateTime financialYearMonth = new DateTime(companySetting.FinancialYear, companySetting.DeclarationStartMonth, 1);
            int i = 0;
            while (i <= 11)
            {
                taxdetails.Add(new TaxDetails
                {
                    Month = financialYearMonth.AddMonths(i).Month,
                    Year = financialYearMonth.AddMonths(i).Year,
                    EmployeeId = employeeDeclaration.EmployeeId,
                    TaxDeducted = permonthTax,
                    TaxPaid = 0
                });
                i++;
            }

            return taxdetails;
        }

        private void BuildSectionWiseComponents(EmployeeCalculation employeeCalculation)
        {
            EmployeeDeclaration employeeDeclaration = employeeCalculation.employeeDeclaration;
            foreach (var x in _sections)
            {
                switch (x.Key)
                {
                    case ApplicationConstants.ExemptionDeclaration:
                        employeeDeclaration.ExemptionDeclaration = employeeDeclaration.SalaryComponentItems.FindAll(i => i.Section != null && x.Value.Contains(i.Section));
                        employeeDeclaration.Declarations.Add(new DeclarationReport
                        {
                            DeclarationName = ApplicationConstants.OneAndHalfLakhsExemptions,
                            NumberOfProofSubmitted = 0,
                            Declarations = employeeDeclaration.ExemptionDeclaration.Where(x => x.DeclaredValue > 0).Select(i => i.Section).ToList(),
                            AcceptedAmount = employeeDeclaration.ExemptionDeclaration.Sum(a => a.AcceptedAmount),
                            RejectedAmount = employeeDeclaration.ExemptionDeclaration.Sum(a => a.RejectedAmount),
                            TotalAmountDeclared = employeeDeclaration.ExemptionDeclaration.Sum(a => a.DeclaredValue),
                            MaxAmount = 150000
                        });
                        break;
                    case ApplicationConstants.OtherDeclaration:
                        employeeDeclaration.OtherDeclaration = employeeDeclaration.SalaryComponentItems.FindAll(i => i.Section != null && x.Value.Contains(i.Section));
                        employeeDeclaration.Declarations.Add(new DeclarationReport
                        {
                            DeclarationName = ApplicationConstants.OtherDeclarationName,
                            NumberOfProofSubmitted = 0,
                            Declarations = employeeDeclaration.OtherDeclaration.Where(x => x.DeclaredValue > 0).Select(i => i.Section).ToList(),
                            AcceptedAmount = employeeDeclaration.OtherDeclaration.Sum(a => a.AcceptedAmount),
                            RejectedAmount = employeeDeclaration.OtherDeclaration.Sum(a => a.RejectedAmount),
                            TotalAmountDeclared = employeeDeclaration.OtherDeclaration.Sum(a => a.DeclaredValue)
                        });
                        break;
                    case ApplicationConstants.TaxSavingAlloance:
                        employeeDeclaration.TaxSavingAlloance = employeeDeclaration.SalaryComponentItems.FindAll(i => i.Section != null && x.Value.Contains(i.Section));
                        employeeDeclaration.Declarations.Add(new DeclarationReport
                        {
                            DeclarationName = ApplicationConstants.TaxSavingAlloanceName,
                            NumberOfProofSubmitted = 0,
                            Declarations = employeeDeclaration.TaxSavingAlloance.Where(x => x.DeclaredValue > 0).Select(i => i.Section).ToList(),
                            AcceptedAmount = employeeDeclaration.TaxSavingAlloance.Sum(a => a.AcceptedAmount),
                            RejectedAmount = employeeDeclaration.TaxSavingAlloance.Sum(a => a.RejectedAmount),
                            TotalAmountDeclared = employeeDeclaration.TaxSavingAlloance.Sum(a => a.DeclaredValue)
                        });
                        break;
                    case ApplicationConstants.Section16TaxExemption:
                        employeeDeclaration.Section16TaxExemption = employeeDeclaration.SalaryComponentItems.FindAll(i => i.Section != null && x.Value.Contains(i.Section));
                        break;
                }
            };

            var houseProperty = employeeDeclaration.SalaryComponentItems.FindAll(x => x.ComponentId.ToLower() == ComponentNames.HRA.ToLower());
            employeeDeclaration.Declarations.Add(new DeclarationReport
            {
                DeclarationName = ComponentNames.HRA,
                NumberOfProofSubmitted = 0,
                Declarations = employeeDeclaration.TaxSavingAlloance.Where(x => x.DeclaredValue > 0).Select(i => i.Section).ToList(),
                AcceptedAmount = houseProperty.Sum(a => a.AcceptedAmount),
                RejectedAmount = houseProperty.Sum(a => a.RejectedAmount),
                TotalAmountDeclared = houseProperty.Sum(a => a.DeclaredValue)
            });

            employeeDeclaration.Declarations.Add(new DeclarationReport
            {
                DeclarationName = ApplicationConstants.IncomeFromOtherSources,
                NumberOfProofSubmitted = 0,
                Declarations = new List<string>(),
                AcceptedAmount = 0,
                RejectedAmount = 0,
                TotalAmountDeclared = 0
            });
        }

        public async Task<string> UpdateTaxDetailsService(long EmployeeId, int PresentMonth, int PresentYear)
        {
            if (EmployeeId <= 0)
                throw new HiringBellException("Invalid employeeId used. Please login again.");

            (EmployeeDeclaration employeeDeclaration, EmployeeSalaryDetail employeeSalaryDetail) =
                _db.GetMulti<EmployeeDeclaration, EmployeeSalaryDetail>("sp_employee_declaration_detail_get_by_empid", new { EmployeeId });
            if (employeeDeclaration == null || employeeSalaryDetail == null)
                throw new HiringBellException("Ohh!! fail to get employee declaration or salary detail. Please contact to admin.");

            List<TaxDetails> breakDetail = null;
            TaxDetails workingDetail = null;
            if (!string.IsNullOrEmpty(employeeSalaryDetail.TaxDetail))
            {
                breakDetail = JsonConvert.DeserializeObject<List<TaxDetails>>(employeeSalaryDetail.TaxDetail);
                workingDetail = breakDetail.FirstOrDefault(x => x.Month == PresentMonth && x.Year == PresentYear);
                if (workingDetail == null)
                    employeeSalaryDetail = await this.CalculateSalaryDetail(EmployeeId, employeeDeclaration);
            }
            else
            {
                employeeSalaryDetail = await this.CalculateSalaryDetail(EmployeeId, employeeDeclaration);
                breakDetail = JsonConvert.DeserializeObject<List<TaxDetails>>(employeeSalaryDetail.TaxDetail);
                workingDetail = breakDetail.FirstOrDefault(x => x.Month == PresentMonth && x.Year == PresentYear);
                if (workingDetail == null)
                    throw new HiringBellException("Fail to calculate salary detail. Look's there are some internal issue. Please contact to admin.");
            }


            foreach (var elem in breakDetail)
            {
                if (elem.Month <= PresentMonth && elem.Month > 3)
                    elem.TaxPaid = elem.TaxDeducted;
            }

            var Result = await _db.ExecuteAsync("sp_employee_salary_detail_upd_salarydetail", new
            {
                EmployeeId = employeeSalaryDetail.EmployeeId,
                CompleteSalaryDetail = employeeSalaryDetail.CompleteSalaryDetail,
                TaxDetail = JsonConvert.SerializeObject(breakDetail),
                CTC = employeeSalaryDetail.CTC,
            }, true);

            return Result.statusMessage;
        }

        public string SwitchEmployeeTaxRegimeService(EmployeeDeclaration employeeDeclaration)
        {
            if (employeeDeclaration.EmployeeId == 0)
                throw new HiringBellException("Invalid employee selected. Please select a valid employee");

            if (employeeDeclaration.EmployeeCurrentRegime == 0)
                throw new HiringBellException("Please select a valid tx regime type");

            var result = _db.Execute<EmployeeDeclaration>("sp_employee_taxregime_update",
                new { EmployeeId = employeeDeclaration.EmployeeId, EmployeeCurrentRegime = employeeDeclaration.EmployeeCurrentRegime }, true);
            if (string.IsNullOrEmpty(result))
                throw new HiringBellException("Fail to switch the tax regime");
            return result;
        }

        public async Task<EmployeeDeclaration> DeleteDeclarationValueService(long DeclarationId, string ComponentId)
        {
            if (DeclarationId <= 0)
                throw new HiringBellException("Invalid declaration id passed.");

            if (string.IsNullOrEmpty(ComponentId))
                throw new HiringBellException("Invalid declaration component selected. Please select a valid component");

            var resultset = _db.FetchDataSet("sp_employee_declaration_get_byId", new { EmployeeDeclarationId = DeclarationId });
            EmployeeDeclaration declaration = Converter.ToType<EmployeeDeclaration>(resultset.Tables[0]);
            List<SalaryComponents> salaryComponent = Converter.ToList<SalaryComponents>(resultset.Tables[1]);
            if (declaration == null || salaryComponent == null)
                throw new HiringBellException("Declaration detail not found. Please contact to admin.");

            List<SalaryComponents> salaryComponents = JsonConvert.DeserializeObject<List<SalaryComponents>>(declaration.DeclarationDetail);
            var component = salaryComponents.FirstOrDefault(x => x.ComponentId == ComponentId);
            if (component == null)
                throw new HiringBellException("Got internal error while cleaning up, please contact to admin.");
            return await ResetComponent(declaration, salaryComponents, component);
        }

        public async Task<EmployeeDeclaration> DeleteDeclaredHRAService(long DeclarationId)
        {
            if (DeclarationId <= 0)
                throw new HiringBellException("Invalid declaration id passed.");

            string ComponentId = ComponentNames.HRA;

            var resultset = _db.FetchDataSet("sp_employee_declaration_get_byId", new { EmployeeDeclarationId = DeclarationId });
            EmployeeDeclaration declaration = Converter.ToType<EmployeeDeclaration>(resultset.Tables[0]);
            List<SalaryComponents> salaryComponent = Converter.ToList<SalaryComponents>(resultset.Tables[1]);
            if (declaration == null || salaryComponent == null)
                throw new HiringBellException("Declaration detail not found. Please contact to admin.");

            declaration.HouseRentDetail = ApplicationConstants.EmptyJsonObject;

            List<SalaryComponents> salaryComponents = JsonConvert.DeserializeObject<List<SalaryComponents>>(declaration.DeclarationDetail);
            var component = salaryComponents.FirstOrDefault(x => x.ComponentId == ComponentId);
            if (component == null)
                throw new HiringBellException("Got internal error while cleaning up, please contact to admin.");

            return await ResetComponent(declaration, salaryComponents, component);
        }

        private async Task<EmployeeDeclaration> ResetComponent(EmployeeDeclaration declaration, List<SalaryComponents> salaryComponents, SalaryComponents component)
        {
            try
            {
                var allFileIds = JsonConvert.DeserializeObject<List<long>>(component.UploadedFileIds);
                string searchString = component.UploadedFileIds.Replace("[", "").Replace("]", "");
                List<Files> files = _db.GetList<Files>("sp_userfiledetail_get_files", new { searchString });

                component.DeclaredValue = 0;
                component.UploadedFileIds = "[]";

                declaration.DeclarationDetail = JsonConvert.SerializeObject(salaryComponents);

                _db.StartTransaction(IsolationLevel.ReadUncommitted);

                DbResult Result = null;
                if (files != null && files.Count > 0)
                {
                    foreach (var file in allFileIds)
                    {
                        Result = await _db.ExecuteAsync("sp_userdetail_del_by_file_id", new { FileId = file }, true);
                        if (!Bot.IsSuccess(Result))
                            throw new HiringBellException("Fail to delete file record, Please contact to admin.");
                    }
                }

                await _db.ExecuteAsync("sp_employee_declaration_insupd", new
                {
                    declaration.EmployeeDeclarationId,
                    declaration.EmployeeId,
                    declaration.DocumentPath,
                    declaration.DeclarationDetail,
                    declaration.HouseRentDetail,
                    declaration.TotalDeclaredAmount,
                    declaration.TotalApprovedAmount,
                    declaration.TotalRejectedAmount,
                    declaration.EmployeeCurrentRegime
                }, true);

                if (files != null)
                    _fileService.DeleteFiles(files);

                _db.Commit();

                return await this.GetEmployeeDeclarationDetail(declaration.EmployeeId, true);
            }
            catch
            {
                _db.RollBack();
                throw;
            }
        }

        public async Task<EmployeeDeclaration> DeleteDeclarationFileService(long DeclarationId, int FileId, string ComponentId)
        {
            try
            {
                if (DeclarationId <= 0)
                    throw new HiringBellException("Invalid declaration id passed. Please try again.");

                if (FileId <= 0)
                    throw new HiringBellException("Invalid file selected. Please select a valid file");

                if (string.IsNullOrEmpty(ComponentId))
                    throw new HiringBellException("Invalid declaration component selected. Please select a valid component");

                (EmployeeDeclaration declaration, Files file) = _db.GetMulti<EmployeeDeclaration, Files>("sp_employee_declaration_and_file_get", new { DeclarationId, FileId });
                if (declaration == null)
                    throw new HiringBellException("Declaration detail not found. Please contact to admin.");

                List<SalaryComponents> salaryComponents = JsonConvert.DeserializeObject<List<SalaryComponents>>(declaration.DeclarationDetail);
                var component = salaryComponents.FirstOrDefault(x => x.ComponentId == ComponentId);
                if (component != null)
                {
                    var fileIds = JsonConvert.DeserializeObject<List<long>>(component.UploadedFileIds);
                    var existingFileId = fileIds.FirstOrDefault(i => i == FileId);
                    fileIds.Remove(existingFileId);
                    component.UploadedFileIds = JsonConvert.SerializeObject(fileIds);
                    declaration.DeclarationDetail = JsonConvert.SerializeObject(salaryComponents);
                }

                _db.StartTransaction(IsolationLevel.ReadUncommitted);

                var Result = await _db.ExecuteAsync("sp_userdetail_del_by_file_id", new { FileId }, true);
                if (ApplicationConstants.IsExecuted(Result.statusMessage))
                {
                    await _db.ExecuteAsync("sp_employee_declaration_insupd", new
                    {
                        declaration.EmployeeDeclarationId,
                        declaration.EmployeeId,
                        declaration.DocumentPath,
                        declaration.DeclarationDetail,
                        declaration.HouseRentDetail,
                        declaration.TotalDeclaredAmount,
                        declaration.TotalApprovedAmount,
                        declaration.TotalRejectedAmount,
                        declaration.EmployeeCurrentRegime
                    }, true);

                    if (file != null)
                        _fileService.DeleteFiles(new List<Files> { file });
                }
                _db.Commit();
                return await this.GetEmployeeDeclarationDetail(declaration.EmployeeId, false);
                //return await Task.FromResult(ApplicationConstants.Successfull);
            }
            catch
            {
                _db.RollBack();
                throw;
            }
        }
    }
}