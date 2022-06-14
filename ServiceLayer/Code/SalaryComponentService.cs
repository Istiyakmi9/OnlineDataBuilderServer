using BottomhalfCore.DatabaseLayer.Common.Code;
using ModalLayer.Modal;
using ModalLayer.Modal.Accounts;
using ServiceLayer.Interface;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace ServiceLayer.Code
{
    public class SalaryComponentService : ISalaryComponentService
    {
        private readonly IDb _db;
        private readonly CurrentSession _currentSession;
        public SalaryComponentService(IDb db, CurrentSession currentSession)
        {
            _db = db;
            _currentSession = currentSession;
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
                                      n.PercentageValue,
                                      n.MaxLimit,
                                      n.DeclaredValue,
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

        public List<SalaryGroup> AddSalaryGroup(SalaryGroup salaryGroup)
        {
            SalaryGroup salaryGrp = _db.Get<SalaryGroup>("sp_salary_group_getById", new { salaryGroup.SalaryGroupId });
            if (salaryGrp == null)
            {
                salaryGrp = salaryGroup;
                salaryGrp.ComponentId = "[]";
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

            if (string.IsNullOrEmpty(recurringComponent.Type))
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
            value.TaxExempt = recurringComponent.TaxExempt;
            value.Section = recurringComponent.Section;
            value.ComponentTypeId = Convert.ToInt32(recurringComponent.Type);
            value.SectionMaxLimit = recurringComponent.SectionMaxLimit;
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
            SalaryGroup salaryGrp = _db.Get<SalaryGroup>("sp_salary_group_getById", new { salaryGroup.SalaryGroupId });
            if (salaryGrp == null)
                throw new HiringBellException("Salary Group already exist.");
            else
            {
                salaryGrp = salaryGroup;
                salaryGrp.ComponentId = "[]";
                salaryGrp.AdminId = _currentSession.CurrentUserDetail.AdminId;
            }

            var result = _db.Execute<SalaryGroup>("sp_salary_group_insupd", salaryGrp, true);
            if (string.IsNullOrEmpty(result))
                throw new HiringBellException("Fail to insert or update.");
            List<SalaryGroup> value = this.GetSalaryGroupService();
            return value;
        }
    }
}
