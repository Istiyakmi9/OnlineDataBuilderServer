using BottomhalfCore.DatabaseLayer.Common.Code;
using BottomhalfCore.Services.Code;
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

        public DeclarationService(IDb db, IFileService fileService, FileLocationDetail fileLocationDetail, CurrentSession currentSession, IOptions<Dictionary<string, List<string>>> options, ISalaryComponentService salaryComponentService)
        {
            _db = db;
            _fileService = fileService;
            _fileLocationDetail = fileLocationDetail;
            _currentSession = currentSession;
            _sections = options.Value;
            _salaryComponentService = salaryComponentService;
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
                HousingProperty = declaration.HousingProperty
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
            var employeeDeclaration = _db.Get<EmployeeDeclaration>("sp_employee_declaration_get_byId", new { EmployeeDeclarationId = EmployeeDeclarationId });
            return employeeDeclaration;
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
            if ((resultSet == null || resultSet.Tables.Count == 0) && resultSet.Tables.Count != 3)
                throw new HiringBellException("Unable to get the detail");

            employeeDeclaration = Converter.ToType<EmployeeDeclaration>(resultSet.Tables[0]);
            if (resultSet.Tables[1].Rows.Count > 0)
                files = Converter.ToList<Files>(resultSet.Tables[1]);

            if (resultSet.Tables[2].Rows.Count == 1)
                employeeDeclaration.SalaryDetail = Converter.ToType<EmployeeSalaryDetail>(resultSet.Tables[2]);

            employeeDeclaration.SalaryComponentItems = JsonConvert.DeserializeObject<List<SalaryComponents>>(employeeDeclaration.DeclarationDetail);
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
            EmployeeDeclaration declaration = this.GetDeclarationById(EmployeeDeclarationId);
            List<SalaryComponents> salaryComponents = new List<SalaryComponents>();
            if (declaration != null)
            {
                salaryComponents = JsonConvert.DeserializeObject<List<SalaryComponents>>(declaration.DeclarationDetail);
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
                HousingProperty = housingTax
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

        public EmployeeSalaryDetail CalculateSalaryDetail(long EmployeeId, EmployeeDeclaration employeeDeclaration, decimal CTC = 0)
        {
            DbParam[] param = new DbParam[]
            {
                new DbParam(EmployeeId, typeof(long), "_EmployeeId"),
                new DbParam(CTC, typeof(decimal), "_CTC")
            };

            var resultSet = _db.GetDataset("sp_salary_components_group_by_employeeid", param);
            if (resultSet == null || resultSet.Tables.Count != 2)
            {
                throw new HiringBellException("Unbale to get salary detail. Please contact to admin.");
            }


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
                    CompleteSalaryDetail = "{}",
                    CTC = employeeDeclaration.SalaryDetail.CTC,
                    EmployeeId = EmployeeId,
                    GrossIncome = 0,
                    GroupId = 0,
                    NetSalary = 0,
                    TaxDetail = "[]"
                };
            }

            CompleteSalaryBreakup completeSalaryBreakup = _salaryComponentService.SalaryBreakupCalcService(EmployeeId, salaryBreakup.CTC);
            employeeDeclaration.TotalAmount = completeSalaryBreakup.GrossAnnually;
            decimal StandardDeduction = 50000;

            SalaryComponents component = null;
            component = salaryGroup.GroupComponents.Find(x => x.ComponentId == "PTAX");
            if (component != null)
                employeeDeclaration.TotalAmount = employeeDeclaration.TotalAmount - component.DeclaredValue;

            component = salaryGroup.GroupComponents.Find(x => x.ComponentId == "EPF");
            if (component != null)
                employeeDeclaration.TotalAmount = employeeDeclaration.TotalAmount - component.DeclaredValue;

            decimal totalDeduction = 0;
            foreach (var item in employeeDeclaration.Declarations)
            {
                decimal value = item.TotalAmountDeclared;
                if (item.DeclarationName == "1.5 Lac Exemptions")
                {
                    if (item.TotalAmountDeclared >= 150000)
                        value = 150000;
                    else
                        value = item.TotalAmountDeclared;
                }
                totalDeduction += value;
            }

            salaryBreakup.GrossIncome = completeSalaryBreakup.GrossAnnually;
            salaryBreakup.CompleteSalaryDetail = JsonConvert.SerializeObject(completeSalaryBreakup);
            employeeDeclaration.SalaryDetail = salaryBreakup;
            employeeDeclaration.TotalAmount = Convert.ToDecimal(string.Format("{0:0.00}", (employeeDeclaration.TotalAmount - (StandardDeduction + totalDeduction))));
            var incomeTaxDetails = this.OldTaxRegimeCalculation(employeeDeclaration.TotalAmount);
            employeeDeclaration.TaxNeedToPay = Convert.ToDecimal(string.Format("{0:0.00}", incomeTaxDetails.GetType().GetProperty("TotalTax").GetValue(incomeTaxDetails, null)));
            employeeDeclaration.IncomeTaxSlab = incomeTaxDetails.GetType().GetProperty("IncomeTaxSlab").GetValue(incomeTaxDetails, null);
            bool IsBuildTaxDetail = false;
            List<TaxDetails> taxdetails = null;
            if (salaryBreakup.TaxDetail != null)
            {
                taxdetails = JsonConvert.DeserializeObject<List<TaxDetails>>(salaryBreakup.TaxDetail);

                if (taxdetails.Count > 0)
                {
                    decimal previousMonthTax = 0;
                    int i = taxdetails.FindIndex(x => x.Month == DateTime.Now.Month && x.Year == DateTime.Now.Year);
                    employeeDeclaration.TaxPaid = Convert.ToDecimal(string.Format("{0:0.00}",taxdetails.Select(x => x.TaxPaid).Aggregate((i, k) => i + k)));
                    int currentMonthIndex = i;
                    if (currentMonthIndex > 0)
                    {
                        previousMonthTax = taxdetails[currentMonthIndex - 1].TaxDeducted;
                    }
                    decimal currentMonthTax = Convert.ToDecimal(string.Format("{0:0.00}",((employeeDeclaration.TaxNeedToPay - employeeDeclaration.TaxPaid) / (12-i))));
                    while (i < taxdetails.Count)
                    {
                        //if (previousMonthTax > currentMonthTax && i == currentMonthIndex)
                        //{
                        //    var extraPaid = Math.Round((previousMonthTax * i), 2) - (currentMonthTax*2);
                        //    currentMonthTax = Math.Round((employeeDeclaration.TaxNeedToPay - extraPaid)/(12-i), 2);
                        //    taxdetails[i].TaxDeducted = currentMonthTax;
                        //}
                        //else
                            taxdetails[i].TaxDeducted = currentMonthTax;

                        i++;
                    }

                }
                else
                {
                    IsBuildTaxDetail = true;
                }
            }
            else
            {
                IsBuildTaxDetail = true;
            }

            if (IsBuildTaxDetail)
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
            return salaryBreakup;
        }

        private void BuildSectionWiseComponents(EmployeeDeclaration employeeDeclaration)
        {
            foreach(var x in _sections)
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

            var houseProperty = employeeDeclaration.SalaryComponentItems.FindAll(x => x.ComponentFullName.ToLower() == "House Property".ToLower());
            employeeDeclaration.Declarations.Add(new DeclarationReport
            {
                DeclarationName = "House Property",
                NumberOfProofSubmitted = 0,
                Declarations = new List<string>(),
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

        private dynamic OldTaxRegimeCalculation(decimal TaxableIncome)
        {
            if (TaxableIncome <= 0)
                throw new HiringBellException("Invalid TaxableIncome");

            decimal tax = 0;
            decimal value = 0;
            decimal remainingAmount = TaxableIncome;
            var taxSlab = new Dictionary<string, decimal>();
            taxSlab.Add("0% Tax on income up to 250000", 0);

            while (remainingAmount > 250000)
            {
                if (remainingAmount > 250000 && remainingAmount <= 500000)
                {
                    value = (250000 - remainingAmount);
                    if (value < 0) value = value * -1;
                    remainingAmount = remainingAmount - value;
                    tax += (value * 5) / 100;
                    taxSlab.Add("5% Tax on income between 250001 and 500000", tax);
                }
                else if (remainingAmount > 500000 && remainingAmount <= 1000000)
                {
                    value = (500000 - remainingAmount);
                    if (value < 0) value = value * -1;
                    remainingAmount = remainingAmount - value;
                    tax += (value * 20) / 100;
                    taxSlab.Add("20% Tax on income between 500001 and 1000000", tax);
                }
                else if (remainingAmount > 1000000)
                {
                    value = (1000000 - remainingAmount);
                    if (value < 0) value = value * -1;
                    remainingAmount = remainingAmount - value;
                    tax += (value * 30) / 100;
                    taxSlab.Add("30% Tax on income above 1000000", tax);
                }
            }

            decimal cess = (tax * 4) / 100;
            taxSlab.Add("Gross Income Tax", tax + cess);
            return new {TotalTax = tax+cess, IncomeTaxSlab = taxSlab };
        }
    }
}