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
        public SalaryComponentService(IDb db)
        {
            _db = db;
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
                                      n.SubComponentTypeId,
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
            }

            else
                throw new HiringBellException("Salary Group already exist.");

            var result = _db.Execute<SalaryGroup>("sp_salary_group_insupd", salaryGrp, true);
            if (string.IsNullOrEmpty(result))
                throw new HiringBellException("Fail to insert or update.");
            List<SalaryGroup> value = this.GetSalaryGroupService();
            return value;
        }

        public List<SalaryStructure> AddRecurringComponents(SalaryStructure salaryStructure)
        {
            List<SalaryStructure> salaryStructures = null;
            return salaryStructures;
        }
    }
}
