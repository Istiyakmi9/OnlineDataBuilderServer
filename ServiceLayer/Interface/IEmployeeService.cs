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
        Employee GetEmployeeByIdService(int EmployeeId, bool IsActive);
    }
}
