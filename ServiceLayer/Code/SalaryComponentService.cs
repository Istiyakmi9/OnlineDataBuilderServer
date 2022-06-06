using BottomhalfCore.DatabaseLayer.Common.Code;
using ModalLayer.Modal;
using ModalLayer.Modal.Accounts;
using ServiceLayer.Interface;
using System;
using System.Collections.Generic;

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

        public SalaryComponents AddorUpdateSalaryGroup(SalaryComponents salaryGroup)
        {
            SalaryComponents salaryGrp = _db.Get<SalaryComponents>("sp_salary_group_getById", new {salaryGroup.ComponentId});
            if (salaryGrp == null)
                salaryGrp = salaryGroup;
            else
                throw new HiringBellException("Salary Group already exist.");

            var result = _db.Execute<SalaryComponents>("sp_salary_group_insupd", salaryGrp, true);
            if (string.IsNullOrEmpty(result))
                throw new HiringBellException("Fail to insert or update.");

            return salaryGrp;
        }
    }
}
