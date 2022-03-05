using ModalLayer.Modal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace ServiceLayer.Interface
{
    public interface IEmployeeService
    {
        List<Employee> GetEmployees(FilterModel filterModel);
        DataSet GetManageEmployeeDetailService(long EmployeeId);
        DataSet UpdateEmployeeDetailService(Employee employee, bool IsUpdating);
        Employee GetEmployeeByIdService(int EmployeeId, bool IsActive);
        string DeleteEmployeeById(int EmployeeId, bool IsActive);
    }
}
