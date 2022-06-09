using ModalLayer.Modal.Accounts;
using System.Collections.Generic;

namespace ServiceLayer.Interface
{
    public interface ISalaryComponentService
    {
        SalaryComponents GetSalaryComponentByIdService();
        List<SalaryComponents> GetSalaryComponentsDetailService();
        List<SalaryComponents> UpdateSalaryComponentService(List<SalaryComponents> salaryComponents);
        List<SalaryGroup> GetSalaryGroupService();
        List<SalaryGroup> AddSalaryGroup(SalaryGroup salaryGroup);
        List<SalaryStructure> AddRecurringComponents(SalaryStructure salaryStructure);
        List<SalaryStructure> AddAdhocComponents(SalaryStructure salaryStructure);
        List<SalaryStructure> AddBonusComponents(SalaryStructure salaryStructure);
        List<SalaryStructure> AddDeductionComponents(SalaryStructure salaryStructure);


    }
}
