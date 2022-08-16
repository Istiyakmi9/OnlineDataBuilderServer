using BottomhalfCore.DatabaseLayer.Common.Code;
using BottomhalfCore.Services.Code;
using BottomhalfCore.Services.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
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

        public EmployeeDeclaration UpdateDeclarationDetail(long EmployeeDeclarationId, EmployeeDeclaration employeeDeclaration, IFormFileCollection FileCollection, List<Files> files)
        {
            EmployeeDeclaration empDeclaration = new EmployeeDeclaration();
            EmployeeDeclaration declaration = this.GetDeclarationById(EmployeeDeclarationId);
            List<SalaryComponents> salaryComponents = new List<SalaryComponents>();
            if (declaration != null)
            {
                salaryComponents = JsonConvert.DeserializeObject<List<SalaryComponents>>(declaration.DeclarationDetail);
                SalaryComponents salaryComponent = salaryComponents.Find(x => x.ComponentId == employeeDeclaration.ComponentId);
                if (salaryComponent == null)
                    throw new HiringBellException("Requested component not found. Please contact to admin.");
                salaryComponent.DeclaredValue = employeeDeclaration.DeclaredValue;

                declaration.DeclarationDetail = JsonConvert.SerializeObject(salaryComponents);
            }
            else
            {
                throw new HiringBellException("Requested component not found. Please contact to admin.");
            }

            string declarationDoc = String.Empty;
            if (FileCollection.Count > 0)
            {
                var email = employeeDeclaration.Email.Replace("@", "_").Replace(".", "_");
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
                                    FileOwnerId = (employeeDeclaration.EmployeeId),
                                    FilePath = declarationDoc,
                                    FileName = n.FileName,
                                    FileExtension = n.FileExtension,
                                    UserTypeId = (int)UserType.Compnay,
                                    AdminId = _currentSession.CurrentUserDetail.UserId
                                });

                DataTable table = Converter.ToDataTable(fileInfo);
                _db.StartTransaction(IsolationLevel.ReadUncommitted);
                int insertedCount = _db.BatchInsert("sp_userfiledetail_Upload", table, false);
                _db.Commit();
            }

            var result = _db.Execute<EmployeeDeclaration>("sp_employee_declaration_insupd", new
            {
                EmployeeDeclarationId = declaration.EmployeeDeclarationId,
                EmployeeId = declaration.EmployeeId,
                DocumentPath = declarationDoc,
                DeclarationDetail = declaration.DeclarationDetail,
                HousingProperty = declaration.HousingProperty,
                TotalDeclaredAmount = 0,
                TotalApprovedAmount = 0
            }, true);

            if (!ApplicationConstants.IsExecuted(result))
            {
                File.Delete(declarationDoc);
            }

            empDeclaration.SalaryComponentItems = salaryComponents;
            this.BuildSectionWiseComponents(empDeclaration);
            EmployeeSalaryDetail employeeSalaryDetail = this.CalculateSalaryDetail(employeeDeclaration.EmployeeId, empDeclaration);

            result = _db.Execute<EmployeeSalaryDetail>("sp_employee_salary_detail_InsUpd", employeeSalaryDetail, true);
            if (string.IsNullOrEmpty(result))
                throw new HiringBellException("Unable to insert or update salary breakup");

            return empDeclaration;
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

        public EmployeeDeclaration GetEmployeeDeclarationDetailById(long EmployeeId)
        {
            List<Files> files = default;
            EmployeeDeclaration employeeDeclaration = default;
            DbParam[] param = new DbParam[]
            {
                new DbParam(EmployeeId, typeof(long), "_EmployeeId"),
                new DbParam(UserType.Compnay, typeof(int), "_UserTypeId")
            };

            DataSet resultSet = _db.GetDataset("sp_employee_declaration_get_byEmployeeId", param);
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

            List<SalaryComponents> salaryComponents = Converter.ToList<SalaryComponents>(resultSet.Tables[3]);
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
                this.CalculateSalaryDetail(EmployeeId, employeeDeclaration);
            }

            employeeDeclaration.FileDetails = files;
            employeeDeclaration.DeclarationDetail = null;
            employeeDeclaration.Sections = _sections;
            return employeeDeclaration;
        }

        public EmployeeDeclaration HousingPropertyDeclarationService(long EmployeeDeclarationId, HousingDeclartion DeclarationDetail, IFormFileCollection FileCollection, List<Files> files)
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
                int insertedCount = _db.BatchInsert("sp_userfiledetail_Upload", table, false);
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

            return empDeclaration;
        }

        private (EmployeeSalaryDetail, SalaryGroup) GetEmployeeSalaryDetail(long EmployeeId, EmployeeDeclaration employeeDeclaration, decimal CTC = 0)
        {
            DbParam[] param = new DbParam[]
            {
                new DbParam(EmployeeId, typeof(long), "_EmployeeId"),
                new DbParam(CTC, typeof(decimal), "_CTC")
            };

            var resultSet = _db.GetDataset("sp_salary_components_group_by_employeeid", param);
            if (resultSet == null || resultSet.Tables.Count != 2)
                throw new HiringBellException("Unbale to get salary detail. Please contact to admin.");

            SalaryGroup salaryGroup = Converter.ToType<SalaryGroup>(resultSet.Tables[0]);
            if (salaryGroup == null)
                throw new HiringBellException("No group found for the current employee salary package. Please create one.");

            salaryGroup.GroupComponents = JsonConvert.DeserializeObject<List<SalaryComponents>>(salaryGroup.SalaryComponents);

            EmployeeSalaryDetail salaryBreakup = null;
            if (resultSet.Tables[1].Rows.Count == 1)
            {
                salaryBreakup = Converter.ToType<EmployeeSalaryDetail>(resultSet.Tables[1]);
                if (salaryBreakup == null)
                    throw new HiringBellException("Unbale to get salary detail. Please contact to admin.");
                if (employeeDeclaration.SalaryDetail != null)
                    salaryBreakup.CTC = employeeDeclaration.SalaryDetail.CTC;
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

            return (salaryBreakup, salaryGroup);
        }

        public EmployeeSalaryDetail CalculateSalaryDetail(long EmployeeId, EmployeeDeclaration employeeDeclaration, decimal CTC = 0)
        {
            (EmployeeSalaryDetail salaryBreakup, SalaryGroup salaryGroup) = GetEmployeeSalaryDetail(EmployeeId, employeeDeclaration, CTC);

            List<CalculatedSalaryBreakupDetail> calculatedSalaryBreakupDetails = new List<CalculatedSalaryBreakupDetail>();
            var annualSalaryBreakups = _salaryComponentService.SalaryBreakupCalcService(EmployeeId, salaryBreakup.CTC);
            if (annualSalaryBreakups == null || annualSalaryBreakups.Count == 0)
                throw new HiringBellException("Unable to build salary detail. Please contact to admin.");
            else
            {
                var currentMonthDateTime = DateTime.UtcNow;
                TimeZoneInfo timeZoneInfo = _currentSession.TimeZone;
                if (timeZoneInfo != null)
                    currentMonthDateTime = _timezoneConverter.ToIstTime(currentMonthDateTime);
                var currentMonthSalaryBreakup = annualSalaryBreakups.Find(i => i.MonthNumber == currentMonthDateTime.Month);
                if (currentMonthSalaryBreakup == null)
                    throw new HiringBellException("Unable to find salary detail. Please contact to admin.");
                else
                    calculatedSalaryBreakupDetails = currentMonthSalaryBreakup.SalaryBreakupDetails;
            }

            var grossComponent = calculatedSalaryBreakupDetails.Find(x => x.ComponentId.ToUpper() == ComponentNames.Gross);
            if (grossComponent == null)
                throw new HiringBellException("Invalid gross amount not found. Please contact to admin.");

            employeeDeclaration.TotalAmount = grossComponent.FinalAmount;

            decimal StandardDeduction = _componentsCalculationService.StandardDeductionComponent(employeeDeclaration);


            // check and apply professional tax
            _componentsCalculationService.ProfessionalTaxComponent(employeeDeclaration, salaryGroup);

            // check and apply employer providentfund
            _componentsCalculationService.EmployerProvidentFund(employeeDeclaration, salaryGroup);


            // check and apply 1.5 lakhs components
            decimal totalDeduction = _componentsCalculationService.OneAndHalfLakhsComponent(employeeDeclaration);

            salaryBreakup.GrossIncome = grossComponent.FinalAmount;
            salaryBreakup.CompleteSalaryDetail = JsonConvert.SerializeObject(annualSalaryBreakups);
            employeeDeclaration.SalaryDetail = salaryBreakup;
            employeeDeclaration.TotalAmount = Convert.ToDecimal(string.Format("{0:0.00}", (employeeDeclaration.TotalAmount - (StandardDeduction + totalDeduction))));

            // Calculate income detail based on old regime
            _componentsCalculationService.OldTaxRegimeCalculation(employeeDeclaration, salaryBreakup.GrossIncome);

            // Calculate hra and apply on deduction
            _componentsCalculationService.HRAComponent(employeeDeclaration, calculatedSalaryBreakupDetails);

            return salaryBreakup;
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
    }
}