using ModalLayer;
using ModalLayer.Modal;
using System.Threading.Tasks;

namespace EMailService.Service
{
    public interface IEMailManager
    {
        Task SendMailAsync(EmailSenderModal emailSenderModal);
        void ReadMails(EmailSettingDetail emailSettingDetail);
        EmailTemplate GetTemplate(EmailRequestModal emailRequestModal);
    }
}
