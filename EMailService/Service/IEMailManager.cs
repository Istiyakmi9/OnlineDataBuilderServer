using ModalLayer.Modal;
using System.Threading.Tasks;

namespace EMailService.Service
{
    public interface IEMailManager
    {
        string SendMail(EmailSenderModal emailSenderModal);
        Task SendMailAsync(EmailSenderModal emailSenderModal);
        void ReadMails(EmailSettingDetail emailSettingDetail);
    }
}
