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
            List<SalaryComponents> salaryComponents = _db.GetList<SalaryComponents>("sp_salary_components_get");
            return salaryComponents;
        }

        public List<SalaryGroup> GetSalaryGroupService()
        {
            List<SalaryGroup> salaryComponents = _db.GetList<SalaryGroup>("sp_salary_group_getAll");
            return salaryComponents;
        }

        public List<SalaryComponents> UpdateSalaryComponentService(List<SalaryComponents> salaryComponents)
        {
            if (salaryComponents.Count > 0)
            {
                List<SalaryComponents> result = _db.GetList<SalaryComponents>("sp_salary_components_get");
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
                                      n.ComponentDescription,
                                      n.CalculateInPercentage,
                                      n.TaxExempt,
                                      n.ComponentTypeId,
                                      n.PercentageValue,
                                      n.MaxLimit,
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
            SalaryGroup salaryGrp = _db.Get<SalaryGroup>("sp_salary_group_getById", new {salaryGroup.SalaryGroupId});
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
            {
                value = new SalaryComponents();
                value.ComponentId = recurringComponent.ComponentName;
                value.ComponentDescription = recurringComponent.ComponentDescription;
                value.MaxLimit = recurringComponent.MaxLimit;
                value.TaxExempt = recurringComponent.TaxExempt;
                value.Section = recurringComponent.Section;
                value.ComponentTypeId = Convert.ToInt32(recurringComponent.Type);
                value.SectionMaxLimit = recurringComponent.SectionMaxLimit;
                value.AdminId = _currentSession.CurrentUserDetail.AdminId;
            }
            else
            {
                value.ComponentId = recurringComponent.ComponentName;
                value.ComponentDescription = recurringComponent.ComponentDescription;
                value.MaxLimit= recurringComponent.MaxLimit;
                value.ComponentTypeId = Convert.ToInt32(recurringComponent.Type);
                value.TaxExempt = recurringComponent.TaxExempt;
                value.Section = recurringComponent.Section;
                value.SectionMaxLimit = recurringComponent.SectionMaxLimit;
                value.AdminId = _currentSession.CurrentUserDetail.AdminId;
            }
            var result = _db.Execute<SalaryComponents>("sp_salary_components_insupd", value, true);
            if (string.IsNullOrEmpty(result))
                throw new HiringBellException("Fail insert salary component.");

            return this.GetSalaryComponentsDetailService();
        }

        public List<SalaryStructure> AddAdhocComponents(SalaryStructure adhocComponent)
        {
            List<SalaryStructure> adhocComponents = null;

            return adhocComponents;
        }

        public List<SalaryStructure> AddDeductionComponents(SalaryStructure deductionComponent)
        {
            List<SalaryStructure> deductionComponents = null;

            return deductionComponents;
        }

        public List<SalaryComponents> AddBonusComponents(SalaryStructure bonusComponent)
        {
            if (string.IsNullOrEmpty(bonusComponent.ComponentName))
                throw new HiringBellException("Invalid component name.");

            List<SalaryComponents> bonuses = _db.GetList<SalaryComponents>("sp_salary_components_get");
            var value = bonuses.Find(x => x.ComponentId == bonusComponent.ComponentName);
            if (value == null)
            {
                value = new SalaryComponents();
                value.ComponentId = bonusComponent.ComponentName;
                value.ComponentDescription = bonusComponent.ComponentDescription;
                value.AdminId = _currentSession.CurrentUserDetail.AdminId;
            }
            else
            {
                value.ComponentId = bonusComponent.ComponentName;
                value.ComponentDescription = bonusComponent.ComponentDescription;
                value.AdminId = _currentSession.CurrentUserDetail.AdminId;
            }
            var result = _db.Execute<SalaryComponents>("sp_salary_components_insupd", value, true);
            if (string.IsNullOrEmpty(result))
                throw new HiringBellException("Fail insert salary component.");

            return this.GetSalaryComponentsDetailService();
        }
    }
}
