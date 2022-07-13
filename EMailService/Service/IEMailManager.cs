using System;
using System.Collections.Generic;
using System.Text;

namespace EMailService.Service
{
    public interface IEMailManager
    {
        bool Send();
        void SendMail(string emailId, string userName, string subject, string body);
    }
}
