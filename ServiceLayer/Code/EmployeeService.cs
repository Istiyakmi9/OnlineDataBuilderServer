using BottomhalfCore.DatabaseLayer.Common.Code;
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
            List<Employee> employees = _commonFilterService.GetSingleModal<Employee>(filterModel, "SP_Employees_Get");
            return employees;
        }
    }
}
