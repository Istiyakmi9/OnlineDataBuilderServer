using ModalLayer.Modal;

namespace EMailService.Service
{
    public interface IEMailManager
    {
        string SendMail(EmailSenderModal emailSenderModal);
        void ReadMails(EmailSettingDetail emailSettingDetail);
    }
}
