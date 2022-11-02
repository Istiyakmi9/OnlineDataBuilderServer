using ModalLayer.Modal;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServiceLayer.Interface
{
    public interface ICommonService
    {
        List<Employee> LoadEmployeeData();
        bool IsEmptyJson(string json);
        EmailTemplate GetTemplate(int EmailTemplateId);
        Task<EmailSenderModal> ReplaceActualData(TemplateReplaceModal templateReplaceModal, EmailTemplate template);
    }
}
