using Microsoft.AspNetCore.Http;
using ModalLayer.Modal;
using ModalLayer.Modal.Accounts;
using ModalLayer.Modal.Leaves;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace ServiceLayer.Interface
{
    public interface IEmployeeService
    {
        List<Employee> GetEmployees(FilterModel filterModel);
        DataSet GetManageEmployeeDetailService(long EmployeeId);
        DataSet GetEmployeeLeaveDetailService(long EmployeeId);
        DataSet LoadMappedClientService(long EmployeeId);
        DataSet GetManageClientService(long EmployeeId);
        DataSet UpdateEmployeeDetailService(Employee employee, bool IsUpdating);
        Employee GetEmployeeByIdService(int EmployeeId, int IsActive);
        List<Employee> ActivateOrDeActiveEmployeeService(int EmployeeId, bool IsActive);
        Task<DataSet> RegisterEmployee(Employee employee, List<AssignedClients> assignedClients, IFormFileCollection fileCollection, bool IsUpdating);
        dynamic GetBillDetailForEmployeeService(FilterModel filterModel);
    }
}
