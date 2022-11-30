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
            string declarationDoc = String.Empty;
            try
            {
                EmployeeDeclaration empDeclaration = new EmployeeDeclaration();
                EmployeeDeclaration declaration = this.GetDeclarationById(EmployeeDeclarationId);
                List<SalaryComponents> salaryComponents = new List<SalaryComponents>();
                SalaryComponents salaryComponent = null;
                if (declaration != null && !string.IsNullOrEmpty(declaration.DeclarationDetail))
                {
                    salaryComponents = JsonConvert.DeserializeObject<List<SalaryComponents>>(declaration.DeclarationDetail);
                    salaryComponent = salaryComponents.Find(x => x.ComponentId == employeeDeclaration.ComponentId);
                    if (salaryComponent == null)
                        throw new HiringBellException("Requested component not found. Please contact to admin.");
                }
                else
                {
                    throw new HiringBellException("Requested component not found. Please contact to admin.");
                }

                var email = employeeDeclaration.Email.Replace("@", "_").Replace(".", "_");
                declarationDoc = Path.Combine(
                    _fileLocationDetail.UserFolder,
                    email,
                    "declarated_documents"
                );

                long fileIndex = 1;
                FileDetail fileDetail = _db.Get<FileDetail>("sp_userfiledetail_get_last_id", null);
                if (fileDetail != null)
                    fileIndex = fileDetail.FileId;

                //files.ForEach(x => x.FileId = fileIndex + 1);
                var i = 0;
                while(i < files.Count)
                {
                    files[i].FileId = fileIndex + (i+1);
                    i++;
                }
                _db.StartTransaction(IsolationLevel.ReadUncommitted);

                DbResult dbResult = null;
                if (FileCollection.Count > 0)
                {
                    _fileService.SaveFileToLocation(declarationDoc, files, FileCollection);

                    var fileInfo = (from n in files
                                    select new
                                    {
                                        FileId = n.FileId,
                                        FileOwnerId = (employeeDeclaration.EmployeeId),
                                        FilePath = declarationDoc,
                                        FileName = n.FileName,
                                        FileExtension = n.FileExtension,
                                        UserTypeId = (int)UserType.Compnay,
                                        AdminId = _currentSession.CurrentUserDetail.UserId
                                    });

                    DataTable table = Converter.ToDataTable(fileInfo);
                    dbResult = await _db.BatchInsertUpdateAsync("sp_userfiledetail_Upload", table, false);
                }

                salaryComponent.DeclaredValue = employeeDeclaration.DeclaredValue;
                salaryComponent.UploadedFileIds = JsonConvert.SerializeObject(files.Select(x => x.FileId).ToList<long>());
                declaration.DeclarationDetail = JsonConvert.SerializeObject(salaryComponents);
                var result = _db.Execute<EmployeeDeclaration>("sp_employee_declaration_insupd", new
                {
                    EmployeeDeclarationId = declaration.EmployeeDeclarationId,
                    EmployeeId = declaration.EmployeeId,
                    DocumentPath = declarationDoc,
                    DeclarationDetail = declaration.DeclarationDetail,
                    HousingProperty = declaration.HousingProperty,
                    TotalDeclaredAmount = declaration.TotalDeclaredAmount,
                    TotalApprovedAmount = declaration.TotalApprovedAmount,
                    TotalRejectedAmount = declaration.TotalRejectedAmount,
                    EmployeeCurrentRegime = declaration.EmployeeCurrentRegime
                }, true);

                if (!ApplicationConstants.IsExecuted(result))
                {
                    _fileService.DeleteFiles(files);
                }

                _db.Commit();

                //empDeclaration.SalaryComponentItems = salaryComponents;
                //this.BuildSectionWiseComponents(empDeclaration);
                //EmployeeSalaryDetail employeeSalaryDetail = this.CalculateSalaryDetail(employeeDeclaration.EmployeeId, empDeclaration);

                //result = _db.Execute<EmployeeSalaryDetail>("sp_employee_salary_detail_InsUpd", employeeSalaryDetail, true);
                //if (string.IsNullOrEmpty(result))
                //    throw new HiringBellException("Unable to insert or update salary breakup");


                return this.GetEmployeeDeclarationDetail(employeeDeclaration.EmployeeId, true);
            }
            catch
            {
                _db.RollBack();
                if (!string.IsNullOrEmpty(declarationDoc))
                    File.Delete(declarationDoc);
                throw;
            }
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

        public EmployeeDeclaration GetEmployeeDeclarationDetail(long EmployeeId, bool reCalculateFlag = false)
        {
            List<Files> files = default;
            EmployeeDeclaration employeeDeclaration = default;
            DataSet resultSet = _db.FetchDataSet("sp_employee_declaration_get_byEmployeeId", new
            {
                EmployeeId = EmployeeId,
                UserTypeId = (int)UserType.Compnay
            });

            if ((resultSet == null || resultSet.Tables.Count == 0) && resultSet.Tables.Count != 4)
                throw new HiringBellException("Unable to get the detail");

            employeeDeclaration = Converter.ToType<EmployeeDeclaration>(resultSet.Tables[0]);
            if (resultSet.Tables[1].Rows.Count > 0)
                files = Converter.ToList<Files>(resultSet.Tables[1]);

            if (resultSet.Tables[2].Rows.Count == 1)
                employeeDeclaration.SalaryDetail = Converter.ToType<EmployeeSalaryDetail>(resultSet.Tables[2]);
            else
                throw new HiringBellException("Employee salary detail not defined. Please check CTC and other detail.");

            if (employeeDeclaration.DeclarationDetail != null)
                employeeDeclaration.SalaryComponentItems = JsonConvert.DeserializeObject<List<SalaryComponents>>(employeeDeclaration.DeclarationDetail);
            else
                throw new Exception("Declaration component are null. Please contact to admin");

            List<SalaryComponents> salaryComponents = Converter.ToList<SalaryComponents>(resultSet.Tables[3]);
            if (salaryComponents.Count <= 0)
                throw new Exception("Salary component are null. Please contact to admin");

            if (salaryComponents.Count != employeeDeclaration.SalaryComponentItems.Count)
                throw new HiringBellException("Salary component and Employee declaration count is not match. Please contact to admin");

            Parallel.ForEach(salaryComponents, x =>
            {
                if (employeeDeclaration.SalaryComponentItems != null)
                {
                    if (employeeDeclaration.SalaryComponentItems.Find(i => i.ComponentId == x.ComponentId) == null)
                        employeeDeclaration.SalaryComponentItems.Add(x);
                }
                else
                {
                    employeeDeclaration.SalaryComponentItems = salaryComponents;
                }
            });

            if (employeeDeclaration.SalaryComponentItems != null)
            {
                this.BuildSectionWiseComponents(employeeDeclaration);
                this.CalculateSalaryDetail(EmployeeId, employeeDeclaration, employeeDeclaration.SalaryDetail.CTC, reCalculateFlag);
            }

            employeeDeclaration.FileDetails = files;
            employeeDeclaration.DeclarationDetail = null;
            employeeDeclaration.Sections = _sections;
            return employeeDeclaration;
        }

        public async Task<EmployeeDeclaration> HousingPropertyDeclarationService(long EmployeeDeclarationId, HousingDeclartion DeclarationDetail, IFormFileCollection FileCollection, List<Files> files)
        {
            EmployeeDeclaration empDeclaration = new EmployeeDeclaration();
            (List<EmployeeDeclaration> declarations, List<SalaryComponents> dbSalaryComponents) = this.GetDeclarationWithComponents(EmployeeDeclarationId);
            if (declarations.Count != 1)
                throw new HiringBellException("Fail to get current employee declaration detail");

            EmployeeDeclaration declaration = declarations.FirstOrDefault();


            List<SalaryComponents> salaryComponents = new List<SalaryComponents>();
            if (declaration != null)
            {
                salaryComponents = JsonConvert.DeserializeObject<List<SalaryComponents>>(declaration.DeclarationDetail);

                Parallel.ForEach(dbSalaryComponents, x =>
                {
                    if (salaryComponents.Find(i => i.ComponentId == x.ComponentId) == null)
                        salaryComponents.Add(x);
                });

                SalaryComponents salaryComponent = salaryComponents.Find(x => x.ComponentId == DeclarationDetail.ComponentId);
                if (salaryComponent == null)
                    throw new HiringBellException("Requested component not found. Please contact to admin.");
                salaryComponent.DeclaredValue = DeclarationDetail.HousePropertyDetail.TotalRent;

                declaration.DeclarationDetail = JsonConvert.SerializeObject(salaryComponents);
            }
            else
            {
                throw new HiringBellException("Requested component not found. Please contact to admin.");
            }

            string declarationDoc = String.Empty;
            if (FileCollection.Count > 0)
            {
                var email = DeclarationDetail.Email.Replace("@", "_").Replace(".", "_");
                declarationDoc = Path.Combine(
                    _fileLocationDetail.UserFolder,
                    email,
                    "declarated_documents"
                );

                _fileService.SaveFileToLocation(declarationDoc, files, FileCollection);

                var fileInfo = (from n in files
                                select new
                                {
                                    FileId = n.FileUid,
                                    FileOwnerId = (DeclarationDetail.EmployeeId),
                                    FilePath = declarationDoc,
                                    FileName = n.FileName,
                                    FileExtension = n.FileExtension,
                                    UserTypeId = (int)UserType.Compnay,
                                    AdminId = _currentSession.CurrentUserDetail.UserId
                                });

                DataTable table = Converter.ToDataTable(fileInfo);
                _db.StartTransaction(IsolationLevel.ReadUncommitted);
                var insertedCount = await _db.BatchInsertUpdateAsync("sp_userfiledetail_Upload", table, false);
                _db.Commit();
            }
            var housingTax = JsonConvert.SerializeObject(DeclarationDetail.HousePropertyDetail);
            var result = _db.Execute<EmployeeDeclaration>("sp_employee_declaration_insupd", new
            {
                EmployeeDeclarationId = declaration.EmployeeDeclarationId,
                EmployeeId = declaration.EmployeeId,
                DocumentPath = declarationDoc,
                DeclarationDetail = declaration.DeclarationDetail,
                HousingProperty = housingTax,
                TotalDeclaredAmount = 0,
                TotalApprovedAmount = 0
            }, true);

            if (!ApplicationConstants.IsExecuted(result))
            {
                File.Delete(declarationDoc);
            }

            empDeclaration.SalaryComponentItems = salaryComponents;
            empDeclaration.HousingProperty = housingTax;
            this.BuildSectionWiseComponents(empDeclaration);

            EmployeeSalaryDetail employeeSalaryDetail = this.CalculateSalaryDetail(DeclarationDetail.EmployeeId, empDeclaration);

            result = _db.Execute<EmployeeSalaryDetail>("sp_employee_salary_detail_InsUpd", employeeSalaryDetail, true);
            if (string.IsNullOrEmpty(result))
                throw new HiringBellException("Unable to insert or update salary breakup");

            return this.GetEmployeeDeclarationDetail(DeclarationDetail.EmployeeId);
        }

        private (EmployeeSalaryDetail, SalaryGroup) GetEmployeeSalaryDetail(long EmployeeId, EmployeeDeclaration employeeDeclaration, decimal CTC = 0)
        {
            EmployeeSalaryDetail salaryBreakup = null;
            DbParam[] param = new DbParam[]
            {
                new DbParam(EmployeeId, typeof(long), "_EmployeeId"),
                new DbParam(CTC, typeof(decimal), "_CTC")
            };

            var resultSet = _db.GetDataset("sp_salary_components_group_by_employeeid", param);
            if (resultSet == null || resultSet.Tables.Count != 2)
                throw new HiringBellException("Unbale to get salary detail. Please contact to admin.");

            SalaryGroup salaryGroup = Converter.ToType<SalaryGroup>(resultSet.Tables[0]);
            if (salaryGroup == null || salaryGroup.SalaryGroupId == 0)
                throw new HiringBellException("No group found for the current employee salary package. Please create one.");

            if (!string.IsNullOrEmpty(salaryGroup.SalaryComponents))
            {
                salaryGroup.GroupComponents = JsonConvert.DeserializeObject<List<SalaryComponents>>(salaryGroup.SalaryComponents);

                if (resultSet.Tables[1].Rows.Count == 1)
                {
                    salaryBreakup = Converter.ToType<EmployeeSalaryDetail>(resultSet.Tables[1]);
                    if (salaryBreakup == null)
                        throw new HiringBellException("Unbale to get salary detail. Please contact to admin.");
                    if (employeeDeclaration.SalaryDetail != null)
                        employeeDeclaration.SalaryDetail.CTC = salaryBreakup.CTC;
                }
                else
                {
                    salaryBreakup = new EmployeeSalaryDetail
                    {
                        CompleteSalaryDetail = "[]",
                        CTC = employeeDeclaration.SalaryDetail.CTC,
                        EmployeeId = EmployeeId,
                        GrossIncome = 0,
                        GroupId = 0,
                        NetSalary = 0,
                        TaxDetail = "[]"
                    };
                }
            }

            return (salaryBreakup, salaryGroup);
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

        public EmployeeSalaryDetail CalculateSalaryDetail(long EmployeeId, EmployeeDeclaration employeeDeclaration, decimal CTC = 0, bool reCalculateFlag = false)
        {
            bool flag = false;
            (EmployeeSalaryDetail salaryBreakup, SalaryGroup salaryGroup) = GetEmployeeSalaryDetail(EmployeeId, employeeDeclaration, CTC);

            if (salaryBreakup == null || salaryGroup == null)
                return null;

            List<AnnualSalaryBreakup> completeSalaryBreakups =
                JsonConvert.DeserializeObject<List<AnnualSalaryBreakup>>(salaryBreakup.CompleteSalaryDetail);

            if (completeSalaryBreakups.Count == 0)
            {
                completeSalaryBreakups = _salaryComponentService.CreateSalaryBreakupWithValue(EmployeeId, salaryBreakup.CTC);
                if (completeSalaryBreakups == null || completeSalaryBreakups.Count == 0)
                    throw new HiringBellException("Unable to build salary detail. Please contact to admin.");

                salaryBreakup.CompleteSalaryDetail = JsonConvert.SerializeObject(completeSalaryBreakups);
                flag = true;
            }

            // calculate and get gross income value and salary breakup detail
            var calculatedSalaryBreakupDetails = GetGrossIncome(employeeDeclaration, completeSalaryBreakups);
            decimal StandardDeduction = _componentsCalculationService.StandardDeductionComponent(employeeDeclaration);

            // check and apply professional tax
            _componentsCalculationService.ProfessionalTaxComponent(employeeDeclaration, salaryGroup);

            // check and apply employer providentfund
            _componentsCalculationService.EmployerProvidentFund(employeeDeclaration, salaryGroup);

            // check and apply 1.5 lakhs components
            decimal totalDeduction = _componentsCalculationService.OneAndHalfLakhsComponent(employeeDeclaration);

            decimal hraAmount = 0;
            salaryBreakup.GrossIncome = employeeDeclaration.TotalAmount;
            employeeDeclaration.SalaryDetail = salaryBreakup;

            // Calculate hra and apply on deduction
            _componentsCalculationService.HRAComponent(employeeDeclaration, calculatedSalaryBreakupDetails);

            //Convert.ToDecimal(string.Format("{0:0.00}", employeeDeclaration.HRADeatils.TryGetValue("HRAAmount", out hraAmount)));
            if (employeeDeclaration.HRADeatils != null)
                hraAmount = (employeeDeclaration.HRADeatils.HRAAmount * 12);

            employeeDeclaration.TotalAmount = Convert.ToDecimal(string.Format("{0:0.00}", (employeeDeclaration.TotalAmount - (StandardDeduction + totalDeduction + hraAmount))));

            //Tax regime calculation 
            var ageGroup = DateTime.UtcNow.Year - Convert.ToDateTime(_currentSession.CurrentUserDetail.Dob).Year;
            if (employeeDeclaration.TotalAmount < 0)
                employeeDeclaration.TotalAmount = 0;
            var taxRegimeSlabs = _db.GetList<TaxRegime>("sp_tax_regime_by_id_age", new
            {
                RegimeDescId = employeeDeclaration.EmployeeCurrentRegime,
                EmployeeId
            });

            _componentsCalculationService.TaxRegimeCalculation(employeeDeclaration, salaryBreakup.GrossIncome, taxRegimeSlabs);

            //Tac Calculation for every month
            flag = TaxDetailsCalculation(salaryBreakup, employeeDeclaration, reCalculateFlag);

            if (flag)
            {
                var result = _db.ExecuteAsync("sp_employee_salary_detail_InsUpd", new
                {
                    EmployeeId,
                    salaryBreakup.CTC,
                    salaryBreakup.GrossIncome,
                    salaryBreakup.NetSalary,
                    salaryBreakup.CompleteSalaryDetail,
                    salaryBreakup.GroupId,
                    salaryBreakup.TaxDetail,
                }, true);
            }

            return salaryBreakup;
        }

        private bool TaxDetailsCalculation(EmployeeSalaryDetail salaryBreakup, EmployeeDeclaration employeeDeclaration, bool reCalculateFlag)
        {
            bool flag = true;
            List<TaxDetails> taxdetails = null;

            CompanySetting companySetting = _db.Get<CompanySetting>("sp_company_setting_get_byid", new
            {
                CompanyId = _currentSession.CurrentUserDetail.CompanyId
            });

            if (companySetting == null)
                throw new HiringBellException("Company setting not found. Please contact to admin.");

            if (salaryBreakup.TaxDetail != null)
            {
                taxdetails = JsonConvert.DeserializeObject<List<TaxDetails>>(salaryBreakup.TaxDetail);
                if (taxdetails.Count > 0)
                {
                    decimal totalTaxNeedToPay = Convert.ToDecimal(taxdetails.Select(x => x.TaxDeducted)
                        .Aggregate((i, k) => i + k));
                    if (reCalculateFlag)
                    {
                        if (totalTaxNeedToPay == 0)
                        {
                            taxdetails = GetPerMontTaxInitialData(companySetting, employeeDeclaration);
                        }
                        else
                        {
                            DateTime presentDate = _timezoneConverter.ToTimeZoneDateTime(DateTime.UtcNow, _currentSession.TimeZone);
                            employeeDeclaration.TaxPaid = Convert.ToDecimal(taxdetails
                                .Select(x => x.TaxPaid).Aggregate((i, k) => i + k));

                            decimal remaningTaxAmount = employeeDeclaration.TaxNeedToPay - employeeDeclaration.TaxPaid;
                            DateTime financialYearMonth = new DateTime(companySetting.FinancialYear, companySetting.DeclarationStartMonth, 1);

                            // need to check if last day of month is over then next calculation
                            // would be effected from next month else from current month
                            int useCurrentMonth = 0;
                            if (presentDate.Day <= companySetting.EveryMonthLastDayOfDeclaration)
                                useCurrentMonth = 1;

                            int remaningMonths = (12 - (presentDate.Month - financialYearMonth.Month + 1) + useCurrentMonth);
                            presentDate = presentDate.AddMonths(useCurrentMonth == 0 ? 1 : 0);

                            TaxDetails taxDetail = default(TaxDetails);
                            decimal singleMonthTax = Convert.ToDecimal((remaningTaxAmount / remaningMonths));
                            while (presentDate.Month != (companySetting.DeclarationEndMonth + 1))
                            {
                                taxDetail = taxdetails.FirstOrDefault(x => x.Month == presentDate.Month);
                                if (taxDetail != null)
                                {
                                    taxDetail.TaxDeducted = singleMonthTax;
                                    taxDetail.TaxPaid = 0M;
                                }

                                presentDate = presentDate.AddMonths(1);
                            }

                            salaryBreakup.TaxDetail = JsonConvert.SerializeObject(taxdetails);
                            return flag;
                        }
                    }
                    else
                    {
                        employeeDeclaration.TaxPaid = Convert.ToDecimal(taxdetails
                                .Select(x => x.TaxPaid).Aggregate((i, k) => i + k));
                        return false;
                    }
                }
            }

            if (employeeDeclaration.TaxNeedToPay > 0)
            {
                taxdetails = GetPerMontTaxInitialData(companySetting, employeeDeclaration);
            }
            else
            {
                taxdetails = GetPerMontTaxDetail(companySetting, employeeDeclaration.EmployeeId);
            }

            salaryBreakup.TaxDetail = JsonConvert.SerializeObject(taxdetails);
            return flag;
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

        private void BuildSectionWiseComponents(EmployeeDeclaration employeeDeclaration)
        {
            foreach (var x in _sections)
            {
                switch (x.Key)
                {
                    case "ExemptionDeclaration":
                        employeeDeclaration.ExemptionDeclaration = employeeDeclaration.SalaryComponentItems.FindAll(i => i.Section != null && x.Value.Contains(i.Section));
                        employeeDeclaration.Declarations.Add(new DeclarationReport
                        {
                            DeclarationName = "1.5 Lac Exemptions",
                            NumberOfProofSubmitted = 0,
                            Declarations = employeeDeclaration.ExemptionDeclaration.Where(x => x.DeclaredValue > 0).Select(i => i.Section).ToList(),
                            AcceptedAmount = employeeDeclaration.ExemptionDeclaration.Sum(a => a.AcceptedAmount),
                            RejectedAmount = employeeDeclaration.ExemptionDeclaration.Sum(a => a.RejectedAmount),
                            TotalAmountDeclared = employeeDeclaration.ExemptionDeclaration.Sum(a => a.DeclaredValue),
                        });
                        break;
                    case "OtherDeclaration":
                        employeeDeclaration.OtherDeclaration = employeeDeclaration.SalaryComponentItems.FindAll(i => i.Section != null && x.Value.Contains(i.Section));
                        employeeDeclaration.Declarations.Add(new DeclarationReport
                        {
                            DeclarationName = "Other Exemptions",
                            NumberOfProofSubmitted = 0,
                            Declarations = employeeDeclaration.OtherDeclaration.Where(x => x.DeclaredValue > 0).Select(i => i.Section).ToList(),
                            AcceptedAmount = employeeDeclaration.OtherDeclaration.Sum(a => a.AcceptedAmount),
                            RejectedAmount = employeeDeclaration.OtherDeclaration.Sum(a => a.RejectedAmount),
                            TotalAmountDeclared = employeeDeclaration.OtherDeclaration.Sum(a => a.DeclaredValue)
                        });
                        break;
                    case "TaxSavingAlloance":
                        employeeDeclaration.TaxSavingAlloance = employeeDeclaration.SalaryComponentItems.FindAll(i => i.Section != null && x.Value.Contains(i.Section));
                        employeeDeclaration.Declarations.Add(new DeclarationReport
                        {
                            DeclarationName = "Tax Saving Allowance",
                            NumberOfProofSubmitted = 0,
                            Declarations = employeeDeclaration.TaxSavingAlloance.Where(x => x.DeclaredValue > 0).Select(i => i.Section).ToList(),
                            AcceptedAmount = employeeDeclaration.TaxSavingAlloance.Sum(a => a.AcceptedAmount),
                            RejectedAmount = employeeDeclaration.TaxSavingAlloance.Sum(a => a.RejectedAmount),
                            TotalAmountDeclared = employeeDeclaration.TaxSavingAlloance.Sum(a => a.DeclaredValue)
                        });
                        break;
                }
            };

            var houseProperty = employeeDeclaration.SalaryComponentItems.FindAll(x => x.ComponentId.ToLower() == "HP".ToLower());
            employeeDeclaration.Declarations.Add(new DeclarationReport
            {
                DeclarationName = "House Property",
                NumberOfProofSubmitted = 0,
                Declarations = employeeDeclaration.TaxSavingAlloance.Where(x => x.DeclaredValue > 0).Select(i => i.Section).ToList(),
                AcceptedAmount = houseProperty.Sum(a => a.AcceptedAmount),
                RejectedAmount = houseProperty.Sum(a => a.RejectedAmount),
                TotalAmountDeclared = houseProperty.Sum(a => a.DeclaredValue)
            });

            employeeDeclaration.Declarations.Add(new DeclarationReport
            {
                DeclarationName = "Income From Other Sources",
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
                    employeeSalaryDetail = this.CalculateSalaryDetail(EmployeeId, employeeDeclaration, employeeSalaryDetail.CTC);
            }
            else
            {
                employeeSalaryDetail = this.CalculateSalaryDetail(EmployeeId, employeeDeclaration, employeeSalaryDetail.CTC);
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

            var resultset= _db.FetchDataSet("sp_employee_declaration_get_byId", new { EmployeeDeclarationId = DeclarationId });
            EmployeeDeclaration declaration = Converter.ToType<EmployeeDeclaration>(resultset.Tables[0]);
            List<SalaryComponents> salaryComponent = Converter.ToList<SalaryComponents>(resultset.Tables[1]);
            if (declaration == null || salaryComponent == null)
                throw new HiringBellException("Declaration detail not found. Please contact to admin.");

            List<SalaryComponents> salaryComponents = JsonConvert.DeserializeObject<List<SalaryComponents>>(declaration.DeclarationDetail);
            var component = salaryComponents.FirstOrDefault(x => x.ComponentId == ComponentId);
            var allFileIds = JsonConvert.DeserializeObject<List<long>>(component.UploadedFileIds);
            string searchString = $"FileId in ({component.UploadedFileIds})".Replace("[", "").Replace("]", "");
            List<Files> files = _db.GetList<Files>("SP_get_multiple_file_by_filter", new { searchString });
            if (component != null)
            {
                var newComponent = salaryComponent.FirstOrDefault(x => x.ComponentId == ComponentId);
                if (newComponent != null)
                {
                    component.DeclaredValue = newComponent.DeclaredValue;
                    component.UploadedFileIds = newComponent.UploadedFileIds;
                }

                declaration.DeclarationDetail = JsonConvert.SerializeObject(salaryComponents);
            }
            var fileInfo = (from n in allFileIds
                            select new
                            {
                                FileId = n
                            });
            DataTable table = Converter.ToDataTable(fileInfo);
            _db.StartTransaction(IsolationLevel.ReadUncommitted);
            var insertedCount = await _db.BatchInsertUpdateAsync("sp_UserDetail_del_by_file_id", table, true);
            if (insertedCount.rowsEffected > 0)
            {
                await _db.ExecuteAsync("sp_employee_declaration_insupd", new
                {
                    declaration.EmployeeDeclarationId,
                    declaration.EmployeeId,
                    declaration.DocumentPath,
                    declaration.DeclarationDetail,
                    declaration.HousingProperty,
                    declaration.TotalDeclaredAmount,
                    declaration.TotalApprovedAmount,
                    declaration.TotalRejectedAmount,
                    declaration.EmployeeCurrentRegime
                }, true);

                _fileService.DeleteFiles(files);
            }
            _db.Commit();
             return this.GetEmployeeDeclarationDetail(declaration.EmployeeId);
        }

        public async Task<string> DeleteDeclarationFileService(long DeclarationId, int FileId, string ComponentId)
        {
            if (DeclarationId <= 0)
                throw new HiringBellException("Invalid declaration id passed. Please try again.");

            if (FileId <= 0)
                throw new HiringBellException("Invalid file selected. Please select a valid file");

            if (string.IsNullOrEmpty(ComponentId))
                throw new HiringBellException("Invalid declaration component selected. Please select a valid component");

            (EmployeeDeclaration declaration, Files file) = _db.GetMulti<EmployeeDeclaration, Files>("sp_employee_declaration_and_file_get", new { DeclarationId, FileId });
            if (declaration == null || file == null)
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

            var Result = await _db.ExecuteAsync("sp_UserDetail_del_by_file_id", new { FileId }, true);
            if (Result.rowsEffected > 0)
            {
                await _db.ExecuteAsync("sp_employee_declaration_insupd", new
                {
                    declaration.EmployeeDeclarationId,
                    declaration.EmployeeId,
                    declaration.DocumentPath,
                    declaration.DeclarationDetail,
                    declaration.HousingProperty,
                    declaration.TotalDeclaredAmount,
                    declaration.TotalApprovedAmount,
                    declaration.TotalRejectedAmount,
                    declaration.EmployeeCurrentRegime
                }, true);

                _fileService.DeleteFiles(new List<Files> { file });
            }
            _db.Commit();
            return Result.statusMessage;
        }
    }
}