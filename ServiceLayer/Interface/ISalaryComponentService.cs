using ModalLayer.Modal.Accounts;
using System.Collections.Generic;

namespace ServiceLayer.Interface
{
    public interface ISalaryComponentService
    {
        SalaryComponents GetSalaryComponentByIdService();
        List<SalaryComponents> GetSalaryComponentsDetailService();
        List<SalaryComponents> UpdateSalaryComponentService(List<SalaryComponents> salaryComponents);
        List<SalaryComponents> InsertUpdateSalaryComponentsByExcelService(List<SalaryComponents> salaryComponents);
        List<SalaryGroup> GetSalaryGroupService();
        List<SalaryGroup> AddSalaryGroup(SalaryGroup salaryGroup);
        List<SalaryGroup> UpdateSalaryGroup(SalaryGroup salaryGroup);
        List<SalaryComponents> UpdateSalaryGroupComponentService(SalaryGroup salaryGroup);
        List<SalaryComponents> GetSalaryGroupComponents(int salaryGroupId);
        List<SalaryComponents> AddUpdateRecurringComponents(SalaryStructure salaryStructure);
        List<SalaryComponents> AddAdhocComponents(SalaryStructure salaryStructure);
        List<SalaryComponents> AddBonusComponents(SalaryStructure salaryStructure);
        List<SalaryComponents> AddDeductionComponents(SalaryStructure salaryStructure);
        string salaryDetailService(long EmployeeId, SalaryBreakup salaryDetail, CompleteSalaryBreakup ComplcompSalaryDetail);
    }
}
