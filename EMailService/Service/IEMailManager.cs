using ModalLayer.Modal;
using System;
using System.Collections.Generic;
using System.Text;

namespace EMailService.Service
{
    public interface IEMailManager
    {
        string SendMail(EmailSenderModal emailSenderModal, EmailTemplate EmailTemplateDetail, Employee OtherDetails);
        void ReadMails(EmailSettingDetail emailSettingDetail);
    }
}
