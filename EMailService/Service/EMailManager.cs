using ModalLayer.Modal;
using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using EAGetMail;
using System.Globalization;
using System.Text;
using BottomhalfCore.DatabaseLayer.Common.Code;

namespace EMailService.Service
{
    public class EMailManager : IEMailManager
    {
        private readonly FileLocationDetail _fileLocationDetail;
        private readonly IDb _db;

        public EMailManager(FileLocationDetail fileLocationDetail, IDb db)
        {
            _fileLocationDetail = fileLocationDetail;
            _db = db;
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

        private EmailSettingDetail GetSettingDetail()
        {
            var emailSettingDetail = _db.Get<EmailSettingDetail>("sp_email_setting_detail_get", new { EmailSettingDetailId = 0 });
            if (emailSettingDetail == null)
                throw new HiringBellException("Fail to get emaill detail. Please contact to admin.");

            return emailSettingDetail;
        }

        public string SendMail()
        {
            string status = string.Empty;
            EmailSenderModal emailSenderModal = null;
            EmailSettingDetail emailSettingDetail = GetSettingDetail();
            Employee employee = null;

            if (emailSenderModal.To == null || emailSenderModal.To.Count == 0)
                throw new HiringBellException("To send email receiver address is mandatory. Receiver address not found.");

            var fromAddress = new System.Net.Mail.MailAddress(emailSenderModal.From, emailSenderModal.Title);

            var smtp = new SmtpClient
            {
                Host = emailSenderModal.EmailSettingDetails.EmailHost,
                Port = emailSenderModal.EmailSettingDetails.PortNo,
                EnableSsl = emailSenderModal.EmailSettingDetails.EnableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = emailSenderModal.EmailSettingDetails.UserDefaultCredentials,
                Credentials = new NetworkCredential(fromAddress.Address, emailSenderModal.EmailSettingDetails.Credentials)
            };

            var message = new MailMessage();
            message.Subject = emailSenderModal.Title;
            message.Body = this.GetClientBillingBody(employee);
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

        private string GetClientBillingBody(Employee employee)
        {
            StringBuilder firstPhase = new StringBuilder();
            StringBuilder body = new StringBuilder();
            StringBuilder endPhase = new StringBuilder();
            GenerateBillFileDetail emailTemplateDetail = null;
            var emailTemplate = emailTemplateDetail.EmailTemplateDetail;

            body.AppendLine(emailTemplate.BodyContent);

            string style = @"
                    <style>                        
                        .fw-bold {
                            font-weight: 700!important;
                        }

                        .ps-2 {
                            padding-left: 0.5rem!important;
                        }
                        .w-100 {
                            width: 100%!important;
                        }

                        .mt-3 {
                            margin-top: 1rem!important;
                        }
                    </style>
                ";

            string htmlBody = $@"<!DOCTYPE html>            
                    <html>
                        <head>
                            <title>STAFFING BILL</title> 
                            {style}
                         </head> 
                         <body>
                            <div>
                              <div style='margin-top: 1.5rem!important;'>
                                <div style='margin-bottom: 0.25rem!important;'>
                                  {emailTemplate.Salutation}
                                </div>
                                <div style='border: 1px solid #dee2e6!important; padding - top: 1rem!important;
                                    padding-bottom: 1rem!important; padding - right: 0.5rem!important;
                                    padding-left: 0.5rem!important;'>

                                  <div style='flex: 0 0 auto; width: 100%;'>
                                    <div>
                                      <div>
                                        {body.ToString()}
                                      </div> 
                                    </div>
                                  </div>

                                  <div style='flex: 0 0 auto; width: 100%; margin-top: 1.5rem!important;
                                        margin-bottom: 1.5rem!important;'>
                                    <div>
                                      <div>
                                        <div style='display: flex!important;'>
                                          <span style='padding-right 1.5rem !important; padding-left 1.5rem !important; font-weight: 700!important;'>
                                                CANDIDATE NAME: 
                                          </span>
                                          <span>{employee.FirstName + ' ' + employee.LastName}</span>
                                        </div>

                                        <div style='display: flex!important;'>
                                          <span style='padding-right 1.5rem !important; padding-left 1.5rem !important; font-weight: 700!important;'>
                                            CANDIDATE ROLE: 
                                          </span>
                                          <span>SOFTWARE DEVELOPER</span>
                                        </div>
                        
                                        <div style='display: flex!important;'>
                                          <span style='padding-right 1.5rem !important; padding-left 1.5rem !important; font-weight: 700!important;'>
                                            BILLING MONTH: 
                                          </span>
                                          <span>{emailTemplateDetail.MonthName}, {emailTemplateDetail.ForYear}</span>
                                        </div>
                                      </div> 
                                    </div>
                                  </div>

                                </div>
                                <div style='display: flex!important;'>
                                  <div style='font-weight: 700!important;'>Note: </div> 
                                  <div>
                                    {emailTemplate.EmailNote}
                                  </div>
                                </div>
                                <div style='margin-top: 1rem !important;'>
                                  {emailTemplate.EmailClosingStatement}
                                </div>
                                <div>
                                  <div>{emailTemplate.SignatureDetail}</div>
                                </div>
                                <div>
                                  Contact No#:
                                  <div>
                                    +91-{emailTemplate.ContactNo}
                                  </div>
                                </div>
                              </div>
                            </div>
                        </body> 
                    </html>";

            return htmlBody;
        }
    }
}
