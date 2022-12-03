using ModalLayer.Modal;
using ModalLayer.Modal.Accounts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServiceLayer.Interface
{
    public interface ISalaryComponentService
    {
        SalaryComponents GetSalaryComponentByIdService();
        List<SalaryComponents> GetSalaryComponentsDetailService();
        List<SalaryComponents> UpdateSalaryComponentService(List<SalaryComponents> salaryComponents);
        List<SalaryComponents> InsertUpdateSalaryComponentsByExcelService(List<SalaryComponents> salaryComponents);
        List<SalaryGroup> GetSalaryGroupService(int CompanyId);
        dynamic GetCustomSalryPageDataService(int CompanyId);
        SalaryGroup GetSalaryGroupsByIdService(int SalaryGroupId);
        List<SalaryGroup> AddSalaryGroup(SalaryGroup salaryGroup);
        List<SalaryGroup> UpdateSalaryGroup(SalaryGroup salaryGroup);
        SalaryGroup RemoveAndUpdateSalaryGroupService(string componentId, int groupId);
        List<SalaryComponents> UpdateSalaryGroupComponentService(SalaryGroup salaryGroup);
        List<SalaryComponents> GetSalaryGroupComponents(int salaryGroupId, decimal CTC);
        Task<List<SalaryComponents>> AddUpdateRecurringComponents(SalaryStructure salaryStructure);
        List<SalaryComponents> AddAdhocComponents(SalaryStructure salaryStructure);
        List<SalaryComponents> AddBonusComponents(SalaryStructure salaryStructure);
        List<SalaryComponents> AddDeductionComponents(SalaryStructure salaryStructure);
        string SalaryDetailService(long EmployeeId, List<CalculatedSalaryBreakupDetail> calculatedSalaryBreakupDetail, int PresentMonth, int PresentYear);
        Task<List<AnnualSalaryBreakup>> SalaryBreakupCalcService(long EmployeeId, decimal CTCAnnually);
        EmployeeSalaryDetail GetSalaryBreakupByEmpIdService(long EmployeeId);
        SalaryGroup GetSalaryGroupByCTC(decimal CTC, long EmployeeId);
        List<AnnualSalaryBreakup> CreateSalaryBreakupWithValue(EmployeeCalculation empCal);
    }
}
