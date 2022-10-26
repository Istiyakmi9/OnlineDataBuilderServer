using BottomhalfCore.DatabaseLayer.Common.Code;
using EAGetMail;
using ModalLayer.Modal;
using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace EMailService.Service
{
    public class EMailManager : IEMailManager
    {
        private static EMailManager _instance;
        private static object _lock = new object();
        private readonly FileLocationDetail _fileLocationDetail;
        private EmailSettingDetail _emailSettingDetail;

        private EMailManager(FileLocationDetail fileLocationDetail, IDb _db)
        {
            _fileLocationDetail = fileLocationDetail;
            if (_emailSettingDetail == null)
            {
                _emailSettingDetail = _db.Get<EmailSettingDetail>("sp_email_setting_detail_get",
                    new { EmailSettingDetailId = 0 });
            }
        }

        public static void SetEmailDetail(EmailSettingDetail emailSettingDetail)
        {
            _instance._emailSettingDetail = emailSettingDetail;
        }

        public static EMailManager GetInstance(FileLocationDetail fileLocationDetail, IDb _db)
        {
            if (_instance == null)
                lock (_lock)
                    if (_instance == null)
                        _instance = new EMailManager(fileLocationDetail, _db);

            return _instance;
        }

        public void ReadMails(EmailSettingDetail emailSettingDetail)
        {
            string localInbox = string.Format("{0}\\inbox", Directory.GetCurrentDirectory());
            MailServer oServer = new MailServer("pop.secureserver.net",
                emailSettingDetail.EmailAddress, // "info@bottomhalf.in",
                emailSettingDetail.Credentials, // "bottomhalf@mi9",
                ServerProtocol.Pop3);

            // Enable SSL/TLS connection, most modern email server require SSL/TLS by default
            oServer.SSLConnection = true;
            oServer.Port = 995;

            // if your server doesn't support SSL/TLS, please use the following codes
            // oServer.SSLConnection = false;
            // oServer.Port = 110;

            MailClient oClient = new MailClient("TryIt");
            oClient.Connect(oServer);

            MailInfo[] infos = oClient.GetMailInfos();
            Console.WriteLine("Total {0} email(s)\r\n", infos.Length);
            for (int i = 0; i < infos.Length; i++)
            {
                MailInfo info = infos[i];
                Console.WriteLine("Index: {0}; Size: {1}; UIDL: {2}",
                    info.Index, info.Size, info.UIDL);

                // Receive email from POP3 server
                Mail oMail = oClient.GetMail(info);

                Console.WriteLine("From: {0}", oMail.From.ToString());
                Console.WriteLine("Subject: {0}\r\n", oMail.Subject);

                // Generate an unqiue email file name based on date time.
                string fileName = _generateFileName(i + 1);
                string fullPath = string.Format("{0}\\{1}", localInbox, fileName);

                // Save email to local disk
                oMail.SaveAs(fullPath, true);

                // Mark email as deleted from POP3 server.
                oClient.Delete(info);
            }

            // Quit and expunge emails marked as deleted from POP3 server.
            oClient.Quit();
            Console.WriteLine("Completed!");
        }

        private string _generateFileName(int sequence)
        {
            DateTime currentDateTime = DateTime.Now;
            return string.Format("{0}-{1:000}-{2:000}.eml",
                currentDateTime.ToString("yyyyMMddHHmmss", new CultureInfo("en-US")),
                currentDateTime.Millisecond,
                sequence);
        }

        public Task SendMailAsync(EmailSenderModal emailSenderModal)
        {
            Task.Run(() => Send(emailSenderModal));
            return Task.CompletedTask;
        }

        public string SendMail(EmailSenderModal emailSenderModal)
        {
            return Send(emailSenderModal);
        }

        private string Send(EmailSenderModal emailSenderModal)
        {
            if (emailSenderModal == null || emailSenderModal.To == null || emailSenderModal.To.Count == 0)
                throw new HiringBellException("To send email receiver address is mandatory. Receiver address not found.");

            var fromAddress = new System.Net.Mail.MailAddress(_emailSettingDetail.EmailAddress, emailSenderModal.Title);

            var smtp = new SmtpClient
            {
                Host = _emailSettingDetail.EmailHost,
                Port = _emailSettingDetail.PortNo,
                EnableSsl = _emailSettingDetail.EnableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = _emailSettingDetail.UserDefaultCredentials,
                Credentials = new NetworkCredential(fromAddress.Address, _emailSettingDetail.Credentials)
            };

            var message = new MailMessage();
            message.Subject = emailSenderModal.Subject;
            message.Body = emailSenderModal.Body;
            message.IsBodyHtml = true;
            message.From = fromAddress;

            foreach (var emailAddress in emailSenderModal.To)
                message.To.Add(emailAddress);

            if (emailSenderModal.CC != null && emailSenderModal.CC.Count > 0)
                foreach (var emailAddress in emailSenderModal.CC)
                    message.CC.Add(emailAddress);

            if (emailSenderModal.BCC != null && emailSenderModal.BCC.Count > 0)
                foreach (var emailAddress in emailSenderModal.BCC)
                    message.Bcc.Add(emailAddress);

            try
            {
                if (emailSenderModal.FileDetails != null && emailSenderModal.FileDetails.Count > 0)
                    foreach (var files in emailSenderModal.FileDetails)
                    {
                        message.Attachments.Add(
                            new System.Net.Mail.Attachment(Path.Combine(_fileLocationDetail.RootPath, files.FilePath, files.FileName + ".pdf"))
                        );
                    }

                smtp.Send(message);
            }
            catch (Exception ex)
            {
                var _e = ex;
                throw;
            }
            return ApplicationConstants.Successfull;
        }
    }
}
