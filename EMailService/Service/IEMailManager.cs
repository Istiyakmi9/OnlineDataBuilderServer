using ModalLayer.Modal;

namespace EMailService.Service
{
    public interface IEMailManager
    {
        string SendMail();
        void ReadMails(EmailSettingDetail emailSettingDetail);
    }
}
