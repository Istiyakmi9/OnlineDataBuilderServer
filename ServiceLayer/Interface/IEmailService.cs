using DocumentFormat.OpenXml.Office2010.ExcelAc;
using Microsoft.AspNetCore.Http;
using ModalLayer.Modal;
using System.Collections.Generic;

namespace ServiceLayer.Interface
{
    public interface IEmailService
    {
        string SendEmailRequestService(EmailSenderModal mailRequest, IFormFileCollection files);
        List<string> GetMyMailService();
        EmailSettingDetail GetEmailSettingByCompIdService(int CompanyId);
        EmailSettingDetail InsertUpdateEmailSettingService(EmailSettingDetail emailSettingDetail);
    }
}
