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
using BottomhalfCore.Services.Code;
using BottomhalfCore.Services.Interface;

namespace ServiceLayer.Code
{
    public class SalaryComponentService : ISalaryComponentService
    {
        private readonly IDb _db;
        private readonly CurrentSession _currentSession;
        private readonly IEvaluationPostfixExpression _postfixToInfixConversion;
        private readonly ITimezoneConverter _timezoneConverter;

        public SalaryComponentService(IDb db, CurrentSession currentSession,
            IEvaluationPostfixExpression postfixToInfixConversion,
            ITimezoneConverter timezoneConverter
        )
        {
            _db = db;
            _currentSession = currentSession;
            _postfixToInfixConversion = postfixToInfixConversion;
            _timezoneConverter = timezoneConverter;
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

        public List<SalaryGroup> GetSalaryGroupService(int CompanyId)
        {
            List<SalaryGroup> salaryComponents = _db.GetList<SalaryGroup>("sp_salary_group_getbyCompanyId", new { CompanyId }, false);
            return salaryComponents;
        }

        public dynamic GetCustomSalryPageDataService(int CompanyId)
        {
            List<SalaryGroup> salaryGroups = this.GetSalaryGroupService(CompanyId);
            List<SalaryComponents> salaryComponents = this.GetSalaryComponentsDetailService();
            return new { SalaryComponents = salaryComponents, SalaryGroups = salaryGroups };
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

                var table = Converter.ToDataTable(itemOfRows);
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
                    if (result.Count > 0)
                    {
                        finalResult = result;
                        foreach (var newComponents in salaryComponents)
                        {
                            var existing = finalResult.Find(x => x.ComponentId == newComponents.ComponentId);
                            if (existing != null)
                            {
                                existing.ComponentFullName = newComponents.ComponentFullName;
                                existing.AdHocId = newComponents.AdHocId;
                                existing.AdminId = newComponents.AdminId;
                                existing.ComponentId = newComponents.ComponentId;
                                existing.ComponentDescription = newComponents.ComponentDescription;
                                existing.CalculateInPercentage = newComponents.CalculateInPercentage;
                                existing.TaxExempt = newComponents.TaxExempt;
                                existing.ComponentTypeId = newComponents.ComponentTypeId;
                                existing.ComponentCatagoryId = newComponents.ComponentCatagoryId;
                                existing.PercentageValue = newComponents.PercentageValue;
                                existing.MaxLimit = newComponents.MaxLimit;
                                existing.DeclaredValue = newComponents.DeclaredValue;
                                existing.Formula = newComponents.Formula;
                                existing.EmployeeContribution = newComponents.EmployeeContribution;
                                existing.EmployerContribution = newComponents.EmployerContribution;
                                existing.IncludeInPayslip = newComponents.IncludeInPayslip;
                                existing.IsAdHoc = newComponents.IsAdHoc;
                                existing.Section = newComponents.Section;
                                existing.SectionMaxLimit = newComponents.SectionMaxLimit;
                                existing.IsAffectInGross = newComponents.IsAffectInGross;
                                existing.RequireDocs = newComponents.RequireDocs;
                                existing.IsOpted = newComponents.IsOpted;
                                existing.IsActive = newComponents.IsActive;
                            }
                            else
                                finalResult.Add(newComponents);
                        }

                    }
                    else
                    {
                        finalResult = salaryComponents;
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
            if (salaryGroup.CompanyId <= 0)
                throw new HiringBellException("Invalid data selected to create group. Please contact to admin.");

            SalaryGroup salaryGrp = _db.Get<SalaryGroup>("sp_salary_group_get_if_exists", new
            {
                salaryGroup.CompanyId,
                salaryGroup.MinAmount,
                salaryGroup.MaxAmount
            });

            if (salaryGrp != null)
                throw new HiringBellException("Salary group limit already exist");

            List<SalaryComponents> initialSalaryComponents = _db.GetList<SalaryComponents>("sp_salary_group_get_initial_components");

            if (salaryGrp == null)
            {
                salaryGrp = salaryGroup;
                salaryGrp.SalaryComponents = JsonConvert.SerializeObject(initialSalaryComponents);
                salaryGrp.AdminId = _currentSession.CurrentUserDetail.AdminId;
            }
            else
                throw new HiringBellException("Salary Group already exist.");

            var result = _db.Execute<SalaryGroup>("sp_salary_group_insupd", new
            {
                salaryGrp.SalaryGroupId,
                salaryGrp.CompanyId,
                salaryGrp.SalaryComponents,
                salaryGrp.GroupName,
                salaryGrp.GroupDescription,
                salaryGroup.MinAmount,
                salaryGrp.MaxAmount,
                salaryGroup.AdminId
            }, true);
            if (string.IsNullOrEmpty(result))
                throw new HiringBellException("Fail to insert or update.");
            List<SalaryGroup> value = this.GetSalaryGroupService(salaryGroup.CompanyId);
            return value;
        }

        public async Task<List<SalaryComponents>> AddUpdateRecurringComponents(SalaryStructure recurringComponent)
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

            List<SalaryComponents> salaryComponents = this.GetSalaryComponentsDetailService();
            await updateSalaryGroupByUdatingComponent(recurringComponent);

            return salaryComponents;
        }

        private async Task updateSalaryGroupByUdatingComponent(SalaryStructure recurringComponent)
        {
            List<SalaryGroup> salaryGroups = _db.GetList<SalaryGroup>("sp_salary_group_getAll", false);
            if (salaryGroups.Count > 0)
            {
                foreach (var item in salaryGroups)
                {
                    if (string.IsNullOrEmpty(item.SalaryComponents))
                        throw new HiringBellException("Salary component not found");

                    List<SalaryComponents> salaryComponents = JsonConvert.DeserializeObject<List<SalaryComponents>>(item.SalaryComponents);
                    var component = salaryComponents.Find(x => x.ComponentId == recurringComponent.ComponentName);
                    if (component != null)
                    {
                        component.ComponentId = recurringComponent.ComponentName;
                        component.ComponentCatagoryId = recurringComponent.ComponentCatagoryId;
                        component.ComponentTypeId = recurringComponent.ComponentTypeId;
                        component.ComponentFullName = recurringComponent.ComponentFullName;
                        component.MaxLimit = recurringComponent.MaxLimit;
                        component.ComponentDescription = recurringComponent.ComponentDescription;
                        component.TaxExempt = recurringComponent.TaxExempt;
                        component.Section = recurringComponent.Section;
                        component.SectionMaxLimit = recurringComponent.SectionMaxLimit;
                    }
                    item.SalaryComponents = JsonConvert.SerializeObject(salaryComponents);
                }
                var table = Converter.ToDataTable(salaryGroups);
                _db.StartTransaction(IsolationLevel.ReadUncommitted);
                var statue = await _db.BatchInsertUpdateAsync("sp_salary_group_insupd", table, true);
                _db.Commit();

                await Task.CompletedTask;
            }
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
                if (string.IsNullOrEmpty(salaryGrp.SalaryComponents))
                    salaryGrp.SalaryComponents = "[]";
                //else
                //    if (salaryGroup.GroupComponents == null && salaryGroup.SalaryComponents == null)
                //        salaryGroup.SalaryComponents = "[]";
                //    else
                //        salaryGrp.SalaryComponents = JsonConvert.SerializeObject(salaryGroup.GroupComponents);

                salaryGrp.GroupName = salaryGroup.GroupName;
                salaryGrp.GroupDescription = salaryGroup.GroupDescription;
                salaryGrp.MinAmount = salaryGroup.MinAmount;
                salaryGrp.MaxAmount = salaryGroup.MaxAmount;
                salaryGrp.AdminId = _currentSession.CurrentUserDetail.AdminId;
            }

            var result = _db.Execute<SalaryGroup>("sp_salary_group_insupd", salaryGrp, true);
            if (string.IsNullOrEmpty(result))
                throw new HiringBellException("Fail to insert or update.");
            List<SalaryGroup> value = this.GetSalaryGroupService(salaryGroup.CompanyId);
            return value;
        }

        public SalaryGroup RemoveAndUpdateSalaryGroupService(string componentId, int groupId)
        {
            SalaryGroup salaryGrp = _db.Get<SalaryGroup>("sp_salary_group_getById", new { SalaryGroupId = groupId });
            if (salaryGrp == null)
                throw new HiringBellException("Salary Group already exist.");

            if (string.IsNullOrEmpty(salaryGrp.SalaryComponents))
                throw new HiringBellException("Salary Group already exist.", System.Net.HttpStatusCode.NotFound);

            var components = JsonConvert.DeserializeObject<List<SalaryComponents>>(salaryGrp.SalaryComponents);
            var component = components.FirstOrDefault(x => x.ComponentId == componentId);
            if (component != null)
            {
                if (components.Remove(component))
                {
                    salaryGrp.SalaryComponents = JsonConvert.SerializeObject(components);
                    var result = _db.Execute<SalaryGroup>("sp_salary_group_insupd", salaryGrp, true);
                    if (string.IsNullOrEmpty(result))
                        throw new HiringBellException("Fail to insert or update.");
                }
                else
                {
                    throw new HiringBellException("Component does not exist in the group.", System.Net.HttpStatusCode.NotFound);
                }
            }
            else
            {
                components = components.Where(x => x.ComponentId != null && x.ComponentId != "").ToList();
                salaryGrp.SalaryComponents = JsonConvert.SerializeObject(components);
                var result = _db.Execute<SalaryGroup>("sp_salary_group_insupd", salaryGrp, true);
                if (string.IsNullOrEmpty(result))
                    throw new HiringBellException("Fail to insert or update.");
            }

            return salaryGrp;
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
            List<SalaryComponents> value = this.GetSalaryGroupComponents(salaryGroup.SalaryGroupId, Convert.ToDecimal(salaryGroup.CTC));
            return value;
        }

        public List<SalaryComponents> GetSalaryGroupComponents(int salaryGroupId, decimal CTC)
        {
            SalaryGroup salaryGroup = _db.Get<SalaryGroup>("sp_salary_group_get_by_id_or_ctc", new { SalaryGroupId = salaryGroupId, CTC });
            if (salaryGroup == null)
                throw new HiringBellException("Fail to calulate salar detail, salary group not defined for the current package.");

            salaryGroup.GroupComponents = JsonConvert.DeserializeObject<List<SalaryComponents>>(salaryGroup.SalaryComponents);
            return salaryGroup.GroupComponents;
        }

        public List<SalaryComponents> GetSalaryGroupComponentsByCTC(long EmployeeId, decimal CTC)
        {
            SalaryGroup salaryGroup = _db.Get<SalaryGroup>("sp_salary_group_get_by_ctc", new { EmployeeId, CTC });
            if (salaryGroup == null)
            {
                salaryGroup = new SalaryGroup
                {
                    CTC = CTC,
                    GroupComponents = new List<SalaryComponents>()
                };
            }
            else
                salaryGroup.GroupComponents = JsonConvert.DeserializeObject<List<SalaryComponents>>(salaryGroup.SalaryComponents);

            return salaryGroup.GroupComponents;
        }

        private bool CompareFieldsValue(AnnualSalaryBreakup matchedSalaryBreakup, List<CalculatedSalaryBreakupDetail> completeSalaryBreakup)
        {
            bool flag = false;
            int i = 0;
            while (i < matchedSalaryBreakup.SalaryBreakupDetails.Count)
            {
                var item = matchedSalaryBreakup.SalaryBreakupDetails.ElementAt(i);
                var elem = completeSalaryBreakup.Find(x => x.ComponentId == item.ComponentId);
                if (elem == null)
                    break;

                if (item.FinalAmount != elem.FinalAmount)
                {
                    flag = true;
                    break;
                }

                i++;
            }

            return flag;
        }

        private void UpdateIfChangeFound(List<AnnualSalaryBreakup> annualSalaryBreakups, List<CalculatedSalaryBreakupDetail> salaryBreakup, int presentMonth, int PresentYear)
        {
            DateTime present = new DateTime(PresentYear, presentMonth, 1);
            if (_currentSession.TimeZone != null)
                present = _timezoneConverter.ToIstTime(present);

            AnnualSalaryBreakup matchedSalaryBreakups = annualSalaryBreakups.Where(x => x.MonthFirstDate.Subtract(present).TotalDays >= 0).FirstOrDefault<AnnualSalaryBreakup>();
            if (matchedSalaryBreakups == null)
                throw new HiringBellException("Invalid data found in salary detail. Please contact to admin.");
            else
                matchedSalaryBreakups.SalaryBreakupDetails = salaryBreakup;
        }

        private void ValidateCorrectnessOfSalaryDetail(List<CalculatedSalaryBreakupDetail> calculatedSalaryBreakupDetail)
        {
            // implement code to check the correctness of the modal on value level.
        }

        public string SalaryDetailService(long EmployeeId, List<CalculatedSalaryBreakupDetail> calculatedSalaryBreakupDetail, int PresentMonth, int PresentYear)
        {
            if (EmployeeId <= 0)
                throw new HiringBellException("Invalid EmployeeId");

            EmployeeSalaryDetail employeeSalaryDetail = _db.Get<EmployeeSalaryDetail>("sp_employee_salary_detail_get_by_empid", new { EmployeeId = EmployeeId });
            if (employeeSalaryDetail == null)
                throw new HiringBellException("Fail to get salary detail. Please contact to admin.");

            List<AnnualSalaryBreakup> annualSalaryBreakups = JsonConvert.DeserializeObject<List<AnnualSalaryBreakup>>(employeeSalaryDetail.CompleteSalaryDetail);

            // implement code to check the correctness of the modal on value level.
            ValidateCorrectnessOfSalaryDetail(calculatedSalaryBreakupDetail);

            UpdateIfChangeFound(annualSalaryBreakups, calculatedSalaryBreakupDetail, PresentMonth, PresentYear);

            EmployeeSalaryDetail salaryBreakup = new EmployeeSalaryDetail
            {
                CompleteSalaryDetail = JsonConvert.SerializeObject(calculatedSalaryBreakupDetail),
                CTC = employeeSalaryDetail.CTC,
                EmployeeId = EmployeeId,
                GrossIncome = employeeSalaryDetail.GrossIncome,
                GroupId = employeeSalaryDetail.GroupId,
                NetSalary = employeeSalaryDetail.NetSalary,
                TaxDetail = employeeSalaryDetail.TaxDetail
            };
            var result = _db.Execute<EmployeeSalaryDetail>("sp_employee_salary_detail_InsUpd", salaryBreakup, true);
            if (string.IsNullOrEmpty(result))
                throw new HiringBellException("Unable to insert or update salary breakup");
            else
                result = "Inserted/Updated successfully";
            return result;
        }

        private decimal GetEmployeeContributionAmount(List<SalaryComponents> salaryComponents, decimal CTC)
        {
            decimal finalAmount = 0;
            var gratutity = salaryComponents.FirstOrDefault(x => x.ComponentId.ToUpper() == "GRA");
            if (gratutity != null && !string.IsNullOrEmpty(gratutity.Formula))
            {
                if (gratutity.Formula.Contains("[CTC]"))
                    gratutity.Formula = gratutity.Formula.Replace("[CTC]", (Convert.ToDecimal(CTC)).ToString());

                finalAmount += this.calculateExpressionUsingInfixDS(gratutity.Formula, gratutity.DeclaredValue);
            }

            var employeePF = salaryComponents.FirstOrDefault(x => x.ComponentId.ToUpper() == "EPER-PF");
            if (employeePF != null && !string.IsNullOrEmpty(employeePF.Formula))
            {
                if (employeePF.Formula.Contains("[CTC]"))
                    employeePF.Formula = employeePF.Formula.Replace("[CTC]", (Convert.ToDecimal(CTC)).ToString());

                finalAmount += this.calculateExpressionUsingInfixDS(employeePF.Formula, employeePF.DeclaredValue);
            }

            var employeeInsurance = salaryComponents.FirstOrDefault(x => x.ComponentId.ToUpper() == "ECI");
            if (employeeInsurance != null && !string.IsNullOrEmpty(employeeInsurance.Formula))
            {
                if (employeeInsurance.Formula.Contains("[CTC]"))
                    employeeInsurance.Formula = employeeInsurance.Formula.Replace("[CTC]", (Convert.ToDecimal(CTC)).ToString());

                finalAmount += this.calculateExpressionUsingInfixDS(employeeInsurance.Formula, employeeInsurance.DeclaredValue);
            }

            return finalAmount;
        }

        private decimal GetPerquisiteAmount(List<SalaryComponents> salaryComponents)
        {
            decimal finalAmount = 0;
            var prequisiteComponents = salaryComponents.FindAll(x => x.ComponentTypeId == 6);
            if (prequisiteComponents.Count > 0)
            {
                foreach (var item in prequisiteComponents)
                {
                    finalAmount += this.calculateExpressionUsingInfixDS(item.Formula, item.DeclaredValue);
                }
            }
            return finalAmount;
        }

        private decimal GetBaiscAmountValue(List<SalaryComponents> salaryComponents, decimal grossAmount, decimal CTC)
        {
            decimal finalAmount = 0;
            var basicComponent = salaryComponents.Find(x => x.ComponentId.ToUpper() == "BS");
            if (basicComponent != null)
            {
                if (!string.IsNullOrEmpty(basicComponent.Formula))
                {
                    if (basicComponent.Formula.Contains("[CTC]"))
                        basicComponent.Formula = basicComponent.Formula.Replace("[CTC]", (Convert.ToDecimal(CTC)).ToString());
                    else if (basicComponent.Formula.Contains("[GROSS]"))
                        basicComponent.Formula = basicComponent.Formula.Replace("[GROSS]", grossAmount.ToString());
                }

                finalAmount = this.calculateExpressionUsingInfixDS(basicComponent.Formula, basicComponent.DeclaredValue);
            }

            return finalAmount;
        }

        private async Task<EmployeeCalculation> GetEmployeeSalaryDetail(long EmployeeId, decimal CTCAnnually)
        {
            EmployeeCalculation employeeCalculation = new EmployeeCalculation();

            employeeCalculation.EmployeeId = EmployeeId;
            employeeCalculation.CTC = CTCAnnually;

            var ResultSet = _db.FetchDataSet("sp_salary_components_group_by_employeeid",
                new { employeeCalculation.EmployeeId });
            if (ResultSet == null || ResultSet.Tables.Count != 3)
                throw new HiringBellException("Unbale to get salary detail. Please contact to admin.");

            if (ResultSet.Tables[0].Rows.Count == 0)
                throw new HiringBellException($"Salary group not found for salary: [{CTCAnnually}]");

            if (ResultSet.Tables[1].Rows.Count == 0)
                throw new HiringBellException($"Salary detail not found for employee Id: [{EmployeeId}]");

            if (ResultSet.Tables[2].Rows.Count == 0)
                throw new HiringBellException($"Employee company setting is not defined. Please contact to admin.");

            employeeCalculation.salaryGroup = Converter.ToType<SalaryGroup>(ResultSet.Tables[0]);

            employeeCalculation.employeeSalaryDetail = Converter.ToType<EmployeeSalaryDetail>(ResultSet.Tables[1]);

            employeeCalculation.companySetting = Converter.ToType<CompanySetting>(ResultSet.Tables[2]);

            if (string.IsNullOrEmpty(employeeCalculation.salaryGroup.SalaryComponents))
                throw new HiringBellException($"Salary components not found for salary: [{CTCAnnually}]");

            return await Task.FromResult(employeeCalculation);
        }

        public async Task<List<AnnualSalaryBreakup>> SalaryBreakupCalcService(long EmployeeId, decimal CTCAnnually)
        {
            if (EmployeeId < 0)
                throw new HiringBellException("Invalid EmployeeId");

            EmployeeCalculation employeeCalculation = await GetEmployeeSalaryDetail(EmployeeId, CTCAnnually);

            if (CTCAnnually <= 0)
                return this.CreateSalaryBreakUpWithZeroCTC(EmployeeId, CTCAnnually);
            else
                return this.CreateSalaryBreakupWithValue(employeeCalculation);
        }

        public List<AnnualSalaryBreakup> CreateSalaryBreakupWithValue(EmployeeCalculation eCal)
        {
            List<AnnualSalaryBreakup> annualSalaryBreakups = new List<AnnualSalaryBreakup>();
            DateTime startDate = new DateTime(eCal.companySetting.FinancialYear, eCal.companySetting.DeclarationStartMonth, 1);

            if (eCal.salaryGroup == null || string.IsNullOrEmpty(eCal.salaryGroup.SalaryComponents) || eCal.salaryGroup.SalaryComponents == "[]")
                throw new HiringBellException("Salary group or its component not defined. Please contact to admin.");

            eCal.salaryGroup.GroupComponents = JsonConvert
                .DeserializeObject<List<SalaryComponents>>(eCal.salaryGroup.SalaryComponents);

            decimal perquisiteAmount = GetPerquisiteAmount(eCal.salaryGroup.GroupComponents);
            decimal EmployeeContributionAmount = (GetEmployeeContributionAmount(eCal.salaryGroup.GroupComponents, eCal.CTC)) + perquisiteAmount;
            decimal grossAmount = Convert.ToDecimal(eCal.CTC - EmployeeContributionAmount);
            decimal basicAmountValue = GetBaiscAmountValue(eCal.salaryGroup.GroupComponents, grossAmount, eCal.CTC);

            int index = 0;
            while (index < 12)
            {
                List<CalculatedSalaryBreakupDetail> calculatedSalaryBreakupDetails = new List<CalculatedSalaryBreakupDetail>();

                int i = 0;
                while (i < eCal.salaryGroup.GroupComponents.Count)
                {
                    var item = eCal.salaryGroup.GroupComponents.ElementAt(i);
                    if (!string.IsNullOrEmpty(item.Formula))
                    {
                        if (item.Formula.Contains("[BASIC]"))
                            item.Formula = item.Formula.Replace("[BASIC]", basicAmountValue.ToString());
                        else if (item.Formula.Contains("[CTC]"))
                            item.Formula = item.Formula.Replace("[CTC]", (Convert.ToDecimal(eCal.CTC)).ToString());
                        else if (item.Formula.Contains("[GROSS]"))
                            item.Formula = item.Formula.Replace("[GROSS]", grossAmount.ToString());
                    }

                    i++;
                }

                decimal amount = 0;
                CalculatedSalaryBreakupDetail calculatedSalaryBreakupDetail = null;
                foreach (var item in eCal.salaryGroup.GroupComponents)
                {
                    if (!string.IsNullOrEmpty(item.ComponentId))
                    {
                        calculatedSalaryBreakupDetail = new CalculatedSalaryBreakupDetail();

                        amount = this.calculateExpressionUsingInfixDS(item.Formula, item.DeclaredValue);

                        calculatedSalaryBreakupDetail.ComponentId = item.ComponentId;
                        calculatedSalaryBreakupDetail.Formula = item.Formula;
                        calculatedSalaryBreakupDetail.ComponentName = item.ComponentFullName;
                        calculatedSalaryBreakupDetail.ComponentTypeId = item.ComponentTypeId;
                        calculatedSalaryBreakupDetail.FinalAmount = amount / 12;

                        calculatedSalaryBreakupDetails.Add(calculatedSalaryBreakupDetail);
                    }
                }

                var value = calculatedSalaryBreakupDetails.Where(x => x.ComponentTypeId == 2).Sum(x => x.FinalAmount);

                calculatedSalaryBreakupDetail = new CalculatedSalaryBreakupDetail
                {
                    ComponentId = nameof(ComponentNames.Special),
                    Formula = null,
                    ComponentName = ComponentNames.Special,
                    FinalAmount = (grossAmount / 12 - calculatedSalaryBreakupDetails.Where(x => x.ComponentTypeId == 2).Sum(x => x.FinalAmount)),
                    ComponentTypeId = 102
                };

                calculatedSalaryBreakupDetails.Add(calculatedSalaryBreakupDetail);

                calculatedSalaryBreakupDetail = new CalculatedSalaryBreakupDetail
                {
                    ComponentId = nameof(ComponentNames.Gross),
                    Formula = null,
                    ComponentName = ComponentNames.Gross,
                    FinalAmount = (eCal.CTC - EmployeeContributionAmount) / 12,
                    ComponentTypeId = 100
                };

                calculatedSalaryBreakupDetails.Add(calculatedSalaryBreakupDetail);

                calculatedSalaryBreakupDetail = new CalculatedSalaryBreakupDetail
                {
                    ComponentId = nameof(ComponentNames.CTC),
                    Formula = null,
                    ComponentName = ComponentNames.CTC,
                    FinalAmount = eCal.CTC / 12,
                    ComponentTypeId = 101
                };

                calculatedSalaryBreakupDetails.Add(calculatedSalaryBreakupDetail);

                annualSalaryBreakups.Add(new AnnualSalaryBreakup
                {
                    MonthName = startDate.ToString("MMM"),
                    MonthNumber = startDate.Month,
                    MonthFirstDate = startDate,
                    SalaryBreakupDetails = calculatedSalaryBreakupDetails
                });

                startDate = startDate.AddMonths(1);
                index++;
            }

            return annualSalaryBreakups;
        }

        private List<AnnualSalaryBreakup> CreateSalaryBreakUpWithZeroCTC(long EmployeeId, decimal CTCAnnually)
        {
            List<AnnualSalaryBreakup> annualSalaryBreakups = new List<AnnualSalaryBreakup>();


            List<CalculatedSalaryBreakupDetail> calculatedSalaryBreakupDetails = new List<CalculatedSalaryBreakupDetail>();
            List<SalaryComponents> salaryComponents = this.GetSalaryGroupComponentsByCTC(EmployeeId, CTCAnnually);

            CalculatedSalaryBreakupDetail calculatedSalaryBreakupDetail = null;
            foreach (var item in salaryComponents)
            {
                if (!string.IsNullOrEmpty(item.ComponentId))
                {
                    calculatedSalaryBreakupDetail = new CalculatedSalaryBreakupDetail();

                    calculatedSalaryBreakupDetail.ComponentId = item.ComponentId;
                    calculatedSalaryBreakupDetail.Formula = item.Formula;
                    calculatedSalaryBreakupDetail.ComponentName = item.ComponentFullName;
                    calculatedSalaryBreakupDetail.ComponentTypeId = item.ComponentTypeId;
                    calculatedSalaryBreakupDetail.FinalAmount = 0;

                    calculatedSalaryBreakupDetails.Add(calculatedSalaryBreakupDetail);
                }
            }

            calculatedSalaryBreakupDetail = new CalculatedSalaryBreakupDetail
            {
                ComponentId = nameof(ComponentNames.Special),
                Formula = null,
                ComponentName = ComponentNames.Special,
                FinalAmount = 0,
                ComponentTypeId = 102
            };

            calculatedSalaryBreakupDetails.Add(calculatedSalaryBreakupDetail);

            calculatedSalaryBreakupDetail = new CalculatedSalaryBreakupDetail
            {
                ComponentId = nameof(ComponentNames.Gross),
                Formula = null,
                ComponentName = ComponentNames.Gross,
                FinalAmount = 0,
                ComponentTypeId = 100
            };

            calculatedSalaryBreakupDetails.Add(calculatedSalaryBreakupDetail);

            calculatedSalaryBreakupDetail = new CalculatedSalaryBreakupDetail
            {
                ComponentId = nameof(ComponentNames.CTC),
                Formula = null,
                ComponentName = ComponentNames.CTC,
                FinalAmount = 0,
                ComponentTypeId = 101
            };

            calculatedSalaryBreakupDetails.Add(calculatedSalaryBreakupDetail);

            return annualSalaryBreakups;
        }

        public EmployeeSalaryDetail GetSalaryBreakupByEmpIdService(long EmployeeId)
        {
            EmployeeSalaryDetail completeSalaryBreakup = _db.Get<EmployeeSalaryDetail>("sp_employee_salary_detail_get_by_empid", new { EmployeeId });
            return completeSalaryBreakup;
        }

        public SalaryGroup GetSalaryGroupByCTC(decimal CTC, long EmployeeId)
        {
            SalaryGroup salaryGroup = _db.Get<SalaryGroup>("sp_salary_group_get_by_ctc", new { CTC, EmployeeId });
            if (salaryGroup == null)
                throw new HiringBellException("Unable to get salary group. Please contact admin");
            return salaryGroup;
        }

        private decimal calculateExpressionUsingInfixDS(string expression, decimal declaredAmount)
        {
            if (string.IsNullOrEmpty(expression))
                return declaredAmount;

            if (!expression.Contains("()"))
                expression = string.Format("({0})", expression);

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
