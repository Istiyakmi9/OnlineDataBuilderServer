using Microsoft.AspNetCore.Http;
using ModalLayer.Modal;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace ServiceLayer.Interface
{
    public interface IEmployeeService
    {
        List<Employee> GetEmployees(FilterModel filterModel);
        DataSet GetManageEmployeeDetailService(long EmployeeId);
        DataSet UpdateEmployeeDetailService(Employee employee, bool IsUpdating);
        Employee GetEmployeeByIdService(int EmployeeId, bool IsActive);
        string DeleteEmployeeById(int EmployeeId, bool IsActive);
        Task<DataSet> RegisterEmployee(Employee employee, List<AssignedClients> assignedClients, IFormFileCollection fileCollection, bool IsUpdating);
    }
}
