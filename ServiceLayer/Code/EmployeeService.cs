using BottomhalfCore.DatabaseLayer.Common.Code;
using BottomhalfCore.Services.Code;
using ModalLayer.Modal;
using ServiceLayer.Interface;
using System.Collections.Generic;

namespace ServiceLayer.Code
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IDb _db;
        private readonly CommonFilterService _commonFilterService;
        public EmployeeService(IDb db, CommonFilterService commonFilterService)
        {
            _db = db;
            _commonFilterService = commonFilterService;
        }
        public List<Employee> GetEmployees(FilterModel filterModel)
        {
            List<Employee> employees = _commonFilterService.GetResult<Employee>(filterModel, "SP_Employees_Get");
            return employees;
        }

        public Employee GetEmployeeByIdService(int EmployeeId, bool IsActive)
        {
            Employee employee = default;
            DbParam[] param = new DbParam[]
            {
                new DbParam(EmployeeId, typeof(int), "_EmployeeId"),
                new DbParam(IsActive, typeof(bool), "_IsActive")
            };

            var resultSet = _db.GetDataset("SP_Employees_ById", param);
            if (resultSet.Tables.Count > 0 && resultSet.Tables[0].Rows.Count > 0)
            {
                var emps = Converter.ToList<Employee>(resultSet.Tables[0]);
                if (emps != null && emps.Count > 0)
                    employee = emps[0];
            }
            return employee;
        }
    }
}
