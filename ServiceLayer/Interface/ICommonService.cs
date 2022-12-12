using ModalLayer.Modal;
using System.Collections.Generic;

namespace ServiceLayer.Interface
{
    public interface ICommonService
    {
        List<Employee> LoadEmployeeData();
        bool IsEmptyJson(string json);
        EmailTemplate GetTemplate(int EmailTemplateId);
    }
}
