using BottomhalfCore.DatabaseLayer.Common.Code;
using ModalLayer.Modal;
using ModalLayer.Modal.Accounts;
using ServiceLayer.Interface;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json;
using System.Data;

namespace ServiceLayer.Code
{
    public class SalaryComponentService : ISalaryComponentService
    {
        private readonly IDb _db;
        private readonly CurrentSession _currentSession;
        private readonly IEvaluationPostfixExpression _postfixToInfixConversion;
        public SalaryComponentService(IDb db, CurrentSession currentSession, IEvaluationPostfixExpression postfixToInfixConversion)
        {
            _db = db;
            _currentSession = currentSession;
            _postfixToInfixConversion = postfixToInfixConversion;
        }

        public SalaryComponents GetSalaryComponentByIdService()
        {
            throw new NotImplementedException();
        }

        public List<SalaryComponents> GetSalaryComponentsDetailService()
        {
            List<SalaryComponents> salaryComponents = _db.GetList<SalaryComponents>("sp_salary_components_get", false);
            return salaryComponents;
        }

        public List<SalaryGroup> GetSalaryGroupService()
        {
            List<SalaryGroup> salaryComponents = _db.GetList<SalaryGroup>("sp_salary_group_getAll", false);
            return salaryComponents;
        }

        public SalaryGroup GetSalaryGroupsByIdService(int SalaryGroupId)
        {
            if (SalaryGroupId <= 0)
                throw new HiringBellException("Invalid SalaryGroupId");
            SalaryGroup salaryGroup = _db.Get<SalaryGroup>("sp_salary_group_getById", new { SalaryGroupId });
            return salaryGroup;
        }

        public List<SalaryComponents> UpdateSalaryComponentService(List<SalaryComponents> salaryComponents)
        {
            if (salaryComponents.Count > 0)
            {
                List<SalaryComponents> result = _db.GetList<SalaryComponents>("sp_salary_components_get", false);
                Parallel.ForEach(result, x =>
                {
                    var item = salaryComponents.Find(i => i.ComponentId == x.ComponentId);
                    if (item != null)
                    {
                        x.IsActive = item.IsActive;
                        x.Formula = item.Formula;
                        x.CalculateInPercentage = item.CalculateInPercentage;
                    }
                });


                var itemOfRows = (from n in result
                                  select new
                                  {
                                      n.ComponentId,
                                      n.ComponentFullName,
                                      n.ComponentDescription,
                                      n.CalculateInPercentage,
                                      n.TaxExempt,
                                      n.ComponentTypeId,
                                      n.ComponentCatagoryId,
                                      n.PercentageValue,
                                      n.MaxLimit,
                                      n.DeclaredValue,
                                      n.RejectedAmount,
                                      n.AcceptedAmount,
                                      n.UploadedFileIds,
                                      n.Formula,
                                      n.EmployeeContribution,
                                      n.EmployerContribution,
                                      n.IncludeInPayslip,
                                      n.IsOpted,
                                      n.IsActive,
                                      Admin = n.CreatedBy,
                                  }).ToList();

                var table = BottomhalfCore.Services.Code.Converter.ToDataTable(itemOfRows);
                _db.BatchInsert("sp_salary_components_insupd", table, true);
            }

            return salaryComponents;
        }

        public List<SalaryComponents> InsertUpdateSalaryComponentsByExcelService(List<SalaryComponents> salaryComponents)
        {
            List<SalaryComponents> finalResult = new List<SalaryComponents>();
            if (salaryComponents.Count > 0)
            {
                List<SalaryComponents> result = _db.GetList<SalaryComponents>("sp_salary_components_get", false);

                foreach (SalaryComponents item in salaryComponents)
                {
                    if (string.IsNullOrEmpty(item.ComponentId) || string.IsNullOrEmpty(item.ComponentFullName))
                        throw new HiringBellException("ComponentId or ComponentFullName is empty.");
                }

                var itemOfRows = (from n in salaryComponents
                                  select new
                                  {
                                      n.ComponentId,
                                      n.ComponentFullName,
                                      n.ComponentDescription,
                                      n.CalculateInPercentage,
                                      n.TaxExempt,
                                      n.ComponentTypeId,
                                      n.ComponentCatagoryId,
                                      n.PercentageValue,
                                      n.MaxLimit,
                                      n.DeclaredValue,
                                      n.AcceptedAmount,
                                      n.RejectedAmount,
                                      n.UploadedFileIds,
                                      n.Formula,
                                      n.EmployeeContribution,
                                      n.EmployerContribution,
                                      n.IncludeInPayslip,
                                      n.IsAdHoc,
                                      n.AdHocId,
                                      n.Section,
                                      n.SectionMaxLimit,
                                      n.IsAffectInGross,
                                      n.RequireDocs,
                                      n.IsOpted,
                                      n.IsActive,
                                      AdminId = _currentSession.CurrentUserDetail.UserId,
                                  }).ToList();

                var table = BottomhalfCore.Services.Code.Converter.ToDataTable(itemOfRows);
                int count = _db.BatchInsert("sp_salary_components_insupd", table, true);
                if (count > 0)
                {
                    foreach (var item in result)
                    {
                        var modified = salaryComponents.Find(x => x.ComponentId == item.ComponentId);
                        if (modified != null)
                        {
                            item.ComponentFullName = modified.ComponentFullName;
                            item.AdHocId = modified.AdHocId;
                            item.AdminId = modified.AdminId;
                            item.ComponentId = modified.ComponentId;
                            item.ComponentDescription = modified.ComponentDescription;
                            item.CalculateInPercentage = modified.CalculateInPercentage;
                            item.TaxExempt = modified.TaxExempt;
                            item.ComponentTypeId = modified.ComponentTypeId;
                            item.ComponentCatagoryId = modified.ComponentCatagoryId;
                            item.PercentageValue = modified.PercentageValue;
                            item.MaxLimit = modified.MaxLimit;
                            item.DeclaredValue = modified.DeclaredValue;
                            item.Formula = modified.Formula;
                            item.EmployeeContribution = modified.EmployeeContribution;
                            item.EmployerContribution = modified.EmployerContribution;
                            item.IncludeInPayslip = modified.IncludeInPayslip;
                            item.IsAdHoc = modified.IsAdHoc;
                            item.Section = modified.Section;
                            item.SectionMaxLimit = modified.SectionMaxLimit;
                            item.IsAffectInGross = modified.IsAffectInGross;
                            item.RequireDocs = modified.RequireDocs;
                            item.IsOpted = modified.IsOpted;
                            item.IsActive = modified.IsActive;

                            finalResult.Add(item);
                        }
                        else
                        {
                            finalResult.Add(item);
                        }
                    }
                }
                else
                {
                    finalResult = result;
                }
            }

            return finalResult;
        }

        public List<SalaryGroup> AddSalaryGroup(SalaryGroup salaryGroup)
        {
            List<SalaryGroup> salaryGroups = _db.GetList<SalaryGroup>("sp_salary_group_getAll", false);
            SalaryGroup salaryGrp = _db.Get<SalaryGroup>("sp_salary_group_getById", new { salaryGroup.SalaryGroupId });
            foreach (SalaryGroup existSalaryGroup in salaryGroups)
            {
                if ((salaryGroup.MinAmount > existSalaryGroup.MinAmount && salaryGroup.MinAmount < existSalaryGroup.MaxAmount) || (salaryGroup.MaxAmount > existSalaryGroup.MinAmount && salaryGroup.MaxAmount < existSalaryGroup.MaxAmount))
                    throw new HiringBellException("Salary group limit already exist");
            }

            List<SalaryComponents> initialSalaryComponents = _db.GetList<SalaryComponents>("sp_salary_group_get_initial_components");

            if (salaryGrp == null)
            {
                salaryGrp = salaryGroup;
                salaryGrp.SalaryComponents = JsonConvert.SerializeObject(initialSalaryComponents);
                salaryGrp.AdminId = _currentSession.CurrentUserDetail.AdminId;
            }
            else
                throw new HiringBellException("Salary Group already exist.");

            var result = _db.Execute<SalaryGroup>("sp_salary_group_insupd", salaryGrp, true);
            if (string.IsNullOrEmpty(result))
                throw new HiringBellException("Fail to insert or update.");
            List<SalaryGroup> value = this.GetSalaryGroupService();
            return value;
        }

        public List<SalaryComponents> AddUpdateRecurringComponents(SalaryStructure recurringComponent)
        {
            if (string.IsNullOrEmpty(recurringComponent.ComponentName))
                throw new HiringBellException("Invalid component name.");

            if (recurringComponent.ComponentTypeId <= 0)
                throw new HiringBellException("Invalid component type.");


            if (recurringComponent.ComponentCatagoryId <= 0)
                throw new HiringBellException("Invalid component type.");

            List<SalaryComponents> components = _db.GetList<SalaryComponents>("sp_salary_components_get");
            var value = components.Find(x => x.ComponentId == recurringComponent.ComponentName);
            if (value == null)
                value = new SalaryComponents();

            value.ComponentId = recurringComponent.ComponentName;
            value.ComponentFullName = recurringComponent.ComponentFullName;
            value.ComponentDescription = recurringComponent.ComponentDescription;
            value.MaxLimit = recurringComponent.MaxLimit;
            value.DeclaredValue = recurringComponent.DeclaredValue;
            value.AcceptedAmount = recurringComponent.AcceptedAmount;
            value.RejectedAmount = recurringComponent.RejectedAmount;
            value.UploadedFileIds = recurringComponent.UploadedFileIds;
            value.TaxExempt = recurringComponent.TaxExempt;
            value.Section = recurringComponent.Section;
            value.ComponentTypeId = recurringComponent.ComponentTypeId;
            value.SectionMaxLimit = recurringComponent.SectionMaxLimit;
            value.ComponentCatagoryId = recurringComponent.ComponentCatagoryId;
            value.AdminId = _currentSession.CurrentUserDetail.AdminId;

            var result = _db.Execute<SalaryComponents>("sp_salary_components_insupd", value, true);
            if (string.IsNullOrEmpty(result))
                throw new HiringBellException("Fail insert salary component.");

            return this.GetSalaryComponentsDetailService();
        }

        public List<SalaryComponents> AddAdhocComponents(SalaryStructure adhocComponent)
        {
            if (string.IsNullOrEmpty(adhocComponent.ComponentName))
                throw new HiringBellException("Invalid AdHoc component name.");
            if (adhocComponent.AdHocId <= 0)
                throw new HiringBellException("Invalid AdHoc type component.");
            List<SalaryComponents> adhocComp = _db.GetList<SalaryComponents>("sp_salary_components_get");
            var value = adhocComp.Find(x => x.ComponentId == adhocComponent.ComponentName);
            if (value == null)
            {
                value = new SalaryComponents();
                value.ComponentId = adhocComponent.ComponentName;
                value.ComponentFullName = adhocComponent.ComponentFullName;
                value.ComponentDescription = adhocComponent.ComponentDescription;
                value.MaxLimit = adhocComponent.MaxLimit;
                value.DeclaredValue = adhocComponent.DeclaredValue;
                value.AcceptedAmount = adhocComponent.AcceptedAmount;
                value.RejectedAmount = adhocComponent.RejectedAmount;
                value.UploadedFileIds = adhocComponent.UploadedFileIds;
                value.TaxExempt = adhocComponent.TaxExempt;
                value.Section = adhocComponent.Section;
                value.AdHocId = Convert.ToInt32(adhocComponent.AdHocId);
                value.SectionMaxLimit = adhocComponent.SectionMaxLimit;
                value.IsAdHoc = adhocComponent.IsAdHoc;
                value.AdminId = _currentSession.CurrentUserDetail.AdminId;
            }
            else
                throw new HiringBellException("Component already exist.");

            var result = _db.Execute<SalaryComponents>("sp_salary_components_insupd", value, true);
            if (string.IsNullOrEmpty(result))
                throw new HiringBellException("Fail insert salary component.");

            return this.GetSalaryComponentsDetailService();
        }

        public List<SalaryComponents> AddDeductionComponents(SalaryStructure deductionComponent)
        {
            if (string.IsNullOrEmpty(deductionComponent.ComponentName))
                throw new HiringBellException("Invalid AdHoc component name.");
            if (deductionComponent.AdHocId <= 0)
                throw new HiringBellException("Invalid AdHoc type component.");
            List<SalaryComponents> adhocComp = _db.GetList<SalaryComponents>("sp_salary_components_get");
            var value = adhocComp.Find(x => x.ComponentId == deductionComponent.ComponentName);
            if (value == null)
            {
                value = new SalaryComponents();
                value.ComponentId = deductionComponent.ComponentName;
                value.ComponentFullName = deductionComponent.ComponentFullName;
                value.ComponentDescription = deductionComponent.ComponentDescription;
                value.IsAffectInGross = deductionComponent.IsAffectInGross;
                value.AdHocId = Convert.ToInt32(deductionComponent.AdHocId);
                value.MaxLimit = deductionComponent.MaxLimit;
                value.DeclaredValue = deductionComponent.DeclaredValue;
                value.AcceptedAmount = deductionComponent.AcceptedAmount;
                value.RejectedAmount = deductionComponent.RejectedAmount;
                value.UploadedFileIds = deductionComponent.UploadedFileIds;
                value.IsAdHoc = deductionComponent.IsAdHoc;
                value.AdminId = _currentSession.CurrentUserDetail.AdminId;
            }
            else
                throw new HiringBellException("Deduction Component already exist.");

            var result = _db.Execute<SalaryComponents>("sp_salary_components_insupd", value, true);
            if (string.IsNullOrEmpty(result))
                throw new HiringBellException("Fail insert salary component.");

            return this.GetSalaryComponentsDetailService();
        }

        public List<SalaryComponents> AddBonusComponents(SalaryStructure bonusComponent)
        {
            if (string.IsNullOrEmpty(bonusComponent.ComponentName))
                throw new HiringBellException("Invalid component name.");
            if (bonusComponent.AdHocId <= 0)
                throw new HiringBellException("Invalid AdHoc type component.");

            List<SalaryComponents> bonuses = _db.GetList<SalaryComponents>("sp_salary_components_get");
            var value = bonuses.Find(x => x.ComponentId == bonusComponent.ComponentName);
            if (value == null)
            {
                value = new SalaryComponents();
                value.ComponentId = bonusComponent.ComponentName;
                value.ComponentFullName = bonusComponent.ComponentFullName;
                value.ComponentDescription = bonusComponent.ComponentDescription;
                value.AdHocId = Convert.ToInt32(bonusComponent.AdHocId);
                value.MaxLimit = bonusComponent.MaxLimit;
                value.DeclaredValue = bonusComponent.DeclaredValue;
                value.AcceptedAmount = bonusComponent.AcceptedAmount;
                value.RejectedAmount = bonusComponent.RejectedAmount;
                value.UploadedFileIds = bonusComponent.UploadedFileIds;
                value.IsAdHoc = bonusComponent.IsAdHoc;
                value.AdminId = _currentSession.CurrentUserDetail.AdminId;
            }
            else
                throw new HiringBellException("Bonus Component already exist.");

            var result = _db.Execute<SalaryComponents>("sp_salary_components_insupd", value, true);
            if (string.IsNullOrEmpty(result))
                throw new HiringBellException("Fail insert salary component.");

            return this.GetSalaryComponentsDetailService();
        }

        public List<SalaryGroup> UpdateSalaryGroup(SalaryGroup salaryGroup)
        {
            List<SalaryGroup> salaryGroups = _db.GetList<SalaryGroup>("sp_salary_group_getAll", false);
            SalaryGroup salaryGrp = _db.Get<SalaryGroup>("sp_salary_group_getById", new { salaryGroup.SalaryGroupId });
            foreach (SalaryGroup existSalaryGroup in salaryGroups)
            {
                if ((salaryGroup.MinAmount < existSalaryGroup.MinAmount && salaryGroup.MinAmount > existSalaryGroup.MaxAmount) || (salaryGroup.MaxAmount > existSalaryGroup.MinAmount && salaryGroup.MaxAmount < existSalaryGroup.MaxAmount))
                    throw new HiringBellException("Salary group limit already exist");
            }
            if (salaryGrp == null)
                throw new HiringBellException("Salary Group already exist.");
            else
            {
                salaryGrp = salaryGroup;
                if (string.IsNullOrEmpty(salaryGrp.SalaryComponents))
                    salaryGrp.SalaryComponents = "[]";
                else
                    salaryGrp.SalaryComponents = JsonConvert.SerializeObject(salaryGroup.GroupComponents);
                salaryGrp.AdminId = _currentSession.CurrentUserDetail.AdminId;
            }

            var result = _db.Execute<SalaryGroup>("sp_salary_group_insupd", salaryGrp, true);
            if (string.IsNullOrEmpty(result))
                throw new HiringBellException("Fail to insert or update.");
            List<SalaryGroup> value = this.GetSalaryGroupService();
            return value;
        }

        public List<SalaryComponents> UpdateSalaryGroupComponentService(SalaryGroup salaryGroup)
        {
            SalaryGroup salaryGrp = _db.Get<SalaryGroup>("sp_salary_group_getById", new { salaryGroup.SalaryGroupId });
            if (salaryGrp == null)
                throw new HiringBellException("Salary Group already exist.");
            else
            {
                salaryGrp = salaryGroup;
                if (salaryGrp.GroupComponents == null)
                    salaryGrp.SalaryComponents = "[]";
                else
                    salaryGrp.SalaryComponents = JsonConvert.SerializeObject(salaryGroup.GroupComponents);
                salaryGrp.AdminId = _currentSession.CurrentUserDetail.AdminId;
            }

            var result = _db.Execute<SalaryGroup>("sp_salary_group_insupd", salaryGrp, true);
            if (string.IsNullOrEmpty(result))
                throw new HiringBellException("Fail to insert or update.");
            List<SalaryComponents> value = this.GetSalaryGroupComponents(salaryGroup.SalaryGroupId);
            return value;
        }

        public List<SalaryComponents> GetSalaryGroupComponents(int salaryGroupId)
        {
            SalaryGroup salaryGroup = _db.Get<SalaryGroup>("sp_salary_group_getById", new { SalaryGroupId = salaryGroupId });
            if (salaryGroup == null)
                throw new HiringBellException("Unable to get salary group. Please contact admin");

            salaryGroup.GroupComponents = JsonConvert.DeserializeObject<List<SalaryComponents>>(salaryGroup.SalaryComponents);
            return salaryGroup.GroupComponents;
        }

        public List<SalaryComponents> GetSalaryGroupComponentsByCTC(decimal CTC)
        {
            SalaryGroup salaryGroup = _db.Get<SalaryGroup>("sp_salary_group_get_by_ctc", new { CTC });
            if (salaryGroup == null)
                throw new HiringBellException("Unable to get salary group. Please contact admin");

            salaryGroup.GroupComponents = JsonConvert.DeserializeObject<List<SalaryComponents>>(salaryGroup.SalaryComponents);
            return salaryGroup.GroupComponents;
        }

        public string SalaryDetailService(long EmployeeId, EmployeeSalaryDetail salaryDetail, CompleteSalaryBreakup ComplcompSalaryDetail)
        {
            EmployeeSalaryDetail salaryBreakup = new EmployeeSalaryDetail();
            if (EmployeeId <= 0)
                throw new HiringBellException("Invalid EmployeeId");
            List<EmployeeSalaryDetail> allSalaryBreakups = _db.GetList<EmployeeSalaryDetail>("sp_employee_salary_detail_get_by_empid", new { EmployeeId = EmployeeId });
            if (allSalaryBreakups == null)
            {
                salaryBreakup = salaryDetail;
                salaryBreakup.CompleteSalaryDetail = JsonConvert.SerializeObject(ComplcompSalaryDetail);
            }
            else
            {
                salaryBreakup = allSalaryBreakups.FirstOrDefault(x => x.EmployeeId == EmployeeId);
                salaryBreakup.CTC = salaryDetail.CTC;
                salaryBreakup.GrossIncome = salaryDetail.GrossIncome;
                salaryBreakup.GroupId = salaryDetail.GroupId;
                salaryBreakup.NetSalary = salaryDetail.NetSalary;
                salaryBreakup.CompleteSalaryDetail = JsonConvert.SerializeObject(ComplcompSalaryDetail);
            }
            var result = _db.Execute<EmployeeSalaryDetail>("sp_employee_salary_detail_InsUpd", salaryBreakup, true);
            if (string.IsNullOrEmpty(result))
                throw new HiringBellException("Unable to insert or update salary breakup");
            else
                result = "Inserted/Updated successfully";
            return result;
        }

        //public void CalculateBreakup(long employeeId, CompleteSalaryBreakup complcompSalaryDetail)
        //{
        //    EmployeeDeclaration employeeDeclaration = default;
        //    DbParam[] param = new DbParam[]
        //    {
        //        new DbParam(employeeId, typeof(long), "_EmployeeId"),
        //        new DbParam(UserType.Compnay, typeof(int), "_UserTypeId")
        //    };

        //    DataSet resultSet = _db.GetDataset("sp_employee_declaration_get_byEmployeeId", param);
        //    if ((resultSet == null || resultSet.Tables.Count == 0) && resultSet.Tables.Count != 3)
        //        throw new HiringBellException("Unable to get the detail");

        //    employeeDeclaration = BottomhalfCore.Services.Code.Converter.ToType<EmployeeDeclaration>(resultSet.Tables[0]);

        //    if (resultSet.Tables[2].Rows.Count == 1)
        //        employeeDeclaration.SalaryDetail = BottomhalfCore.Services.Code.Converter.ToType<SalaryBreakup>(resultSet.Tables[2]);
        //}

        public CompleteSalaryBreakup SalaryBreakupCalcService(long EmployeeId, decimal CTCAnnually)
            {
            CompleteSalaryBreakup completeSalaryBreakup = new CompleteSalaryBreakup();
            if (EmployeeId < 0)
                throw new HiringBellException("Invalid EmployeeId");

            if (CTCAnnually <= 0)
                throw new HiringBellException("Invalid CTCAnnually");

            List<SalaryComponents> salaryComponents = this.GetSalaryGroupComponentsByCTC(CTCAnnually);
            List<SalaryComponents> fixedComponents = salaryComponents.FindAll(x => x.IsOpted == true); //change percentagevalue to IsOpted == true
            completeSalaryBreakup.CTCAnnually = CTCAnnually;
            foreach (SalaryComponents component in fixedComponents)
            {
                switch (component.ComponentId.ToUpper())
                {
                    case "GRA":
                        completeSalaryBreakup.GratuityAnnually = component.DeclaredValue;
                        break;
                    case "CA":
                        completeSalaryBreakup.ConveyanceAnnually = Convert.ToDecimal(this.calculateExpressionUsingInfixDS(component.Formula));
                        break;
                    case "EPER-PF":
                        completeSalaryBreakup.PFAnnually = component.DeclaredValue;
                        break;
                    case "MA":
                        completeSalaryBreakup.MedicalAnnually = component.DeclaredValue;
                        break;
                    case "SHA":
                        completeSalaryBreakup.ShiftAnnually = component.DeclaredValue;
                        break;
                    case "ECI":
                        completeSalaryBreakup.InsuranceAnnually = component.DeclaredValue;
                        break;
                }
            }
            var gross = completeSalaryBreakup.CTCAnnually - (completeSalaryBreakup.InsuranceAnnually + completeSalaryBreakup.PFAnnually + completeSalaryBreakup.GratuityAnnually);
            if (gross > 0)
                completeSalaryBreakup.GrossAnnually = gross;
            else
                throw new HiringBellException("invalid gross salary");
            foreach (var item in salaryComponents)
            {
                var formula = item.Formula;
                var componentId = item.ComponentId;
                decimal finalvalue = 0;
                if (!string.IsNullOrEmpty(formula))
                {
                    if (formula.Contains("[BASIC]"))
                    {
                        formula = formula.Replace("[BASIC]", (Convert.ToInt32(completeSalaryBreakup.BasicAnnually)).ToString());
                    }
                    else if (formula.Contains("[CTC]"))
                    {
                        formula = formula.Replace("[CTC]", (Convert.ToInt32(completeSalaryBreakup.CTCAnnually)).ToString());
                    }
                    else if (formula.Contains("[GROSS]"))
                    {
                        formula = formula.Replace("[GROSS]", (Convert.ToInt32(completeSalaryBreakup.GrossAnnually)).ToString());
                    }
                    if (formula.Contains('+') || formula.Contains('-') || formula.Contains('*') || formula.Contains('/') || formula.Contains('%'))
                    {
                        finalvalue = this.calculateExpressionUsingInfixDS(formula);
                    }

                    switch (componentId.ToUpper())
                    {
                        case "BS":
                            completeSalaryBreakup.BasicAnnually = finalvalue;
                            break;
                        case "HRA":
                            completeSalaryBreakup.HRAAnnually = finalvalue;
                            break;
                    }
                }
            }
            completeSalaryBreakup.SpecialAnnually = completeSalaryBreakup.GrossAnnually - (completeSalaryBreakup.BasicAnnually + completeSalaryBreakup.ConveyanceAnnually + completeSalaryBreakup.HRAAnnually + completeSalaryBreakup.MedicalAnnually + completeSalaryBreakup.ShiftAnnually);
            return completeSalaryBreakup;
        }

        public EmployeeSalaryDetail GetSalaryBreakupByEmpIdService(long EmployeeId)
        {
            EmployeeSalaryDetail completeSalaryBreakup = _db.Get<EmployeeSalaryDetail>("sp_employee_salary_detail_get_by_empid", new { EmployeeId });
            return completeSalaryBreakup;
        }

        public SalaryGroup GetSalaryGroupByCTC(decimal CTC)
        {
            SalaryGroup salaryGroup = _db.Get<SalaryGroup>("sp_salary_group_get_by_ctc", new { CTC });
            if (salaryGroup == null)
                throw new HiringBellException("Unable to get salary group. Please contact admin");
            return salaryGroup;
        }
        private decimal calculateExpressionUsingInfixDS(string expression)
        {
            if (!expression.Contains("()"))
            {
                expression = string.Format("({0})", expression);
            }
            List<string> operatorStact = new List<string>();
            var expressionStact = new List<object>();
            int index = 0;
            var lastOp = "";
            var ch = "";

            while (index < expression.Length)
            {
                ch = expression[index].ToString();
                if (ch.Trim() == "")
                {
                    index++;
                    continue;
                }
                int number;
                if (!int.TryParse(ch.ToString(), out number))
                {
                    switch (ch)
                    {
                        case "+":
                        case "-":
                        case "/":
                        case "%":
                        case "*":
                            if (operatorStact.Count > 0)
                            {
                                lastOp = operatorStact[operatorStact.Count - 1];
                                if (lastOp == "+" || lastOp == "-" || lastOp == "/" || lastOp == "*" || lastOp == "%")
                                {
                                    lastOp = operatorStact[operatorStact.Count - 1];
                                    operatorStact.RemoveAt(operatorStact.Count - 1);
                                    expressionStact.Add(lastOp);
                                }
                            }
                            operatorStact.Add(ch);
                            break;
                        case ")":
                            while (true)
                            {
                                lastOp = operatorStact[operatorStact.Count - 1];
                                operatorStact.RemoveAt(operatorStact.Count - 1);
                                if (lastOp == "(")
                                {
                                    break;
                                }
                                expressionStact.Add(lastOp);
                            }
                            break;
                        case "(":
                            operatorStact.Add(ch);
                            break;
                    }
                }
                else
                {
                    decimal value = 0;
                    decimal fraction = 0;
                    bool isFractionFound = false;
                    while (true)
                    {
                        ch = expression[index].ToString();
                        if (ch == ".")
                        {
                            index++;
                            isFractionFound = true;
                            break;
                        }

                        if (ch.Trim() == "")
                        {
                            expressionStact.Add($"{value}.{fraction}");
                            break;
                        }

                        if (int.TryParse(ch.ToString(), out number))
                        {
                            if (!isFractionFound)
                                value = Convert.ToDecimal(value + ch);
                            else
                                fraction = Convert.ToDecimal(fraction + ch);
                            index++;
                        }
                        else
                        {
                            index--;
                            expressionStact.Add($"{value}.{fraction}");
                            break;
                        }
                    }
                }

                index++;
            }

            var exp = expressionStact.Aggregate((x, y) => x.ToString() + " " + y.ToString()).ToString();
            return _postfixToInfixConversion.evaluatePostfix(exp);
        }

    }
}
