using ModalLayer.Modal;
using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using EAGetMail;
using System.Globalization;

namespace EMailService.Service
{
    public class EMailManager : IEMailManager
    {
        private readonly FileLocationDetail _fileLocationDetail;

        public EMailManager(FileLocationDetail fileLocationDetail)
        {
            _fileLocationDetail = fileLocationDetail;
        }

        public void ReadMails()
        {
            string localInbox = string.Format("{0}\\inbox", Directory.GetCurrentDirectory());
            MailServer oServer = new MailServer("pop.secureserver.net",
                "info@bottomhalf.in",
                "bottomhalf@mi9",
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

        public string SendMail(EmailSenderModal emailSenderModal)
        {
            string status = string.Empty;

            if (emailSenderModal.To == null || emailSenderModal.To.Count == 0)
                throw new HiringBellException("To send email receiver address is mandatory. Receiver address not found.");

            var fromEmail = new
            {
                Id = emailSenderModal.From,
                Pwd = "bottomhalf@mi9",
                Host = "smtpout.asia.secureserver.net",
                Port = 587  // 25	                 
            };

            var fromAddress = new System.Net.Mail.MailAddress(fromEmail.Id, emailSenderModal.Title);

            var smtp = new SmtpClient
            {
                Host = fromEmail.Host,
                Port = fromEmail.Port,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromEmail.Pwd)
            };

            var message = new MailMessage();
            message.Subject = emailSenderModal.Title;
            message.Body = this.GetClientBillingBody();
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
                status = "success";
            }
            catch (Exception ex)
            {
                var _e = ex;
                throw;
            }
            return status;
        }

        private string GetClientBillingBody()
        {
            string htmlBody = @"<!DOCTYPE html>            
                    <html>
                        <head>
                            <title>STAFFING BILL</title> 
                         </head> 
                         <body>
                            <h4>Hi Sir/Madam, </h4> 
                            <p>PFA bill for the month of July.</p> 
                            <p>Developer detail as follows:</p>
                            <div style='margin-left:10px;'>1. FAHIM SHAIKH  [ROLE: SOFTWARE DEVELOPER]</div> 
                            <div style='margin-left:10px;'>2. VANHAR BASHA  [ROLE: SOFTWARE DEVELOPER]</div> 
                            
                            <p style='margin-top: 2rem;'>Thanks & Regards,</p>
                            <div>Team BottomHalf</div>
                            <div>Mob: +91-9100544384</div>
                        </body> 
                    </html>";

            return htmlBody;
        }
    }
}
