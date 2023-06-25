using BottomhalfCore.DatabaseLayer.Common.Code;
using BottomhalfCore.Services.Code;
using BottomhalfCore.Services.Interface;
using Microsoft.Extensions.Logging;
using ModalLayer.Modal;
using ModalLayer.Modal.Accounts;
using Newtonsoft.Json;
using ServiceLayer.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using static ApplicationConstants;

namespace ServiceLayer.Code
{
    public class SalaryComponentService : ISalaryComponentService
    {
        private readonly IDb _db;
        private readonly CurrentSession _currentSession;
        private readonly IEvaluationPostfixExpression _postfixToInfixConversion;
        private readonly ITimezoneConverter _timezoneConverter;
        private readonly ILogger<DeclarationService> _logger;

        public SalaryComponentService(IDb db, CurrentSession currentSession,
            IEvaluationPostfixExpression postfixToInfixConversion,
            ITimezoneConverter timezoneConverter,
            ILogger<DeclarationService> logger)
        {
            _db = db;
            _currentSession = currentSession;
            _postfixToInfixConversion = postfixToInfixConversion;
            _timezoneConverter = timezoneConverter;
            _logger = logger;
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

        public async Task<List<SalaryComponents>> UpdateSalaryComponentService(List<SalaryComponents> salaryComponents)
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

                await _db.BulkExecuteAsync("sp_salary_components_insupd", itemOfRows, true);
            }

            return salaryComponents;
        }

        public async Task<List<SalaryComponents>> InsertUpdateSalaryComponentsByExcelService(List<SalaryComponents> salaryComponents)
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

                int count = await _db.BulkExecuteAsync("sp_salary_components_insupd", itemOfRows, true);
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
            ValidateSalaryGroup(salaryGroup);

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
                AdminId = _currentSession.CurrentUserDetail.UserId
            }, true);
            if (string.IsNullOrEmpty(result))
                throw new HiringBellException("Fail to insert or update.");
            List<SalaryGroup> value = this.GetSalaryGroupService(salaryGroup.CompanyId);
            return value;
        }

        private void ValidateSalaryGroup(SalaryGroup salaryGroup)
        {
            if (salaryGroup.CompanyId <= 0)
                throw new HiringBellException("Invalid data selected to create group. Please contact to admin.");

            if (string.IsNullOrEmpty(salaryGroup.GroupName))
                throw HiringBellException.ThrowBadRequest("Salary group name is null or empty");

            if (string.IsNullOrEmpty(salaryGroup.GroupDescription))
                throw HiringBellException.ThrowBadRequest("Salary group description is null or empty");

            if (salaryGroup.MinAmount < 0)
                throw HiringBellException.ThrowBadRequest("Salary group minimum amount is invalid");

            if (salaryGroup.MaxAmount < 0)
                throw HiringBellException.ThrowBadRequest("Salary group maximum amount is invalid");
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

            if (string.IsNullOrEmpty(value.UploadedFileIds))
                value.UploadedFileIds = "[]";

            var result = await _db.ExecuteAsync("sp_salary_components_insupd", new
            {
                value.ComponentId,
                value.ComponentFullName,
                value.ComponentDescription,
                value.CalculateInPercentage,
                value.TaxExempt,
                value.ComponentTypeId,
                value.AcceptedAmount,
                value.RejectedAmount,
                value.UploadedFileIds,
                value.ComponentCatagoryId,
                value.PercentageValue,
                value.MaxLimit,
                value.DeclaredValue,
                value.Formula,
                value.EmployeeContribution,
                value.EmployerContribution,
                value.IncludeInPayslip,
                value.IsAdHoc,
                value.AdHocId,
                value.Section,
                value.SectionMaxLimit,
                value.IsAffectInGross,
                value.RequireDocs,
                value.IsOpted,
                value.IsActive,
                value.AdminId,
            }, true);

            if (string.IsNullOrEmpty(result.statusMessage))
                throw new HiringBellException("Fail insert salary component.");
            else
                await updateSalaryGroupByUdatingComponent(value);

            List<SalaryComponents> salaryComponents = this.GetSalaryComponentsDetailService();
            return salaryComponents;
        }

        private async Task updateSalaryGroupByUdatingComponent(SalaryComponents recurringComponent)
        {
            List<SalaryGroup> salaryGroups = _db.GetList<SalaryGroup>("sp_salary_group_getAll", false);
            if (salaryGroups.Count > 0)
            {
                foreach (var item in salaryGroups)
                {
                    if (string.IsNullOrEmpty(item.SalaryComponents))
                        throw new HiringBellException("Salary component not found");

                    List<SalaryComponents> salaryComponents = JsonConvert.DeserializeObject<List<SalaryComponents>>(item.SalaryComponents);
                    var component = salaryComponents.Find(x => x.ComponentId == recurringComponent.ComponentId);
                    if (component != null)
                    {
                        component.ComponentId = recurringComponent.ComponentId;
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
                    var result = await _db.ExecuteAsync("sp_salary_group_insupd", new
                    {
                        item.SalaryGroupId,
                        item.CompanyId,
                        item.SalaryComponents,
                        item.GroupName,
                        item.GroupDescription,
                        item.MinAmount,
                        item.MaxAmount,
                        AdminId = _currentSession.CurrentUserDetail.UserId
                    }, true);

                    if (string.IsNullOrEmpty(result.statusMessage))
                        throw HiringBellException.ThrowBadRequest("Fail to update salary group. Please contact to admin");
                }


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

        public List<SalaryComponents> AddBonusComponents(SalaryComponents bonusComponent)
        {
            if (string.IsNullOrEmpty(bonusComponent.ComponentId))
                throw new HiringBellException("Invalid component name.");

            List<SalaryComponents> bonuses = _db.GetList<SalaryComponents>("sp_adhoc_detail_get");
            var value = bonuses.Find(x => x.ComponentId == bonusComponent.ComponentId);
            if (value != null)
                throw new HiringBellException("Bonus Component already exist.");

            value = new SalaryComponents();
            value.ComponentId = bonusComponent.ComponentId;
            value.ComponentFullName = bonusComponent.ComponentFullName;
            value.ComponentDescription = bonusComponent.ComponentDescription;
            value.DeclaredValue = bonusComponent.DeclaredValue;
            value.UploadedFileIds = "[]";
            value.IsAdHoc = true;
            value.AdHocId = (int)AdhocType.Bonus;
            value.AdminId = _currentSession.CurrentUserDetail.AdminId;

            var result = _db.Execute<SalaryComponents>("sp_salary_components_insupd", new
            {
                value.ComponentId,
                value.ComponentFullName,
                value.ComponentDescription,
                value.CalculateInPercentage,
                value.TaxExempt,
                value.ComponentTypeId,
                value.AcceptedAmount,
                value.RejectedAmount,
                value.UploadedFileIds,
                value.ComponentCatagoryId,
                value.PercentageValue,
                value.MaxLimit,
                value.DeclaredValue,
                value.Formula,
                value.EmployeeContribution,
                value.EmployerContribution,
                value.IncludeInPayslip,
                value.IsAdHoc,
                value.AdHocId,
                value.Section,
                value.SectionMaxLimit,
                value.IsAffectInGross,
                value.RequireDocs,
                value.IsOpted,
                value.IsActive,
                value.AdminId,
            }, true);

            if (string.IsNullOrEmpty(result))
                throw new HiringBellException("Fail insert salary component.");

            return this.GetSalaryComponentsDetailService();
        }

        public List<SalaryGroup> UpdateSalaryGroup(SalaryGroup salaryGroup)
        {
            List<SalaryGroup> salaryGroups = _db.GetList<SalaryGroup>("sp_salary_group_getAll", false);
            salaryGroups = salaryGroups.Where(x => x.SalaryGroupId != salaryGroup.SalaryGroupId).ToList();
            foreach (SalaryGroup existSalaryGroup in salaryGroups)
            {
                if ((salaryGroup.MinAmount < existSalaryGroup.MinAmount && salaryGroup.MinAmount > existSalaryGroup.MaxAmount) || (salaryGroup.MaxAmount > existSalaryGroup.MinAmount && salaryGroup.MaxAmount < existSalaryGroup.MaxAmount))
                    throw new HiringBellException("Salary group limit already exist");
            }
            SalaryGroup salaryGrp = _db.Get<SalaryGroup>("sp_salary_group_getById", new { salaryGroup.SalaryGroupId });
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
            {
                salaryGroup = GetDefaultSalaryGroup();
                //throw new HiringBellException("Fail to calulate salar detail, salary group not defined for the current package.");
            }

            salaryGroup.GroupComponents = JsonConvert.DeserializeObject<List<SalaryComponents>>(salaryGroup.SalaryComponents);
            return salaryGroup.GroupComponents;
        }

        private SalaryGroup GetDefaultSalaryGroup()
        {
            var result = _db.Get<SalaryGroup>("sp_salary_group_getById", new { SalaryGroupId = 1 });
            if (result == null)
                throw new HiringBellException("Default salry group not found");

            return result;
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
                TaxDetail = employeeSalaryDetail.TaxDetail,
                NewSalaryDetail = employeeSalaryDetail.NewSalaryDetail
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

        private decimal GetTaxExamptedAmount(List<SalaryComponents> salaryComponents)
        {
            _logger.LogInformation("Starting method: GetTaxExamptedAmount");

            decimal finalAmount = 0;
            var taxExamptedComponents = salaryComponents.FindAll(x => x.TaxExempt);
            if (taxExamptedComponents.Count > 0)
            {
                foreach (var item in taxExamptedComponents)
                {
                    finalAmount += this.calculateExpressionUsingInfixDS(item.Formula, item.DeclaredValue);
                }
            }
            _logger.LogInformation("Leaving method: GetTaxExamptedAmount");

            return finalAmount;
        }

        private decimal GetBaiscAmountValue(List<SalaryComponents> salaryComponents, decimal CTC)
        {
            _logger.LogInformation("Starting method: GetBaiscAmountValue");

            decimal finalAmount = 0;
            var basicComponent = salaryComponents.Find(x => x.ComponentId.ToUpper() == ComponentNames.Basic);
            if (basicComponent != null)
            {
                if (!string.IsNullOrEmpty(basicComponent.Formula))
                {
                    if (basicComponent.Formula.Contains(ComponentNames.CTCName))
                        basicComponent.Formula = basicComponent.Formula.Replace(ComponentNames.CTCName, (Convert.ToDecimal(CTC)).ToString());
                }

                finalAmount = this.calculateExpressionUsingInfixDS(basicComponent.Formula, basicComponent.DeclaredValue);
            }
            _logger.LogInformation("Leaving method: GetBaiscAmountValue");

            return finalAmount;
        }

        private async Task<EmployeeCalculation> GetEmployeeSalaryDetail(long EmployeeId, decimal CTCAnnually)
        {
            EmployeeCalculation employeeCalculation = new EmployeeCalculation();

            employeeCalculation.EmployeeId = EmployeeId;
            employeeCalculation.CTC = CTCAnnually;

            var ResultSet = _db.FetchDataSet("sp_salary_components_group_by_employeeid",
                new { employeeCalculation.EmployeeId });
            if (ResultSet == null || ResultSet.Tables.Count != 4)
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
            _logger.LogInformation("Starting method: CreateSalaryBreakupWithValue");

            List<AnnualSalaryBreakup> annualSalaryBreakups = new List<AnnualSalaryBreakup>();
            DateTime startDate = new DateTime(eCal.companySetting.FinancialYear, eCal.companySetting.DeclarationStartMonth, 1, 0, 0, 0, DateTimeKind.Utc);

            eCal.PayrollStartDate = startDate;

            if (eCal.salaryGroup == null || string.IsNullOrEmpty(eCal.salaryGroup.SalaryComponents) || eCal.salaryGroup.SalaryComponents == ApplicationConstants.EmptyJsonArray)
                throw new HiringBellException("Salary group or its component not defined. Please contact to admin.");

            eCal.salaryGroup.GroupComponents = JsonConvert
                .DeserializeObject<List<SalaryComponents>>(eCal.salaryGroup.SalaryComponents);

            decimal taxExamptedComponents = GetTaxExamptedAmount(eCal.salaryGroup.GroupComponents);
            eCal.TaxableCTC = Convert.ToDecimal(eCal.CTC - taxExamptedComponents);
            decimal basicAmountValue = GetBaiscAmountValue(eCal.salaryGroup.GroupComponents, eCal.TaxableCTC);

            int i = 0;
            while (i < eCal.salaryGroup.GroupComponents.Count)
            {
                var item = eCal.salaryGroup.GroupComponents.ElementAt(i);
                if (!string.IsNullOrEmpty(item.Formula))
                {
                    if (item.Formula.Contains(ComponentNames.BasicName))
                        item.Formula = item.Formula.Replace(ComponentNames.BasicName, basicAmountValue.ToString());
                    else if (item.Formula.Contains(ComponentNames.CTCName))
                        item.Formula = item.Formula.Replace(ComponentNames.CTCName, (Convert.ToDecimal(eCal.TaxableCTC)).ToString());
                }

                i++;
            }

            List<AnnualSalaryBreakup> annualSalaryBreakup = null;
            if (!string.IsNullOrEmpty(eCal.employeeSalaryDetail.CompleteSalaryDetail))
                annualSalaryBreakup = JsonConvert.DeserializeObject<List<AnnualSalaryBreakup>>(eCal.employeeSalaryDetail.CompleteSalaryDetail);

            // create salary brackup freshelly
            if (eCal.employee.IsCTCChanged || annualSalaryBreakup == null || annualSalaryBreakup.Count == 0)
            {
                annualSalaryBreakups = CreateFreshSalaryBreakUp(eCal);
            }
            else // update salary brackup
            {
                annualSalaryBreakups = UpdateSalaryBreakUp(eCal, annualSalaryBreakup);
            }

            _logger.LogInformation("Leaving method: CreateSalaryBreakupWithValue");

            return annualSalaryBreakups;
        }

        private List<AnnualSalaryBreakup> UpdateSalaryBreakUp(EmployeeCalculation eCal, List<AnnualSalaryBreakup> annualSalaryBreakup)
        {
            _logger.LogInformation("Starting method: UpdateSalaryBreakUp");

            DateTime doj = _timezoneConverter.ToTimeZoneDateTime(eCal.Doj, _currentSession.TimeZone);
            DateTime startDate = eCal.PayrollStartDate;

            decimal taxableComponentAmount = 0;
            List<CalculatedSalaryBreakupDetail> calculatedSalaryBreakupDetails = GetComponentsDetail(eCal, ref taxableComponentAmount);

            bool IsWorkingDayNotMatchedWithActual = false;
            int daysInMonth = 0;
            int workingDays = 0;
            decimal monthlyGrossIncome = 0;
            int pendingPayrolls = annualSalaryBreakup.Count(x => !x.IsPayrollExecutedForThisMonth);
            foreach (var salary in annualSalaryBreakup)
            {
                if (!salary.IsPayrollExecutedForThisMonth)
                {
                    List<CalculatedSalaryBreakupDetail> otherDetail = new List<CalculatedSalaryBreakupDetail>();

                    monthlyGrossIncome = 0;
                    daysInMonth = DateTime.DaysInMonth(startDate.Year, startDate.Month);
                    workingDays = daysInMonth;
                    if (startDate.Subtract(doj).TotalDays < 0 && startDate.Month != doj.Month)
                    {
                        IsWorkingDayNotMatchedWithActual = true;
                    }
                    else if (startDate.Month == doj.Month)
                    {
                        workingDays = daysInMonth - doj.Day + 1;
                        IsWorkingDayNotMatchedWithActual = false;
                    }
                    else
                    {
                        IsWorkingDayNotMatchedWithActual = false;
                    }

                    monthlyGrossIncome = eCal.TaxableCTC / 12;
                    if (monthlyGrossIncome < taxableComponentAmount)
                        throw HiringBellException.ThrowBadRequest("Invalid calculation. Gross amount must be greater than or equals to the sum of other components.");


                    var component = calculatedSalaryBreakupDetails.Find(x => x.Formula == ApplicationConstants.AutoCalculation);
                    if (component != null)
                    {
                        component.FinalAmount = (monthlyGrossIncome - taxableComponentAmount);
                    }

                    //var finalSpecialAmount = (monthlyGrossIncome - taxableComponentAmount);


                    //var calculatedSalaryBreakupDetail = new CalculatedSalaryBreakupDetail
                    //{
                    //    ComponentId = nameof(ComponentNames.Special),
                    //    Formula = null,
                    //    ComponentName = ComponentNames.Special,
                    //    FinalAmount = finalSpecialAmount,
                    //    ComponentTypeId = 102,
                    //    IsIncludeInPayslip = true
                    //};

                    //otherDetail.Add(calculatedSalaryBreakupDetail);

                    var finalMonthlyAmount = IsWorkingDayNotMatchedWithActual ? monthlyGrossIncome : (monthlyGrossIncome / daysInMonth) * workingDays;
                    var calculatedSalaryBreakupDetail = new CalculatedSalaryBreakupDetail
                    {
                        ComponentId = nameof(ComponentNames.Gross),
                        Formula = null,
                        ComponentName = ComponentNames.Gross,
                        FinalAmount = finalMonthlyAmount,
                        ComponentTypeId = 100,
                        IsIncludeInPayslip = true
                    };

                    otherDetail.Add(calculatedSalaryBreakupDetail);

                    var finalMonthlyCTC = IsWorkingDayNotMatchedWithActual ? eCal.CTC / 12 : ((eCal.CTC / 12) / daysInMonth) * workingDays;
                    calculatedSalaryBreakupDetail = new CalculatedSalaryBreakupDetail
                    {
                        ComponentId = nameof(ComponentNames.CTC),
                        Formula = null,
                        ComponentName = ComponentNames.CTC,
                        FinalAmount = finalMonthlyCTC,
                        ComponentTypeId = 101,
                        IsIncludeInPayslip = true
                    };

                    otherDetail.Add(calculatedSalaryBreakupDetail);
                    otherDetail.AddRange(calculatedSalaryBreakupDetails);

                    salary.SalaryBreakupDetails = otherDetail;
                }

                startDate = startDate.AddMonths(1);
            }
            _logger.LogInformation("Leaving method: UpdateSalaryBreakUp");

            return annualSalaryBreakup;
        }

        private CalculatedSalaryBreakupDetail ResolvEMPPFForumulaAmount(EmployeeCalculation eCal)
        {
            SalaryComponents employerPFComponents = eCal.salaryGroup.GroupComponents.Where(x => x.ComponentId == ComponentNames.EmployerPF).FirstOrDefault();
            if (employerPFComponents.ComponentId == null)
                throw HiringBellException.ThrowBadRequest("EmployerPR component is not defined. Please add one.");

            decimal amount = this.calculateExpressionUsingInfixDS(employerPFComponents.Formula, employerPFComponents.DeclaredValue);

            CalculatedSalaryBreakupDetail calculatedSalaryBreakupDetail = new CalculatedSalaryBreakupDetail();
            calculatedSalaryBreakupDetail.ComponentId = employerPFComponents.ComponentId;
            calculatedSalaryBreakupDetail.Formula = employerPFComponents.Formula;
            calculatedSalaryBreakupDetail.ComponentName = employerPFComponents.ComponentFullName;
            calculatedSalaryBreakupDetail.ComponentTypeId = employerPFComponents.ComponentTypeId;
            calculatedSalaryBreakupDetail.FinalAmount = amount / 12;
            calculatedSalaryBreakupDetail.IsIncludeInPayslip = employerPFComponents.IncludeInPayslip;

            return calculatedSalaryBreakupDetail;
        }

        private List<CalculatedSalaryBreakupDetail> GetComponentsDetail(EmployeeCalculation eCal, ref decimal taxableComponentAmount)
        {
            _logger.LogInformation("Starting method: GetComponentsDetail");

            decimal expectedGrossIncome = 0;
            List<CalculatedSalaryBreakupDetail> calculatedSalaryBreakupDetails = new List<CalculatedSalaryBreakupDetail>();

            decimal amount = 0;
            var taxableComponents = eCal.salaryGroup.GroupComponents.Where(x => x.TaxExempt == false);
            foreach (var item in taxableComponents)
            {
                if (!string.IsNullOrEmpty(item.ComponentId) && item.Formula != ApplicationConstants.AutoCalculation)
                    amount = this.calculateExpressionUsingInfixDS(item.Formula, item.DeclaredValue);
                else
                    amount = 0;

                CalculatedSalaryBreakupDetail calculatedSalaryBreakupDetail = new CalculatedSalaryBreakupDetail();
                calculatedSalaryBreakupDetail.ComponentId = item.ComponentId;
                calculatedSalaryBreakupDetail.Formula = item.Formula;
                calculatedSalaryBreakupDetail.ComponentName = item.ComponentFullName;
                calculatedSalaryBreakupDetail.ComponentTypeId = item.ComponentTypeId;
                expectedGrossIncome += amount;
                calculatedSalaryBreakupDetail.FinalAmount = amount > 0 ? amount / 12 : 0;
                calculatedSalaryBreakupDetail.IsIncludeInPayslip = item.IncludeInPayslip;

                taxableComponentAmount += calculatedSalaryBreakupDetail.FinalAmount;
                calculatedSalaryBreakupDetails.Add(calculatedSalaryBreakupDetail);
            }

            // calculatedSalaryBreakupDetails.Add(ResolvEMPPFForumulaAmount(eCal));
            _logger.LogInformation("Endning method: GetComponentsDetail");

            return calculatedSalaryBreakupDetails;
        }

        private List<AnnualSalaryBreakup> CreateFreshSalaryBreakUp(EmployeeCalculation eCal)
        {
            _logger.LogInformation("Starting method: CreateFreshSalaryBreakUp");

            List<AnnualSalaryBreakup> annualSalaryBreakups = new List<AnnualSalaryBreakup>();
            DateTime doj = _timezoneConverter.ToTimeZoneDateTime(eCal.Doj, _currentSession.TimeZone);
            DateTime startDate = eCal.PayrollStartDate;

            int index = 0;
            bool IsJoinedInMiddleOfCalendar = false;
            decimal taxableComponentAmount = 0;
            while (index < 12)
            {
                List<CalculatedSalaryBreakupDetail> otherDetails = new List<CalculatedSalaryBreakupDetail>();

                IsJoinedInMiddleOfCalendar = false;

                // checking if joined in middle of calendar year. i.e Joined in Aug but cycle is from April, then previous month of Aug will be zeo initially.
                if (startDate.Subtract(doj).TotalDays < 0 && startDate.Month != doj.Month)
                {
                    IsJoinedInMiddleOfCalendar = true;
                }

                taxableComponentAmount = 0;
                List<CalculatedSalaryBreakupDetail> calculatedSalaryBreakupDetails = GetComponentsDetail(eCal, ref taxableComponentAmount);

                decimal monthlyGrossIncome = eCal.TaxableCTC / 12;
                if (monthlyGrossIncome < taxableComponentAmount)
                    throw HiringBellException.ThrowBadRequest("Invalid calculation. Gross amount must be greater than or equals to the sum of other components.");

                var component = calculatedSalaryBreakupDetails.Find(x => x.Formula == ApplicationConstants.AutoCalculation);
                if (component != null)
                {
                    component.FinalAmount = (monthlyGrossIncome - taxableComponentAmount);
                }

                //var finalSpecialAmount = (monthlyGrossIncome - taxableComponentAmount);

                //var calculatedSalaryBreakupDetail = new CalculatedSalaryBreakupDetail
                //{
                //    ComponentId = nameof(ComponentNames.Special),
                //    Formula = null,
                //    ComponentName = ComponentNames.Special,
                //    FinalAmount = finalSpecialAmount,
                //    ComponentTypeId = 102,
                //    IsIncludeInPayslip = true
                //};

                //otherDetails.Add(calculatedSalaryBreakupDetail);
                var calculatedSalaryBreakupDetail = new CalculatedSalaryBreakupDetail
                {
                    ComponentId = nameof(ComponentNames.Gross),
                    Formula = null,
                    ComponentName = ComponentNames.Gross,
                    FinalAmount = monthlyGrossIncome,
                    ComponentTypeId = 100,
                    IsIncludeInPayslip = true
                };

                otherDetails.Add(calculatedSalaryBreakupDetail);
                calculatedSalaryBreakupDetail = new CalculatedSalaryBreakupDetail
                {
                    ComponentId = nameof(ComponentNames.CTC),
                    Formula = null,
                    ComponentName = ComponentNames.CTC,
                    FinalAmount = eCal.CTC / 12,
                    ComponentTypeId = 101,
                    IsIncludeInPayslip = true
                };

                otherDetails.Add(calculatedSalaryBreakupDetail);
                otherDetails.AddRange(calculatedSalaryBreakupDetails);


                annualSalaryBreakups.Add(new AnnualSalaryBreakup
                {
                    MonthName = startDate.ToString("MMM"),
                    IsPayrollExecutedForThisMonth = IsJoinedInMiddleOfCalendar,
                    MonthNumber = startDate.Month,
                    MonthFirstDate = startDate,
                    IsActive = !IsJoinedInMiddleOfCalendar,
                    SalaryBreakupDetails = otherDetails
                });


                startDate = startDate.AddMonths(1);
                index++;
            }
            _logger.LogInformation("Leaving method: CreateFreshSalaryBreakUp");

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
                    calculatedSalaryBreakupDetail.IsIncludeInPayslip = item.IncludeInPayslip;

                    calculatedSalaryBreakupDetails.Add(calculatedSalaryBreakupDetail);
                }
            }

            calculatedSalaryBreakupDetail = new CalculatedSalaryBreakupDetail
            {
                ComponentId = nameof(ComponentNames.Special),
                Formula = null,
                ComponentName = ComponentNames.Special,
                FinalAmount = 0,
                ComponentTypeId = 102,
                IsIncludeInPayslip = true
            };

            calculatedSalaryBreakupDetails.Add(calculatedSalaryBreakupDetail);

            calculatedSalaryBreakupDetail = new CalculatedSalaryBreakupDetail
            {
                ComponentId = nameof(ComponentNames.Gross),
                Formula = null,
                ComponentName = ComponentNames.Gross,
                FinalAmount = 0,
                ComponentTypeId = 100,
                IsIncludeInPayslip = true
            };

            calculatedSalaryBreakupDetails.Add(calculatedSalaryBreakupDetail);

            calculatedSalaryBreakupDetail = new CalculatedSalaryBreakupDetail
            {
                ComponentId = nameof(ComponentNames.CTC),
                Formula = null,
                ComponentName = ComponentNames.CTC,
                FinalAmount = 0,
                ComponentTypeId = 101,
                IsIncludeInPayslip = true
            };

            calculatedSalaryBreakupDetails.Add(calculatedSalaryBreakupDetail);

            return annualSalaryBreakups;
        }

        public dynamic GetSalaryBreakupByEmpIdService(long EmployeeId)
        {
            (EmployeeSalaryDetail completeSalaryBreakup, UserDetail userDetail) = _db.GetMulti<EmployeeSalaryDetail, UserDetail>("sp_employee_salary_detail_get_by_empid", new { EmployeeId });
            return new { completeSalaryBreakup, userDetail };
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
            _logger.LogInformation("Starting method: calculateExpressionUsingInfixDS");

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
            _logger.LogInformation("Leaving method: calculateExpressionUsingInfixDS");

            return _postfixToInfixConversion.evaluatePostfix(exp);
        }

        public List<SalaryComponents> GetBonusComponentsService()
        {
            List<SalaryComponents> result = _db.GetList<SalaryComponents>("sp_salary_components_get");
            result = result.FindAll(x => x.IsAdHoc == true && x.AdHocId == (int)AdhocType.Bonus);
            return result;
        }

        public DataSet GetAllSalaryDetailService(FilterModel filterModel)
        {
            if (string.IsNullOrEmpty(filterModel.SearchString))
            {
                filterModel.SearchString = $"1=1 and e.CompanyId = {_currentSession.CurrentUserDetail.CompanyId}";
            }
            else
            {
                filterModel.SearchString += $" and e.CompanyId = {_currentSession.CurrentUserDetail.CompanyId}";
            }

            var result = _db.FetchDataSet("sp_employee_salary_detail_getbyFilter", filterModel);
            result.Tables[0].TableName = "SalaryDetail";
            return result;
        }
    }
}
