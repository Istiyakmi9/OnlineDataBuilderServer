using ModalLayer.Modal;
using System;
using System.IO;
using System.Net;
using System.Net.Mail;

namespace EMailService.Service
{
    public class EMailManager : IEMailManager
    {
        private readonly FileLocationDetail _fileLocationDetail;

        public EMailManager(FileLocationDetail fileLocationDetail)
        {
            _fileLocationDetail = fileLocationDetail;
        }

        public string SendMail(EmailSenderModal emailSenderModal)
        {
            string status = string.Empty;
            emailSenderModal.CC.Add("marghub12@gmail.com");
            emailSenderModal.CC.Add("marghub12@rediffmail.com");
            emailSenderModal.BCC.Add("marghub.rahman96@gmail.com");
            emailSenderModal.BCC.Add("marghub12@exdonuts.com");
            var fromEmail = new
            {
                Id = emailSenderModal.From,
                Pwd = "bottomhalf@mi9",
                Host = "smtpout.asia.secureserver.net",
                Port = 587  // 25	                 
            };

            var fromAddress = new MailAddress(fromEmail.Id, emailSenderModal.Title);
            var toAddress = new MailAddress(emailSenderModal.To, emailSenderModal.UserName);

            var smtp = new SmtpClient
            {
                Host = fromEmail.Host,
                Port = fromEmail.Port,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromEmail.Pwd)
            };
            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = emailSenderModal.Title,
                Body = this.GetClientBillingBody(),
                IsBodyHtml = true
            })
            {
                foreach (var emailAddress in emailSenderModal.CC)
                    message.CC.Add(emailAddress);

                foreach (var emailAddress in emailSenderModal.BCC)
                    message.Bcc.Add(emailAddress);

                try
                {
                    foreach (var files in emailSenderModal.FileDetails)
                    {
                        message.Attachments.Add(
                            new Attachment(Path.Combine(_fileLocationDetail.RootPath, files.FilePath, files.FileName + ".pdf"))
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
