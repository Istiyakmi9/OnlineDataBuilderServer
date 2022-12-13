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
        List<AutoCompleteEmployees> EmployeesListDataService(FilterModel filterModel);
        DataSet GetManageEmployeeDetailService(long EmployeeId);
        DataSet GetEmployeeLeaveDetailService(long EmployeeId);
        DataSet LoadMappedClientService(long EmployeeId);
        DataSet GetManageClientService(long EmployeeId);
        DataSet UpdateEmployeeMappedClientDetailService(Employee employee, bool IsUpdating);
        Employee GetEmployeeByIdService(int EmployeeId, int IsActive);
        List<Employee> ActivateOrDeActiveEmployeeService(int EmployeeId, bool IsActive);
        Task<DataSet> RegisterEmployeeService(Employee employee, IFormFileCollection fileCollection);
        Task<DataSet> UpdateEmployeeService(Employee employee, IFormFileCollection fileCollection);
        dynamic GetBillDetailForEmployeeService(FilterModel filterModel);
        string GenerateOfferLetterService(EmployeeOfferLetter employeeOfferLetter);
    }
}
